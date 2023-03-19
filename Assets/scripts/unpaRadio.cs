
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class unpaRadio : UdonSharpBehaviour
{
	public Transform paRadioReset;

	public void paStart() {
		Networking.GetOwner(gameObject).SetVoiceDistanceFar(0);
	}

	public void paEnd() {
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
