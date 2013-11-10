using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
	
[ExecuteInEditMode()]
public class VJDMX512Controller : MonoBehaviour {
	public string address = "255.255.255.255";
	public int port = 6454;
	public int universe = 0;
	public byte[] channelData = new byte[512];
	
	private byte[] oldChannelData = new byte[512];
	
	private UdpClient udpClient = new UdpClient();
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (!channelData.SequenceEqual(oldChannelData)) {
			ArtNetSend();
			
			Buffer.BlockCopy(channelData, 0, oldChannelData, 0, 512);
		}
	}
	
	void ArtNetSend () {
        byte[] dgram = new byte[512 + 18];
        dgram[0] = (byte)'A';
        dgram[1] = (byte)'r';
        dgram[2] = (byte)'t';
        dgram[3] = (byte)'-';
        dgram[4] = (byte)'N';
        dgram[5] = (byte)'e';
        dgram[6] = (byte)'t';
        dgram[7] = 0;
        dgram[8] = 0;
        dgram[9] = 0x50;
        dgram[10] = 0;
        dgram[11] = 14;
        dgram[12] = 0;
        dgram[13] = 0;
        dgram[14] = 0;
        dgram[15] = 0;
        dgram[16] = 0;
        dgram[17] = 6;
		
		Buffer.BlockCopy(channelData, 0, dgram, 18, 512);

		udpClient.Send(dgram, dgram.Length, address, port);
	}
}
