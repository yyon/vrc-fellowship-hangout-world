using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public enum MirrorMode {
	Off, Cutout, Full
}

public enum MirrorResolution {
	X256, X512, X1024, Auto
}

public enum MirrorAutoOff {
	Near, Far, Off
}

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MirrorGlobal : UdonSharpBehaviour {
	[HideInInspector] public MirrorPanel[] panels = new MirrorPanel[0];

	public MirrorMode mode;
	public MirrorResolution resolution;
	public float opacity;
	public MirrorAutoOff autoOff;

	public float nearAutoOffMeters = 3;
	public float farAutoOffMeters = 7;

	public Material mirrorMaterial;
	private float[] autoOffFadeStart;
	private float[] autoOffFadeLength;

	public float updateIntervalSeconds = 0.5f;

	public Transform mirrorContainer;
	public GameObject mirrorObjectX256;
	public GameObject mirrorObjectX512;
	public GameObject mirrorObjectX1024;
	public GameObject mirrorObjectAuto;
	public GameObject mirrorObjectX256Environment;
	public GameObject mirrorObjectX512Environment;
	public GameObject mirrorObjectX1024Environment;
	public GameObject mirrorObjectAutoEnvironment;

	[HideInInspector] public int activeMirror = -1;

	public void Start() {
		SendCustomEventDelayedFrames(nameof(DelayedStart), 10);
	}

	public void DelayedStart() {
		UpdateNearFarAutoOff(nearAutoOffMeters, farAutoOffMeters);
		DidUpdate();
	}

	private bool updateQueued = false;
	public void UpdateLoop() {
		if(mode == MirrorMode.Off) {
			updateQueued = false;
			return;
		}

		for(int i = 0; i < panels.Length; i++) {
			panels[i].nearCollider.DefintiveCheck();
			panels[i].farCollider.DefintiveCheck();
		}
		UpdateActive();
		SendCustomEventDelayedSeconds(nameof(UpdateLoop), updateIntervalSeconds);
	}

	public void UpdateActive() {
		int bestIndex = -1;
		float bestDistance = float.PositiveInfinity;
		Vector3 playerPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
		Vector3 playerNormal = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;
		for(int i = 0; i < panels.Length; i++) {
			if(!panels[i].renderCheck.willRender) continue;

			Vector3 mirrorClosestPoint = panels[i].renderCheck.viewCollider.ClosestPoint(playerPosition);
			Vector3 distanceVector = playerPosition - mirrorClosestPoint;
			distanceVector.y *= 25;
			float distance = distanceVector.magnitude;

			Transform mirror = panels[i].renderCheck.transform;
			Vector3 mirrorNormal = mirror.rotation * -Vector3.forward;
			float dirAlign = Vector3.Dot(playerNormal, -mirrorNormal);

			distance *= 4 - dirAlign;

			if(distance < bestDistance) {
				bestIndex = i;
				bestDistance = distance;
			}
		}

		SetActive(bestIndex);
	}

	public void registerForUpdates(MirrorPanel udonBehaviour) {
		Utils.Append(ref panels, udonBehaviour);
	}

	public void UpdateNearFarAutoOff(float nearMeters, float farMeters) {
		nearAutoOffMeters = nearMeters;
		farAutoOffMeters = farMeters;
		for(int i = 0; i < panels.Length; i++) {
			panels[i].nearCollider.collider.size = new Vector3(1, 1, nearMeters);
			panels[i].nearCollider.collider.center = new Vector3(0, 0, nearMeters / 2);
			panels[i].nearCollider.DefintiveCheck();

			panels[i].farCollider.collider.size = new Vector3(1, 1, farMeters);
			panels[i].farCollider.collider.center = new Vector3(0, 0, farMeters / 2);
			panels[i].farCollider.DefintiveCheck();
		}

		autoOffFadeStart = new float[] { nearAutoOffMeters * 0.8f, farAutoOffMeters * 0.8f, 9999999 };
		autoOffFadeLength = new float[] { nearAutoOffMeters * 0.2f, farAutoOffMeters * 0.2f, 9999999 };
	}

	public void SetActive(int newActiveMirror) {
		if(newActiveMirror != activeMirror && activeMirror != -1) {
			panels[activeMirror].mirrorStatus.color = panels[activeMirror].buttonsOff;
		}

		activeMirror = newActiveMirror;

		bool mirrorOn = activeMirror != -1;
		if(mirrorOn && autoOff == MirrorAutoOff.Near && !panels[activeMirror].nearCollider.isInCollider) mirrorOn = false;
		if(mirrorOn && autoOff == MirrorAutoOff.Far && !panels[activeMirror].farCollider.isInCollider) mirrorOn = false;
		if(opacity == 0) mirrorOn = false;

		mirrorContainer.gameObject.SetActive(mirrorOn);

		if(activeMirror != -1) {
			MirrorPanel panel = panels[activeMirror];
			mirrorContainer.SetParent(panel.mirrorContainer, false);
			panel.mirrorStatus.color = mirrorOn ? panel.buttonsOn : panel.buttonsOff;
		}
	}

	public void DidUpdate() {
		if(mode != MirrorMode.Off && !updateQueued) {
			updateQueued = true;
			UpdateLoop();
		}
		if(mode == MirrorMode.Off) SetActive(-1);
		else SetActive(activeMirror);
		mirrorMaterial.SetFloat("_Transparency", 1 - opacity);
		mirrorMaterial.SetFloat("_DistanceFade", autoOffFadeStart[(int)autoOff]);
		mirrorMaterial.SetFloat("_DistanceFadeLength", autoOffFadeLength[(int)autoOff]);
		mirrorMaterial.SetFloat("_HideBackground", mode == MirrorMode.Cutout ? 1 : 0);

		mirrorObjectX256.SetActive(mode == MirrorMode.Cutout && resolution == MirrorResolution.X256);
		mirrorObjectX512.SetActive(mode == MirrorMode.Cutout && resolution == MirrorResolution.X512);
		mirrorObjectX1024.SetActive(mode == MirrorMode.Cutout && resolution == MirrorResolution.X1024);
		mirrorObjectAuto.SetActive(mode == MirrorMode.Cutout && resolution == MirrorResolution.Auto);
		mirrorObjectX256Environment.SetActive(mode == MirrorMode.Full && resolution == MirrorResolution.X256);
		mirrorObjectX512Environment.SetActive(mode == MirrorMode.Full && resolution == MirrorResolution.X512);
		mirrorObjectX1024Environment.SetActive(mode == MirrorMode.Full && resolution == MirrorResolution.X1024);
		mirrorObjectAutoEnvironment.SetActive(mode == MirrorMode.Full && resolution == MirrorResolution.Auto);

		foreach(MirrorPanel panel in panels) panel.OnUpdate();
	}
}
