using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class VJDMX512Controller : MonoBehaviour {
	public byte[] channelData = new byte[512];
	
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
	
	void PatchingThread() {
		while (true) {
			GCHandle channelDataHandle = GCHandle.Alloc(channelData, GCHandleType.Pinned);
			SendChannelData(channelDataHandle.AddrOfPinnedObject());
			channelDataHandle.Free();
		}
	}
}
