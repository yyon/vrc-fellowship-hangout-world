using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class BlahajNPC : UdonSharpBehaviour {
	public string blahajName;
	public Transform blahaj;
	public Transform speechBubble;
	public TextMeshPro speechBubbleText;
	public float positionInterpolateSpeed;
	public float sightUpdateIntervalSeconds;
	public BlahajSight locationSight;
	//public BlahajSight nearSight;
	public BlahajSight regularSight;
	//public BlahajSight farSight;

	[UdonSynced(UdonSyncMode.None)] private string line = "Why are there always holiday decorations here?";
	[UdonSynced(UdonSyncMode.None)] private double lineExpiration = -1;
	private double cooldownEnd = 0;

	public void Start() {
		OnDeserialization();

		//needed because sight values start with wrong values
		locationSight.Look();
		regularSight.Look();
		SendCustomEventDelayedSeconds(nameof(sightUpdate), 2);
	}

	void Update() {
		Quaternion targetRotation = Quaternion.LookRotation(blahaj.position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
		targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

		Vector3 offset = new Vector3(0.55f, 0.35f, 0.15f);
		if(locationSight.isSeeHome) offset = new Vector3(0.55f, 0.35f, -0.25f);

		Vector3 targetPosition = blahaj.position + targetRotation * offset;

		if(Vector3.Distance(speechBubble.position, targetPosition) > 7) {
			speechBubble.position = blahaj.position;
		}

		speechBubble.position = Vector3.Lerp(speechBubble.position, targetPosition, positionInterpolateSpeed * Time.deltaTime);
		speechBubble.rotation = Quaternion.Lerp(speechBubble.rotation, targetRotation, positionInterpolateSpeed * Time.deltaTime);
	}

	public void sightUpdate() {
		if(Networking.GetOwner(blahaj.gameObject) == Networking.LocalPlayer) {
			locationSight.Look(true);
			//nearSight.Look();
			regularSight.Look();
			//farSight.Look();
		}
		SendCustomEventDelayedSeconds(nameof(sightUpdate), sightUpdateIntervalSeconds);
	}

	public void setTimes(double expirationSeconds, double cooldownSeconds) {
		double timeSeconds = Utils.UnixSeconds();
		lineExpiration = timeSeconds + expirationSeconds;
		cooldownEnd = timeSeconds + cooldownSeconds;
	}

	public void networkedUpdates() {
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	[UdonSynced(UdonSyncMode.None)] private bool leftHomeRemark = false;
	private bool newPlaceRemark = false;
	[UdonSynced(UdonSyncMode.None)] bool brotherRemark = false;
	private bool wentFarFromHome = false;
	private bool partyRemark = false;
	public void OnSightComplete(BlahajSight sight) {
		double currentTime = Utils.UnixSeconds();
		Random.InitState((int)((currentTime % 100000) * 10000));
		bool cooledDown = (cooldownEnd - currentTime) < 0;

		if(!locationSight.isSeeCloudShip) wentFarFromHome = true;

		if(regularSight.playerCount > 15 && !partyRemark && cooledDown) {
			partyRemark = true;
			line = Utils.PickRandom(new string[] {
				"Wow that's a lot of people, is there a party happening?",
				"It been a while since I've seen this many people",
				"What are all these people doing here?",
				"That's like at least 15 people here",
				"Hi everyone, my name is " + blahajName,
			});

			setTimes(60, 5);
			networkedUpdates();
		}
		else if(locationSight.cSeeTimeCore == Change.True && cooledDown) {
			line = Utils.PickRandom(new string[] {
				"Wow I've only ever heard of time cores...",
				"Be careful around that time core. I hear they're dangerous.",
				"I hope you know what your doing with that time core",
				"What does that time core do?",
				"I've heard stories about time cores. Their said to hold inconceivable mysteries.",
			});

			setTimes(60, 5);
			networkedUpdates();
		}
		else if(locationSight.cSeeHome == Change.True && wentFarFromHome && cooledDown) {
			wentFarFromHome = false;
			line = Utils.PickRandom(new string[] {
				"Home at last",
				"Home sweet home",
			});

			setTimes(60, 5);
			networkedUpdates();
		}
		else if(locationSight.cSeeHome == Change.False && !leftHomeRemark && cooledDown) {
			leftHomeRemark = true;
			line = Utils.PickRandom(new string[] {
				"Hey, where are you taking me?",
				"What are you doing? put me down!",
				"I recognize you... " + Networking.GetOwner(blahaj.gameObject).displayName,
				"Hey, it was real comfy there",
				"Put me back, and let me take a nap",
				"Are you going to take me somewhere fun?",
			}, new float[] { 1, 1, 0.5f, 1, 1, 1 });

			setTimes(60, 5);
			networkedUpdates();
		}
		else if(locationSight.cSeeCloudShip == Change.False && !newPlaceRemark && cooledDown) {
			newPlaceRemark = true;
			line = Utils.PickRandom(new string[] {
				"What is this place?",
				"Where are we?",
				"Where did you take me?",
				"How did we get here?",
				"Ohh, this place looks cool",
			});

			setTimes(60, 5);
			networkedUpdates();
		}
		else if(regularSight.cSeeBrotherBlahaj == Change.True && !brotherRemark && cooledDown) {
			brotherRemark = true;
			line = Utils.PickRandom(new string[] {
				"Brother!!!",
				"It's been ages since I've seen you brother!!!",
				"Where have you been brother!?",
			});

			setTimes(60, 5);
			networkedUpdates();
		}
		else if(regularSight.cSeeMegaBlahaj == Change.True && cooledDown) {
			line = Utils.PickRandom(new string[] {
				"God dam thats a big shark",
				"I didn't know sharks grow that big",
				"How did that massive shark get up here?",
			}, new float[] { 1, 1, locationSight.isSeeHome ? 2 : 0 });

			setTimes(60, 5);
			networkedUpdates();
		}
		else if(regularSight.cSeeGoldenBlahaj == Change.True && cooledDown) {
			line = Utils.PickRandom(new string[] {
				"Look at that, a golden shark",
				"Wow thats one shiny shark",
				"You know, they say golden sharks bring luck to sharks around them",
				"Treat that golden shark with care, they're usually timid",
				"Hi golden shark, what's your name? My name is " + blahajName,
			}, new float[] { 1, 1, 1, 1, regularSight.playerCount > 6 ? 1 : 0.05f });

			setTimes(60, 5);
			networkedUpdates();
		}
	}

	public void expireLine() {
		if(lineExpiration == -1 || lineExpiration - Utils.UnixSeconds() - 1 > 0) return;
		speechBubbleText.text = "";
		speechBubble.gameObject.SetActive(false);
	}

	public override void OnDeserialization() {
		speechBubbleText.text = line;
		speechBubble.gameObject.SetActive(true);
		if(lineExpiration != -1) {
			double secondsToExpire = lineExpiration - Utils.UnixSeconds();
			SendCustomEventDelayedSeconds(nameof(expireLine), Mathf.Max((float)secondsToExpire, 0));
		}
	}
}
