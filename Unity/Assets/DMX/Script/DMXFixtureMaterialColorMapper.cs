using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DMXFixture))]
public class DMXFixtureMaterialColorMapper: MonoBehaviour {
	public Material sourceMaterial;
	
	public enum ColorToMapType { 
		R,
		G,
		B,
		RGB
	}

	public ColorToMapType colorToMap = ColorToMapType.RGB;
	
	[Range(0, 511)]
	public int rChannelOffset = 0;

	[Range(0, 511)]
	public int gChannelOffset = 1;
	
	[Range(0, 511)]
	public int bChannelOffset = 2;

	void Update () {
		DMXFixture fixture = GetComponent<DMXFixture>();
		
		if (sourceMaterial) {
			switch (colorToMap) {
			case ColorToMapType.R:
				fixture.SetChannelData(rChannelOffset, (byte)(sourceMaterial.color.r * 255));
				break;

			case ColorToMapType.G:
				fixture.SetChannelData(gChannelOffset, (byte)(sourceMaterial.color.g * 255));
				break;

			case ColorToMapType.B:
				fixture.SetChannelData(bChannelOffset, (byte)(sourceMaterial.color.b * 255));
				break;
				
			case ColorToMapType.RGB:
				fixture.SetChannelData(rChannelOffset, (byte)(sourceMaterial.color.r * 255));
				fixture.SetChannelData(gChannelOffset, (byte)(sourceMaterial.color.g * 255));
				fixture.SetChannelData(bChannelOffset, (byte)(sourceMaterial.color.b * 255));
				break;
			}
		}
	}
}
