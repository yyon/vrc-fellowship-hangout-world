using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System;

public class BasementControlPanel : UdonSharpBehaviour {
	public float runIntervalInSeconds = 60*5;
	private bool runScheduled = false;
	private bool _running = false;
	public bool running {
		get => _running;
		set {
			_running = value;
			if(!runScheduled) {
				runScheduled = true;
				dynamicUpdate();
			}
		}
	}

	public TextMeshProUGUI uptimeLabel;

	void Start() {
		running = true;
	}

	public void dynamicUpdate() {
		TimeSpan upTime = DateTime.Now.AddYears(800) - new DateTime(2564, 2, 15, 0, 0, 0);
		uptimeLabel.text = "uptime " + Math.Floor(upTime.TotalDays / 365) + "y "
			+ Math.Floor(upTime.TotalDays % 365) + "d "
			+ Math.Floor((upTime.TotalDays * 24) % 24) + "h";

		if(_running) SendCustomEventDelayedSeconds(nameof(dynamicUpdate), runIntervalInSeconds);
		else runScheduled = false;
	}
}
