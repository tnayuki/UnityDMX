using UnityEngine;
using System.Collections;

public class DMXFixture : MonoBehaviour {
	public DMXController controller;
	
	[Range(1, 512)]
	public int startAddress = 1;

	public void SetChannelData(int offset, byte data) {
		controller.SetChannelData(startAddress + offset, data);
	}
}

