using UnityEngine;
using System.Collections;

public class DMXFixtureFixedMapper : MonoBehaviour {
	public int channelOffset = 0;
	public byte data = 0;
	
	void OnEnable() {
		DMXFixture fixture = GetComponent<DMXFixture>();
		
		fixture.SetChannelData(channelOffset, data);
	}
	
	void OnDisable() {
		DMXFixture fixture = GetComponent<DMXFixture>();
		
		fixture.SetChannelData(channelOffset, 0);
	}
}
