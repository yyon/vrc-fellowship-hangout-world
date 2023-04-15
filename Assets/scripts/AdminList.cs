using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AdminList : UdonSharpBehaviour {
	[UdonSynced(UdonSyncMode.None)] public bool allAdmins = true;
	[HideInInspector] public string[] users = new string[0];
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public string[] admins = new string[0];
	[HideInInspector] public string userListStr = "";
	[HideInInspector] public string adminListStr = "";

	public bool isAdmin = false;
	private bool isOwner = false;

	public AdminPanel adminPanel;
	public WorldClock[] clocks;

	public bool containsAdmin(string user) {
		if(user == null) return false;
		return allAdmins || Array.IndexOf(admins, user) >= 0;
	}

	public void addAdmin(string user) {
		Utils.Append(ref admins, user);
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void removeAdmin(int index) {
		Utils.Remove(ref admins, index);
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void setAllAdmins(bool value) {
		if(allAdmins && !value) {
			admins = new string[VRCPlayerApi.GetPlayerCount()];
			VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
			players = VRCPlayerApi.GetPlayers(players);
			for(int i = 0; i < players.Length; i++) {
				admins[i] = players[i].displayName;
			}
		}

		allAdmins = value;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void Start() {
		isOwner = Networking.GetOwner(gameObject) == Networking.LocalPlayer;
		if(isOwner) {
			Utils.Append(ref admins, Networking.LocalPlayer.displayName);
			RequestSerialization();
			OnDeserialization();
		}
	}

	public override void OnOwnershipTransferred(VRCPlayerApi newPlayer) {
		isOwner = newPlayer == Networking.LocalPlayer;
	}

	public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner) {
		return containsAdmin(requestedOwner.displayName);
	}

	public override void OnPlayerJoined(VRCPlayerApi player) {
		OnDeserialization();
	}

	public override void OnPlayerLeft(VRCPlayerApi player) {
		if(isOwner) {
			int index = Array.IndexOf(admins, player.displayName);
			if(index >= 0) {
				Utils.Remove(ref admins, index);
				if(admins.Length == 0) Utils.Append(ref admins, Networking.LocalPlayer.displayName);
				RequestSerialization();
			}
		}

		OnDeserialization();
	}

	public override void OnDeserialization() {
		isAdmin = containsAdmin(Networking.LocalPlayer.displayName);

		VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
		players = VRCPlayerApi.GetPlayers(players);

		users = new string[0];
		userListStr = "";
		adminListStr = "";
		foreach(VRCPlayerApi player in players) {
			if(!containsAdmin(player.displayName)) {
				Utils.Append(ref users, player.displayName);
				userListStr += player.displayName + "\n";
			}
			else {
				adminListStr += player.displayName + "\n";
			}
		}

		adminPanel.OnAdminListUpdate();
		foreach(WorldClock clock in clocks) clock.OnAdminListUpdate();
	}
}
