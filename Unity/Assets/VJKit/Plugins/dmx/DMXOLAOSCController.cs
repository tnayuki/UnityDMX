using UnityEngine;

public class DMXOLAOSCController : DMXAbstractController {
	public string serverHostname = "127.0.0.1";
	public int serverPort = 7770;
	
	private OSCClient oscClient;
	
	void OnEnable() {
		oscClient = new OSCClient(serverHostname, serverPort);
	}
	
	void OnDisable() {
		oscClient = null;
	}

	protected override void SendChannelData(byte[] channelData) {
		oscClient.SendSimpleMessage("/dmx/universe/0", channelData);
	}
}
