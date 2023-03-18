using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AdminList : UdonSharpBehaviour {
	[HideInInspector] public string[] users = new string[0];
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public string[] admins = new string[0];
	[HideInInspector] public string userListStr = "";
	[HideInInspector] public string adminListStr = "";

	private bool isOwner = false;

	public adminPanel panel;

	public static void Append<T>(ref T[] array, T item) {
		T[] nArray = new T[array.Length + 1];
		Array.Copy(array, nArray, array.Length);
		nArray[array.Length] = item;
		array = nArray;
	}

	public static void Remove<T>(ref T[] array, int index) {
		T[] nArray = new T[array.Length - 1];
		Array.Copy(array, 0, nArray, 0, index - 1);
		Array.Copy(array, index + 1, nArray, index, array.Length - 1 - index);
		array = nArray;
	}

	public bool containsAdmin(string user) {
		return Array.IndexOf(admins, user) >= 0;
	}

	public void addAdmin(string user) {
		Append(ref admins, user);
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void removeAdmin(int index) {
		Remove(ref admins, index);
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	void Start() {
		isOwner = Networking.GetOwner(gameObject) == Networking.LocalPlayer;
		if(isOwner) {
			Append(ref admins, Networking.LocalPlayer.displayName);
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
			bool change = false, selectChange = false;
			int index = Array.IndexOf(admins, player.displayName);
			if(index >= 0) {
				Remove(ref admins, index);
				if(admins.Length == 0) Append(ref admins, Networking.LocalPlayer.displayName);
				change = true;
			}

			if(panel.adminSelect == admins.Length && panel.adminSelect != 0) {
				panel.adminSelect--;
				selectChange = true;
			}

			if(panel.userSelect == VRCPlayerApi.GetPlayerCount() && panel.userSelect != 0) {
				panel.userSelect--;
				selectChange = true;
			}

			if(change) {
				RequestSerialization();
			}

			if(selectChange) {
				Networking.SetOwner(Networking.LocalPlayer, panel.gameObject);
				panel.RequestSerialization();
			}
		}

		OnDeserialization();
	}

	public override void OnDeserialization() {
		VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
		players = VRCPlayerApi.GetPlayers(players);

		userListStr = "";
		adminListStr = "";
		foreach(VRCPlayerApi player in players) {
			if(containsAdmin(player.displayName)) {
				adminListStr += player.displayName + "\n";
			}
			else {
				Append(ref users, player.displayName);
				userListStr += player.displayName + "\n";
			}
		}

		panel.OnDeserialization();
	}
}
