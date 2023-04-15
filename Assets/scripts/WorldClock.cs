using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;
using TMPro;
using VRC.SDK3.Components;

public class WorldClock : UdonSharpBehaviour {
	public TimeCore timeCore;
	public AdminList adminList;

	public Slider dayNightSlider;
	public RectTransform dayNightIndicator;
	public GameObject rainIndicator;
	public GameObject snowIndicator;
	public TextMeshProUGUI timeIndicator;

	public void Start() {
		timeCore.registerForUpdates(this);
	}

	public void dayNightChange() {
		timeCore.setDayNightNetworked(timeCore.isDayNightChange(), dayNightSlider.value);
	}

	public void OnAdminListUpdate() {
		dayNightSlider.gameObject.SetActive(adminList.isAdmin);
	}

	public void OnTimeCoreUpdate() {
		dayNightSlider.SetValueWithoutNotify((float)timeCore.latestdayNightPercent);

		dayNightIndicator.localRotation = Quaternion.Euler(0, 0, (float)timeCore.latestdayNightPercent * -360 + 90);

		bool winter = timeCore.latestWorldDate.Month == 12 || timeCore.latestWorldDate.Month < 3;
		rainIndicator.SetActive(timeCore.latestWeatherPercent > 0.2 && !winter);
		snowIndicator.SetActive(timeCore.latestWeatherPercent > 0.2 && winter);

		timeIndicator.text = Math.Floor(timeCore.latestdayNightPercent * 24).ToString().PadLeft(2, '0') + ":" + Math.Floor((timeCore.latestdayNightPercent * 24 * 60) % 60).ToString().PadLeft(2, '0');
	}
}
