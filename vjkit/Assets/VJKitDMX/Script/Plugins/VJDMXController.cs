using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class VJDMXController : MonoBehaviour {
	private byte[] channelData = new byte[512];
	
	[DllImport ("UnityOpenDMXUSBPlugin")] 
	private static extern void Initialize(); 

	[DllImport ("UnityOpenDMXUSBPlugin")] 
	private static extern void SendChannelData(IntPtr data); 
	
	// Use this for initialization
	void Start () {
		Initialize();
		
		new Thread(PatchingThread).Start();
	}
	
	// Update is called once per frame
	void Update () {
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
			lock(channelData) { 
				Buffer.BlockCopy(channelData, 0, buffer, 0, 512);
			}

			GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			SendChannelData(bufferHandle.AddrOfPinnedObject());
			bufferHandle.Free();
		}
	}

}
