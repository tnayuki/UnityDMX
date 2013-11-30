using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class VJDMXController : MonoBehaviour {
	public bool blackOut;
	
	// Debug Window	properties
	protected static int windowIDSeed = 20000;
	protected Rect windowRect = new Rect(160, 20, 120, 50);
	protected int windowId;
	public bool debugWindow = true;
	
	protected int page = 0;
	
	private Thread patchingThread;
	private byte[] channelData = new byte[512];
	
	[DllImport ("UnityOpenDMXUSBPlugin")] 
	private static extern void Initialize(); 

	[DllImport ("UnityOpenDMXUSBPlugin")] 
	private static extern void SendChannelData(IntPtr data); 

	public virtual void Awake() {
		windowRect.x = PlayerPrefs.GetFloat("vjkit.window.pos." + gameObject.name + ".x", Screen.width - 400.0f);
		windowRect.y = PlayerPrefs.GetFloat("vjkit.window.pos." + gameObject.name + ".y", 20.0f);
		debugWindow  = 1 == PlayerPrefs.GetInt("vjkit.window." + gameObject.name + ".debug", 1);
		windowRect.width = 400;
		windowId = windowIDSeed;
		windowIDSeed++;
	}
	
	protected virtual void OnDrawGUIWindow(int windowID) {
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		
		string[] pages= new string[8];
		for (int i = 0; i < pages.Length; i++) {
			pages[i] = string.Format("{0}-{1}", i * 64 + 1, i * 64 + 64);
		}
		page = GUILayout.SelectionGrid(page, pages, pages.Length);
		
		blackOut = GUILayout.Toggle(blackOut, "BO");
		GUILayout.EndHorizontal();
		
		for (int i = 0; i < 2; i++) {
			GUILayout.BeginHorizontal();
			
			for (int j = 0; j < 32; j++) {
				float num = channelData[page * 64 + i * 32 + j] / 255.0f;
				num = GUILayout.VerticalSlider(num, 1.0f, 0.0f);
				channelData[page * 64 + i * 32 + j] = (byte)(num * 255.0f);
			}

			GUILayout.EndHorizontal();
		}

		GUI.DragWindow();
		GUILayout.EndVertical();
	}
	
	public virtual void OnGUI() {
		if(debugWindow) {
			windowRect = GUILayout.Window(windowId, windowRect, OnDrawGUIWindow, name, GUILayout.Width(200));
		}
	}
	
	public virtual void OnApplicationQuit() {
		PlayerPrefs.SetFloat("vjkit.window.pos." + gameObject.name + ".x", windowRect.x);
		PlayerPrefs.SetFloat("vjkit.window.pos." + gameObject.name + ".y", windowRect.y);
		PlayerPrefs.SetInt("vjkit.window." + gameObject.name + ".debug", (debugWindow? 1:0));
	}

	// Use this for initialization
	void Start () {
		Initialize();
		
		patchingThread = new Thread(PatchingThread);
		patchingThread.Start();
	}
	
	void OnDestroy() {
        Debug.Log("Script was destroyed");
		
		patchingThread.Abort();
    }
	
	public void SetChannelData(int channel, float value) {
		channel = Mathf.Clamp(channel, 1, 512);
		byte data = (byte)Mathf.Clamp(255.0f * value, 0.0f, 255.0f);
		
		lock (channelData) {
			channelData[channel - 1] = data;
		}
	}

	protected void PatchingThread() {
		byte[] buffer = new byte[512];
		
		while (true) {
			if (!blackOut) {
				lock(channelData) { 
					Buffer.BlockCopy(channelData, 0, buffer, 0, 512);
				}
			}

			GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			SendChannelData(bufferHandle.AddrOfPinnedObject());
			bufferHandle.Free();
		}
	}

}
