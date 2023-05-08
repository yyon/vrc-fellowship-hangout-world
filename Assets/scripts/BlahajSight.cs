using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum BlahajSightType {
	Location,
	Near,
	Regular,
	Far,
}

public enum Change {
	True,
	None,
	False
}

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BlahajSight : UdonSharpBehaviour {
	public BlahajSightType sightType;
	public BlahajNPC blahaj;
	public Collider blahajSelf;
	[HideInInspector] public int playerCount = 0;
	private string[] colliderNames = new string[] {
		"BlahajSightHome",
		"BlahajSightCloudShip",
		"BlahajSightMainIsland",
		"BlahajSightMegaBlahaj",
		"BlahajSightGoldenBlahaj",
		"BlahajSightBrotherBlahaj",
		"BlahajSightTimeCore",
	};
	private bool[] curSee;
	private bool[] preSee;

	[HideInInspector] public Change cSeeHome = Change.None;
	[HideInInspector] public Change cSeeCloudShip = Change.None;
	[HideInInspector] public Change cSeeMainIsland = Change.None;
	[HideInInspector] public Change cSeeMegaBlahaj = Change.None;
	[HideInInspector] public Change cSeeGoldenBlahaj = Change.None;
	[HideInInspector] public Change cSeeBrotherBlahaj = Change.None;
	[HideInInspector] public Change cSeeTimeCore = Change.None;

	[HideInInspector] public bool isSeeHome = true;
	[HideInInspector] public bool isSeeCloudShip = true;
	[HideInInspector] public bool isSeeMainIsland = false;
	[HideInInspector] public bool isSeeMegaBlahaj = false;
	[HideInInspector] public bool isSeeGoldenBlahaj = false;
	[HideInInspector] public bool isSeeBrotherBlahaj = false;
	[HideInInspector] public bool isSeeTimeCore = false;

	private int step = 0;
	private bool callback = false;

	public void Start() {
		curSee = new bool[colliderNames.Length];
		preSee = new bool[colliderNames.Length];
	}

	public void Look(bool callback = false) {
		Reset();
		step = 0;
		this.callback = callback;
		gameObject.SetActive(true);
	}

	public void FixedUpdate() {
		step++;
		if(step == 2) {
			Change[] isSee = new Change[colliderNames.Length];
			for(int i = 0; i < colliderNames.Length; i++) {
				isSee[i] = (curSee[i] == preSee[i] ? Change.None : (curSee[i] ? Change.True : Change.False));
			}

			cSeeHome = isSee[0];
			cSeeCloudShip = isSee[1];
			cSeeMainIsland = isSee[2];
			cSeeMegaBlahaj = isSee[3];
			cSeeGoldenBlahaj = isSee[4];
			cSeeBrotherBlahaj = isSee[5];
			cSeeTimeCore = isSee[6];

			isSeeHome = curSee[0];
			isSeeCloudShip = curSee[1];
			isSeeMainIsland = curSee[2];
			isSeeMegaBlahaj = curSee[3];
			isSeeGoldenBlahaj = curSee[4];
			isSeeBrotherBlahaj = curSee[5];
			isSeeTimeCore = curSee[6];
			gameObject.SetActive(false);
			if(callback) blahaj.OnSightComplete(this);
		}
	}

	public void Reset() {
		playerCount = 0;
		preSee = curSee;
		curSee = new bool[colliderNames.Length];
	}

	public void OnTriggerStay(Collider other) {
		if(other == blahajSelf) return;
		int index = Array.IndexOf(colliderNames, other.name);
		if(index != -1) curSee[index] = true;
	}

	public override void OnPlayerCollisionStay(VRCPlayerApi player) {
		playerCount++;
	}
}
