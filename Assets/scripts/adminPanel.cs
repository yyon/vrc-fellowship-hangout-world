
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;
using System.Reflection;
using CentauriCore.Blackjack;
using TMPro;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class adminPanel : UdonSharpBehaviour
{
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int tab = 0;
	[UdonSynced(UdonSyncMode.None)] public bool performanceModeOnDefault = false;
	[UdonSynced(UdonSyncMode.None)] public bool stagePlayer = false;
	[UdonSynced(UdonSyncMode.None)] public int spawnPosition = 0;
	private string[] users = new string[0];
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public string[] admins = new string[0];
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int userSelect = 0;
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int adminSelect = 0;

	[HideInInspector] public bool isOwner = false;
	private int userCount = 0;
	private int adminCount = 0;

	private bool isInitialLoad = true;
	public performanceMode performanceModeToggle;
	public Transform worldSpawn;
	public Transform entranceSpawn;
	public Transform basementSpawn;
	public GameObject stagePlayerGameObject;
	public GameObject respawnButton;

	public GameObject notAdminUI;
	public GameObject isAdminUI;
	public TextMeshProUGUI notAdminInfo;
	public VRCPickup paRadio;
	public Image Tab0;
	public Image Tab1;
	public GameObject Tab0Container;
	public GameObject Tab1Container;
	public Toggle perfModeToggle;
	public Dropdown spawnDropdown;
	public Toggle stagePlayerToggle;
	public TextMeshProUGUI userList;
	public RectTransform userSelectBox;
	public TextMeshProUGUI adminList;
	public RectTransform adminSelectBox;

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

	public static bool Contains<T>(T[] array, T item) {
		return Array.IndexOf(array, item) >= 0;
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
		return Contains(admins, requestedOwner.displayName);
	}

	public void ClickTab0() { ClickTab(0); }
	public void ClickTab1() { ClickTab(1); }
	public void ClickTab(int index) {
		tab = index;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void perfModeChange() {
		performanceModeOnDefault = perfModeToggle.isOn;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
	}

	public void spawnChange() {
		spawnPosition = spawnDropdown.value;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void stagePlayerChange() {
		stagePlayer = stagePlayerToggle.isOn;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void userUp() {
		if(userSelect == 0) return;
		userSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void userDown() {
		if(userSelect >= userCount - 1) return;
		userSelect++;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void userMove() {
		if(users.Length <= 0) return;
		Append(ref admins, users[userSelect]);
		if(userSelect > 0) userSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void adminUp() {
		if(adminSelect <= 0) return;
		adminSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void adminDown() {
		if(adminSelect >= adminCount - 1) return;
		adminSelect++;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public void adminMove() {
		if(admins.Length == 1) return;
		Remove(ref admins, adminSelect);
		if(adminSelect > 0) adminSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateFromData();
	}

	public override void OnDeserialization() {
		if(isInitialLoad) {
			isInitialLoad = false;
			if(spawnPosition == 1) Networking.LocalPlayer.TeleportTo(basementSpawn.position, basementSpawn.rotation);
			if(performanceModeToggle.performanceModeOn != performanceModeOnDefault) {
				performanceModeToggle.Interact();
			}
		}

		updateFromData();
	}

	public void updateFromData() {
		bool isAdmin = Contains(admins, Networking.LocalPlayer.displayName);
		respawnButton.SetActive(isAdmin);
		notAdminUI.SetActive(!isAdmin);
		isAdminUI.SetActive(isAdmin);
		paRadio.pickupable = isAdmin;

		if(spawnPosition == 0) {
			worldSpawn.position = entranceSpawn.position;
			worldSpawn.rotation = entranceSpawn.rotation;
		}
		else if(spawnPosition == 1) {
			worldSpawn.position = basementSpawn.position;
			worldSpawn.rotation = basementSpawn.rotation;
		}

		Tab0.color = new Color(1, 1, 1, (tab == 0) ? 0.4f : 0.15f);
		Tab1.color = new Color(1, 1, 1, (tab == 1) ? 0.4f : 0.15f);

		Tab0Container.SetActive(tab == 0);
		Tab1Container.SetActive(tab == 1);

		perfModeToggle.isOn = performanceModeOnDefault;
		spawnDropdown.value = spawnPosition;
		stagePlayerToggle.isOn = stagePlayer;

		stagePlayerGameObject.SetActive(stagePlayer);

		userCount = VRCPlayerApi.GetPlayerCount() - admins.Length;
		adminCount = admins.Length;

		VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
		players = VRCPlayerApi.GetPlayers(players);

		string userStr = "", adminStr = "";
		foreach(VRCPlayerApi player in players) {
			if(Contains(admins, player.displayName)) {
				adminStr += player.displayName + "\n";
			}
			else {
				Append(ref users, player.displayName);
				userStr += player.displayName + "\n";
			}
		}

		userList.text = userStr;
		adminList.text = adminStr;
		userSelectBox.anchoredPosition = new Vector2(0, -13.75f * userSelect);
		adminSelectBox.anchoredPosition = new Vector2(0, -13.75f * adminSelect);

		notAdminInfo.text = "You are not an admin. Ask one of these admins to add you...\n" + adminStr;
	}
}