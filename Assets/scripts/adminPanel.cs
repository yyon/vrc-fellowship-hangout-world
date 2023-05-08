using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;
using TMPro;
using VRC.SDK3.Components;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AdminPanel : UdonSharpBehaviour {
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int tab = 0;
	[UdonSynced(UdonSyncMode.None)] public bool performanceModeOnDefault = false;
	[UdonSynced(UdonSyncMode.None)] public int spawnPosition = 0;
	[UdonSynced(UdonSyncMode.None)] public bool stagePlayer = false;
	[UdonSynced(UdonSyncMode.None)] public bool easterEggHuntEnabled = false;
	[UdonSynced(UdonSyncMode.None)] public bool birthdayDecorationsEnabled = false;
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int userSelect = 0;
	[HideInInspector][UdonSynced(UdonSyncMode.None)] public int adminSelect = 0;

	[HideInInspector] public bool isOwner = false;

	private bool isInitialLoad = true;
	public TimeCore timeCore;
	public Transform worldSpawn;
	public Transform entranceSpawn;
	public Transform basementSpawn;
	public GameObject stagePlayerGameObject;
	private Vector3 stagePlayerLocation = Vector3.zero;
	public GameObject panelRespawnButton;
	public AdminList adminList;
	public VRCPickup handlePickup;
	public GameObject easterEggHunt;
	public GameObject birthdayDecorations;

	public Color offColor;
	public Color onColor;

	public GameObject notAdminUI;
	public GameObject isAdminUI;
	public TextMeshProUGUI notAdminInfo;
	public VRCPickup paRadio;
	public Image allAdminsImage;
	public TextMeshProUGUI allAdminsText;
	public Image Tab0;
	public Image Tab1;
	public Image Tab2;
	public Image Tab3;
	public GameObject Tab0Container;
	public GameObject Tab1Container;
	public GameObject Tab2Container;
	public GameObject Tab3Container;

	public Image perfModeImage;
	public TextMeshProUGUI perfModeText;
	public TextMeshProUGUI spawnText;
	public Image stagePlayerImage;
	public TextMeshProUGUI stagePlayerText;
	public Image easterEggHuntImage;
	public TextMeshProUGUI easterEggHuntText;
	public Image birthdayDecorationsImage;
	public TextMeshProUGUI birthdayDecorationsText;

	public TextMeshProUGUI userListGUI;
	public RectTransform userSelectBox;
	public TextMeshProUGUI adminListGUI;
	public RectTransform adminSelectBox;

	public Slider seasonSlider;
	public Slider dayNightSpeedSlider;
	public TextMeshProUGUI dayNightSpeedLabel;
	public Slider weatherSlider;
	public Image seasonChangeImage;
	public TextMeshProUGUI seasonChangeText;
	public Image dayNightChangeImage;
	public TextMeshProUGUI dayNightChangeText;
	public Image weatherChangeImage;
	public TextMeshProUGUI weatherChangeText;

	public TextMeshProUGUI worldDebugInfoText;
	public TextMeshProUGUI worldDebugLogText;
	private string logText = "";
	private DateTime logLastBlockTime = DateTime.Now;
	private string logLastBlockText = "World Init";
	private int logLastBlockCount = 1;
	private string logCurrentBlockText = "";
	private bool logUpdateQueued = false;

	void Start() {
		Log("AdminPanel Start");
		timeCore.registerForUpdates(this);
		isOwner = Networking.GetOwner(gameObject) == Networking.LocalPlayer;
		if(isOwner) OnDeserialization();
	}

	public override void OnOwnershipTransferred(VRCPlayerApi newPlayer) {
		isOwner = newPlayer == Networking.LocalPlayer;
	}

	public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner) {
		return adminList.containsAdmin(requestedOwner.displayName);
	}

	private string LogTime(DateTime time) {
		return String.Format("[{0,0:00}/{1,0:00}/{2,0:00} - {3,0:00}:{4,0:00}:{5,0:00}]", time.Year + 800, time.Month, time.Day, time.Hour, time.Minute, time.Second);
	}

	private string LogEntry(DateTime date, int count, string text) {
		string[] textArray = text.Split(
			new string[] { "\r\n", "\r", "\n" },
			StringSplitOptions.RemoveEmptyEntries
		);

		string logEntryText = "";
		string header = LogTime(date) + " ";
		if(count != 1) header += "(" + count + ") ";

		for(int i = 0; i < textArray.Length; i++) {
			if(i == 0 || count == 1) logEntryText += header;
			else logEntryText += new string(' ', header.Length);
			logEntryText += textArray[i] + '\n';
		}

		return logEntryText;
	}

	public void logUpdate() {
		if(logLastBlockText != logCurrentBlockText || (DateTime.Now - logLastBlockTime).TotalSeconds > 5) {
			logText += LogEntry(logLastBlockTime, logLastBlockCount, logLastBlockText);

			logLastBlockTime = DateTime.Now;
			logLastBlockText = logCurrentBlockText;
			logLastBlockCount = 0;
		}

		logLastBlockCount++;

		worldDebugLogText.text = logText + LogEntry(logLastBlockTime, logLastBlockCount, logLastBlockText);

		logCurrentBlockText = "";
		logUpdateQueued = false;
	}

	public void Log(string text) {
		logCurrentBlockText += text + "\n";

		if(!logUpdateQueued) {
			logUpdateQueued = true;
			SendCustomEventDelayedFrames(nameof(logUpdate), 0, VRC.Udon.Common.Enums.EventTiming.Update);
		}
	}

	public void allAdminsChange() {
		adminList.setAllAdmins(!adminList.allAdmins);
	}

	public void ClickTab0() { ClickTab(0); }
	public void ClickTab1() { ClickTab(1); }
	public void ClickTab2() { ClickTab(2); }
	public void ClickTab3() { ClickTab(3); }
	public void ClickTab(int index) {
		tab = index;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	//Tab 0

	public void perfModeChange() {
		performanceModeOnDefault = !performanceModeOnDefault;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void spawnChange() {
		spawnPosition = (spawnPosition + 1) % 2;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void stagePlayerChange() {
		stagePlayer = !stagePlayer;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void easterEggHuntChange() {
		easterEggHuntEnabled = !easterEggHuntEnabled;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void birthdayDecorationsChange() {
		birthdayDecorationsEnabled = !birthdayDecorationsEnabled;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	//Tab 1

	public void userUp() {
		if(userSelect <= 0) return;
		userSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void userDown() {
		if(userSelect >= adminList.users.Length - 1) return;
		userSelect++;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void userMove() {
		if(adminList.users.Length == 0) return;
		userSelect--;
		adminList.addAdmin(adminList.users[userSelect + 1]);
	}

	public void adminUp() {
		if(adminSelect <= 0) return;
		adminSelect--;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void adminDown() {
		if(adminSelect >= adminList.admins.Length - 1) return;
		adminSelect++;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		updateData();
	}

	public void adminMove() {
		if(adminList.admins.Length == 1) return;
		adminSelect--;
		adminList.removeAdmin(adminSelect + 1);
	}

	//Tab 2

	public void seasonChange() {
		Log("AdminPanel seasonChange");
		timeCore.setSeasonNetworked(timeCore.isSeasonChange(), seasonSlider.value);
	}

	public void dayNightSpeedChange() {
		Log("AdminPanel dayNightSpeedChange");
		timeCore.setDayNightLengthNetworked(dayNightSpeedSlider.value);
	}

	public void weatherChange() {
		Log("AdminPanel weatherChange");
		timeCore.setWeatherNetworked(false, weatherSlider.value);
	}

	public void seasonChangeChange() {
		Log("AdminPanel seasonChangeChange");
		timeCore.setSeasonNetworked(!timeCore.isSeasonChange());
	}

	public void dayNightChangeChange() {
		Log("AdminPanel dayNightChangeChange");
		timeCore.setDayNightNetworked(!timeCore.isDayNightChange());
	}

	public void weatherChangeChange() {
		Log("AdminPanel weatherChangeChange");
		timeCore.setWeatherNetworked(!timeCore.isWeatherChange());
	}

	public override void OnDeserialization() {
		if(isInitialLoad) {
			isInitialLoad = false;
			if(spawnPosition == 0) Networking.LocalPlayer.TeleportTo(entranceSpawn.position, entranceSpawn.rotation);
			else if(spawnPosition == 1) Networking.LocalPlayer.TeleportTo(basementSpawn.position, basementSpawn.rotation);

			if(timeCore.localPerformanceMode != performanceModeOnDefault) {
				timeCore.setPerformanceMode(performanceModeOnDefault);
			}
		}

		updateData();
	}

	public void OnAdminListUpdate() {
		if(isOwner) {
			bool changeIndex = false;
			if(userSelect < 0 || userSelect > Math.Max(adminList.users.Length - 1, 0)) {
				userSelect = Math.Max(0, Math.Min(userSelect, adminList.users.Length - 1));
				changeIndex = true;
			}

			if(adminSelect < 0 || adminSelect > Math.Max(adminList.admins.Length - 1, 0)) {
				adminSelect = Math.Max(0, Math.Min(adminSelect, adminList.admins.Length - 1));
				changeIndex = true;
			}

			if(changeIndex) {
				RequestSerialization();
			}
		}

		updateData();
	}

	public void OnTimeCoreUpdate() {
		seasonSlider.SetValueWithoutNotify((float)timeCore.latestSeasonPercent);
		dayNightSpeedSlider.SetValueWithoutNotify((float)timeCore.dayNightLengthInMinutes);
		dayNightSpeedLabel.text = Math.Floor(timeCore.dayNightLengthInMinutes) + "min";
		weatherSlider.SetValueWithoutNotify((float)timeCore.latestWeatherPercent);

		seasonChangeText.text = timeCore.isSeasonChange() ? "Enabled" : "Disabled";
		seasonChangeImage.color = timeCore.isSeasonChange() ? onColor : offColor;

		dayNightChangeText.text = timeCore.isDayNightChange() ? "Enabled" : "Disabled";
		dayNightChangeImage.color = timeCore.isDayNightChange() ? onColor : offColor;

		weatherChangeText.text = timeCore.isWeatherChange() ? "Enabled" : "Disabled";
		weatherChangeImage.color = timeCore.isWeatherChange() ? onColor : offColor;

		double currentSeconds = Utils.UnixSeconds();
		double seasonTimeOffset = timeCore.seasonStart <= 0 ? -timeCore.seasonStart : currentSeconds - timeCore.seasonStart;
		double dayNightTimeOffset = timeCore.dayNightStart <= 0 ? -timeCore.dayNightStart : currentSeconds - timeCore.dayNightStart;

		worldDebugInfoText.text = String.Format("Debug Info: start: {0,10:0.0000}, {1,10:0.0000}, {2,10:0.0000}, offset: {3,10:0.0000}, {4,10:0.0000}, latestPercent: {5,10:0.0000}, {6,10:0.0000}, {7,10:0.0000}, latestWorldDate: {8}, latestWind: ({9,10:0.0000}, {10,10:0.0000}, {11,10:0.0000}), length: {12,10:0.0000}, {13,10:0.0000}, {14,10:0.0000}", timeCore.seasonStart, timeCore.dayNightStart, timeCore.weatherInfo, seasonTimeOffset, dayNightTimeOffset, timeCore.latestSeasonPercent, timeCore.latestdayNightPercent, timeCore.latestWeatherPercent, timeCore.latestWorldDate.ToShortDateString(), timeCore.latestWind.x, timeCore.latestWind.y, timeCore.latestWind.z, timeCore.yearLengthInMinutes, timeCore.dayNightLengthInMinutes, timeCore.weatherScaleInMinutes);
	}

	public void updateData() {
		bool isAdmin = adminList.isAdmin;
		panelRespawnButton.SetActive(isAdmin);
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

		allAdminsText.text = adminList.allAdmins ? "Enabled" : "Disabled";
		allAdminsImage.color = adminList.allAdmins ? onColor : offColor;

		Tab0.color = tab == 0 ? onColor : offColor;
		Tab1.color = tab == 1 ? onColor : offColor;
		Tab2.color = tab == 2 ? onColor : offColor;
		Tab3.color = tab == 3 ? onColor : offColor;

		Tab0Container.SetActive(tab == 0);
		Tab1Container.SetActive(tab == 1);
		Tab2Container.SetActive(tab == 2);
		Tab3Container.SetActive(tab == 3);

		perfModeText.text = performanceModeOnDefault ? "Enabled" : "Disabled";
		perfModeImage.color = performanceModeOnDefault ? onColor : offColor;
		spawnText.text = new string[] { "Entrance", "Basement" }[spawnPosition];
		stagePlayerText.text = stagePlayer ? "Enabled" : "Disabled";
		stagePlayerImage.color = stagePlayer ? onColor : offColor;
		easterEggHuntText.text = easterEggHuntEnabled ? "Enabled" : "Disabled";
		easterEggHuntImage.color = easterEggHuntEnabled ? onColor : offColor;
		birthdayDecorationsText.text = birthdayDecorationsEnabled ? "Enabled" : "Disabled";
		birthdayDecorationsImage.color = birthdayDecorationsEnabled ? onColor : offColor;

		if(stagePlayerLocation == Vector3.zero) stagePlayerLocation = stagePlayerGameObject.transform.position;
		if(stagePlayer) stagePlayerGameObject.transform.position = stagePlayerLocation;
		else stagePlayerGameObject.transform.position = new Vector3(1000000, 1000000, 1000000);
		easterEggHunt.SetActive(easterEggHuntEnabled);
		birthdayDecorations.SetActive(birthdayDecorationsEnabled);

		userListGUI.text = adminList.userListStr;
		adminListGUI.text = adminList.adminListStr;
		userSelectBox.anchoredPosition = new Vector2(0, -13.75f * userSelect);
		adminSelectBox.anchoredPosition = new Vector2(0, -13.75f * adminSelect);

		notAdminInfo.text = "You are not an admin. Ask one of these admins to add you...\n" + adminList.adminListStr;
	}
}