
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class paRadio : UdonSharpBehaviour
{
	public Transform paRadioReset;

	public void paStart() {
		Networking.GetOwner(gameObject).SetVoiceVolumetricRadius(1000);
		Networking.GetOwner(gameObject).SetVoiceDistanceNear(1000000);
		Networking.GetOwner(gameObject).SetVoiceDistanceFar(1000000);
	}

	public void paEnd() {
		Networking.GetOwner(gameObject).SetVoiceVolumetricRadius(0);
		Networking.GetOwner(gameObject).SetVoiceDistanceNear(0);
		Networking.GetOwner(gameObject).SetVoiceDistanceFar(25);
		transform.position = paRadioReset.position;
		transform.rotation = paRadioReset.rotation;
	}

	public override void OnPickup() {
		SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "paStart");
	}

	public override void OnDrop() {
		SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "paEnd");
	}
}
