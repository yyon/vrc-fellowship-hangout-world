using CentauriCore.Blackjack;
using Thry;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PrivateRoom : UdonSharpBehaviour {
	public Image lockImg1;
	public Image lockImg2;
	public Color lockOpen;
	public Color lockClosed;
	public Collider doorCollider1;
	public Collider doorCollider2;
	public TextMeshPro text;
	public Collider checkInsideCollider;
	[UdonSynced(UdonSyncMode.None)] public bool isOpen = true;

	public void Start() {
		ManualUpdate();
	}

	public void ManualUpdate() {
		string playerNames = "";
		VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
		foreach(var player in VRCPlayerApi.GetPlayers(players)) {
			Vector3 pos = player.GetPosition();
			if(pos != checkInsideCollider.ClosestPoint(pos)) continue;

			playerNames += "\n" + player.displayName;
		}

		text.text = "In Room:" + playerNames;
		if(Networking.IsOwner(Networking.LocalPlayer, gameObject) && playerNames == "" && isOpen == false) LockToggle();
		SendCustomEventDelayedSeconds(nameof(ManualUpdate), 10);
	}

	public void LockToggle() {
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		isOpen = !isOpen;
		RequestSerialization();
		OnDeserialization();
	}

	public override void OnDeserialization() {
		lockImg1.color = isOpen ? lockOpen : lockClosed;
		lockImg2.color = isOpen ? lockOpen : lockClosed;
		doorCollider1.enabled = isOpen;
		doorCollider2.enabled = isOpen;
	}
}
