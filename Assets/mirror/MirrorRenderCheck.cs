using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(Renderer))]//
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MirrorRenderCheck : UdonSharpBehaviour {
	public MirrorPanel panel;
	private bool preWillRender = false;
	private bool internalWillRender = false;
	[HideInInspector] public bool willRender = false;
	[HideInInspector] public Collider viewCollider;

	public void Start() {
		viewCollider = GetComponent<Collider>();
	}

	public void SetOn(bool on) {
		gameObject.SetActive(on);
	}

	public void Update() {
		willRender = internalWillRender;

		if(willRender) {
			Transform mirror = transform;
			Vector3 playerPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
			Vector3 mirrorToPlayerNormal = Vector3.Normalize(playerPosition - mirror.position);
			Vector3 mirrorNormal = mirror.rotation * -Vector3.forward;

			//behind mirror
			if(Vector3.Dot(mirrorToPlayerNormal, mirrorNormal) < -0.05) willRender = false;
		}

		if(preWillRender != willRender) {
			panel.global.UpdateActive();
		}

		preWillRender = willRender;
		internalWillRender = false;
	}

	public void OnWillRenderObject() {
		internalWillRender = true;
	}
}
