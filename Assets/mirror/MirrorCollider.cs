using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(Collider))]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MirrorCollider : UdonSharpBehaviour {
	public MirrorPanel panel;
	[HideInInspector] public bool isInCollider;
	[HideInInspector] public new BoxCollider collider;

	public void Start() {
		collider = GetComponent<BoxCollider>();
	}

	public override void OnPlayerTriggerEnter(VRCPlayerApi player) {
		if(!player.isLocal) return;
		isInCollider = true;
		panel.global.SetActive(panel.global.activeMirror);
	}

	public override void OnPlayerTriggerExit(VRCPlayerApi player) {
		if(!player.isLocal) return;
		isInCollider = false;
		panel.global.SetActive(panel.global.activeMirror);
	}

	public void DefintiveCheck() {
		Vector3 localPosition = collider.transform.InverseTransformPoint(Networking.LocalPlayer.GetPosition()) - collider.center;
		bool _isInCollider = Mathf.Abs(localPosition.x) < collider.size.x / 2
			&& Mathf.Abs(localPosition.y) < collider.size.y / 2
			&& Mathf.Abs(localPosition.z) < collider.size.z / 2;
		Debug.Log(panel.name + " " + localPosition);

		//bool _isInCollider = collider.bounds.Contains(Networking.LocalPlayer.GetPosition());
		if(isInCollider != _isInCollider) {
			isInCollider = _isInCollider;
			panel.global.SetActive(panel.global.activeMirror);
		}
	}
}
