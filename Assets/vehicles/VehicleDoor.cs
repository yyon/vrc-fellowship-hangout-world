
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class VehicleDoor : UdonSharpBehaviour {
	public Animator animator;

	[UdonSynced(UdonSyncMode.None)] private bool openState = true;

	public void Interact() {
		if(openState) close();
		else open();
	}

	public void open() {
		if(animator.GetCurrentAnimatorStateInfo(0).IsName("Closed")) {
			animator.SetTrigger("Toggle");
			openState = true;
			Networking.SetOwner(Networking.LocalPlayer, gameObject);
			RequestSerialization();
		}
	}

	public void close() {
		if(animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) {
			animator.SetTrigger("Toggle");
			openState = false;
			Networking.SetOwner(Networking.LocalPlayer, gameObject);
			RequestSerialization();
		}
	}

	public void OnDeserialization() {
		if(animator.GetCurrentAnimatorStateInfo(0).IsName("Opened") != openState) animator.SetTrigger("Toggle");
	}
}
