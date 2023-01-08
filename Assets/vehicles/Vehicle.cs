
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Vehicle : UdonSharpBehaviour {
    public Animator vehicleAnim;
    public VehicleDoor doorRight;
    public VehicleDoor doorLeft;
	public TextMeshPro[] infoText;

	private bool isOwner = false;

	[UdonSynced(UdonSyncMode.None)] private int location = 0;
	[UdonSynced(UdonSyncMode.None)] private int stateNameHash = -1;
	[UdonSynced(UdonSyncMode.None)] private double stateStart = 0;
	[UdonSynced(UdonSyncMode.None)] private double stateLength = 0;

	public double UnixSeconds() {
		DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return (DateTime.UtcNow - epochStart).TotalSeconds;
	}

	void Start() {
		isOwner = Networking.GetOwner(gameObject) == Networking.LocalPlayer;
		updateText();
	}

	public override void OnOwnershipTransferred(VRCPlayerApi newPlayer) {
		isOwner = newPlayer == Networking.LocalPlayer;
	}

	public void Update() {
		if(!isOwner) return;
		int currentNameHash = vehicleAnim.GetCurrentAnimatorStateInfo(0).shortNameHash;
		if(currentNameHash != stateNameHash) {
			stateNameHash = currentNameHash;
			stateStart = UnixSeconds();
			stateLength = vehicleAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
			updateText();
			RequestSerialization();
		}
	}

	public void updateText() {
		string text = "Error";

		string[] stateName = { "Vehicle 2 Home Island Idle", "Vehicle 2 Home Island View Loop", "Vehicle 2 Building Idle", "Vehicle 2 Blahaj Idle" };
		string[] friendlyName = { "Home Island", "Home Island Aerial View", "Building", "Blåhaj" };

		if(vehicleAnim.GetCurrentAnimatorStateInfo(0).IsName(stateName[location])) text = "Location: " + friendlyName[location];
		else text = "Heading to: " + friendlyName[location];

		for(int i = 0; i < infoText.Length; i++) infoText[i].text = text;
	}

    public void closeDoors() {
		doorRight.SendCustomEvent("close");
		doorLeft.SendCustomEvent("close");
	}

    public void openDoors() {
		doorRight.SendCustomEvent("open");
		doorLeft.SendCustomEvent("open");
	}

	public void homeIsland() => setNewLocation(0);
	public void homeIslandView() => setNewLocation(1);
	public void building() => setNewLocation(2);
	public void blahaj() => setNewLocation(3);

	private void setNewLocation(int newLocation) {
		if(location == newLocation) return;
		location = newLocation;
		vehicleAnim.SetInteger("Location", location);
		if(!isOwner) Networking.SetOwner(Networking.LocalPlayer, gameObject);
		updateText();
		RequestSerialization();
	}

	public void OnDeserialization() {
		vehicleAnim.SetInteger("Location", location);

		if(stateNameHash != -1 && vehicleAnim.GetCurrentAnimatorStateInfo(0).shortNameHash != stateNameHash) {
			double curSeconds = UnixSeconds();
			double animationOffset = curSeconds - stateStart;
			if(stateLength != 0) animationOffset /= stateLength;
			vehicleAnim.Play(stateNameHash, 0, (float)animationOffset % 1);
		}

		updateText();
	}
}
