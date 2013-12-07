using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class DMXController : MonoBehaviour {
	public bool blackOut;
	
	private bool showSliders;
	
	// Debug Window	properties
	protected static int windowIDSeed = 20000;
	protected Rect windowRect = new Rect(160, 20, 120, 50);
	protected int windowId;
	public bool debugWindow = true;
	
	protected int page = 0;
	
	private byte[] channelData = new byte[512];
	private byte[] blackOutChannelData = new byte[512];
	
	[DllImport ("UnityOpenDMXUSBPlugin")] 
	private static extern void InitializePlugin(); 

	[DllImport ("UnityOpenDMXUSBPlugin")] 
	private static extern void DeinitializePlugin(); 

	[DllImport ("UnityOpenDMXUSBPlugin")] 
	private static extern void UpdateBuffer(IntPtr data); 

	public virtual void Awake() {
		windowRect.x = PlayerPrefs.GetFloat("dmx.window.pos." + gameObject.name + ".x", Screen.width - 400.0f);
		windowRect.y = PlayerPrefs.GetFloat("dmx.window.pos." + gameObject.name + ".y", 20.0f);
		debugWindow = 1 == PlayerPrefs.GetInt("dmx.window." + gameObject.name + ".debug", 1);
		showSliders = 1 == PlayerPrefs.GetInt("dmx.window." + gameObject.name + ".showSliders", 0);
		windowRect.width = 400;
		windowId = windowIDSeed;
		windowIDSeed++;
	}
	
	protected virtual void OnDrawGUIWindow(int windowID) {
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		
		showSliders = GUILayout.Toggle(showSliders, "Show sliders");
		blackOut = GUILayout.Toggle(blackOut, "Black out");

		GUILayout.EndHorizontal();
		
		if (showSliders) {
			string[] pages= new string[8];
			for (int i = 0; i < pages.Length; i++) {
				pages[i] = string.Format("{0}-{1}", i * 64 + 1, i * 64 + 64);
			}
			page = GUILayout.SelectionGrid(page, pages, pages.Length);
			
			for (int i = 0; i < 2; i++) {
				GUILayout.BeginHorizontal();
				
				for (int j = 0; j < 32; j++) {
					float num = channelData[page * 64 + i * 32 + j] / 255.0f;
					num = GUILayout.VerticalSlider(num, 1.0f, 0.0f);
					channelData[page * 64 + i * 32 + j] = (byte)(num * 255.0f);
				}
	
				GUILayout.EndHorizontal();
			}
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
		PlayerPrefs.SetFloat("dmx.window.pos." + gameObject.name + ".x", windowRect.x);
		PlayerPrefs.SetFloat("dmx.window.pos." + gameObject.name + ".y", windowRect.y);
		PlayerPrefs.SetInt("dmx.window." + gameObject.name + ".debug", (debugWindow ? 1 : 0));
		PlayerPrefs.SetInt("dmx.window." + gameObject.name + ".showSliders", (showSliders ? 1 : 0));
	}

	void OnEnable() {
		InitializePlugin();
	}
	
	void OnDisable() {
		DeinitializePlugin();
    }

	void LateUpdate() {
		if (!blackOut) {
			GCHandle channelDataHandle = GCHandle.Alloc(channelData, GCHandleType.Pinned);
			UpdateBuffer(channelDataHandle.AddrOfPinnedObject());
			channelDataHandle.Free();
		} else {
			GCHandle blackOutChannelDataHandle = GCHandle.Alloc(blackOutChannelData, GCHandleType.Pinned);
			UpdateBuffer(blackOutChannelDataHandle.AddrOfPinnedObject());
			blackOutChannelDataHandle.Free();
		}
	}
	
	public void SetChannelData(int channel, byte data) {
		channel = Mathf.Clamp(channel, 1, 512);
		
		channelData[channel - 1] = data;
	}
}
