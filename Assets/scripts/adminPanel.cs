
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;
using TMPro;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class adminPanel : UdonSharpBehaviour {
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int tab = 0;
	[UdonSynced(UdonSyncMode.None)] public bool performanceModeOnDefault = false;
	[UdonSynced(UdonSyncMode.None)] public bool stagePlayer = false;
	[UdonSynced(UdonSyncMode.None)] public int spawnPosition = 0;
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int userSelect = 0;
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int adminSelect = 0;

	[HideInInspector] public bool isOwner = false;

	private bool isInitialLoad = true;
	public performanceMode performanceModeToggle;
	public Transform worldSpawn;
	public Transform entranceSpawn;
	public Transform basementSpawn;
	public GameObject stagePlayerGameObject;
	public GameObject respawnButton;
	public AdminList adminList;
	public VRCPickup handlePickup;

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
	public TextMeshProUGUI userListGUI;
	public RectTransform userSelectBox;
	public TextMeshProUGUI adminListGUI;
	public RectTransform adminSelectBox;

	void Start() {
		isOwner = Networking.GetOwner(gameObject) == Networking.LocalPlayer;
		if(isOwner) OnDeserialization();
	}

	public override void OnOwnershipTransferred(VRCPlayerApi newPlayer) {
		isOwner = newPlayer == Networking.LocalPlayer;
	}

	public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner) {
		return adminList.containsAdmin(requestedOwner.displayName);
	}

	public void ClickTab0() { ClickTab(0); }
	public void ClickTab1() { ClickTab(1); }
	public void ClickTab(int index) {
		tab = index;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void perfModeChange() {
		performanceModeOnDefault = perfModeToggle.isOn;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void spawnChange() {
		spawnPosition = spawnDropdown.value;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void stagePlayerChange() {
		stagePlayer = stagePlayerToggle.isOn;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void userUp() {
		if(userSelect == 0) return;
		userSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void userDown() {
		if(userSelect >= adminList.users.Length - 1) return;
		userSelect++;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void userMove() {
		if(adminList.users.Length == 0) return;
		adminList.addAdmin(adminList.users[userSelect]);
		if(userSelect > 0) {
			userSelect--;
			Networking.SetOwner(Networking.LocalPlayer, gameObject);
			RequestSerialization();
			OnDeserialization();
		}
	}

	public void adminUp() {
		if(adminSelect <= 0) return;
		adminSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void adminDown() {
		if(adminSelect >= adminList.admins.Length - 1) return;
		adminSelect++;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void adminMove() {
		if(adminList.admins.Length == 1) return;
		adminList.removeAdmin(adminSelect);
		if(adminSelect > 0) {
			adminSelect--;
			Networking.SetOwner(Networking.LocalPlayer, gameObject);
			RequestSerialization();
			OnDeserialization();
		}
	}

	public override void OnDeserialization() {
		if(isInitialLoad) {
			isInitialLoad = false;
			if(spawnPosition == 1) Networking.LocalPlayer.TeleportTo(basementSpawn.position, basementSpawn.rotation);
			if(performanceModeToggle.performanceModeOn != performanceModeOnDefault) {
				performanceModeToggle.Interact();
			}
		}

		bool isAdmin = adminList.containsAdmin(Networking.LocalPlayer.displayName);
		respawnButton.SetActive(isAdmin);
		notAdminUI.SetActive(!isAdmin);
		isAdminUI.SetActive(isAdmin);
		paRadio.pickupable = isAdmin;
		handlePickup.pickupable = isAdmin;

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

		userListGUI.text = adminList.userListStr;
		adminListGUI.text = adminList.adminListStr;
		userSelectBox.anchoredPosition = new Vector2(0, -13.75f * userSelect);
		adminSelectBox.anchoredPosition = new Vector2(0, -13.75f * adminSelect);

		notAdminInfo.text = "You are not an admin. Ask one of these admins to add you...\n" + adminList.adminListStr;
	}
}