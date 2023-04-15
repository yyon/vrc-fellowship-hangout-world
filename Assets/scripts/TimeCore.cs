using CentauriCore.Blackjack;
using Newtonsoft.Json.Linq;
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using static Utils;
using static VRC.Core.ApiInfoPushSystem;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TimeCore : UdonSharpBehaviour {
	[Header("Settings")]
	public bool localPerformanceMode = true;
	public int localFogMode = 1;
	public bool localParticleEffects = true;
	//start variables: 0 or less is constant offset (seconds), greater than 0 is start in unix time (seconds)
	[UdonSynced(UdonSyncMode.None)] public double seasonStart = 0;
	[UdonSynced(UdonSyncMode.None)] public double dayNightStart = 0;
	//weatherInfo: 0 or less is weather strength, greater than 0 is weather offset for procedural generation
	[UdonSynced(UdonSyncMode.None)] public double weatherInfo = 1;
	[UdonSynced(UdonSyncMode.None)] public double dayNightLengthInMinutes = 30;
	[UdonSynced(UdonSyncMode.None)] public double yearLengthInMinutes = 240;
	[Tooltip("Roughly minimum minutes between storms")]
	public double weatherScaleInMinutes = 30;

	[Header("DayNight Settings")]
	public Color SunColorMid = new Color(255, 151, 121);
	public Color SunColorDay = new Color(255, 244, 214);
	public Color AmbientColorNight = new Color(0, 0, 0);
	public Color AmbientColorMid = new Color(197, 167, 159);
	public Color AmbientColorDay = new Color(255, 244, 214);
	public Color CloudColorNight = new Color(53, 48, 60);
	public Color CloudColorMid = new Color(255, 149, 121);
	public Color CloudColorDay = new Color(255, 255, 255);
	public Color MoonColorNight = new Color(255, 255, 255);
	public Color MoonColorMid = new Color(209, 172, 37);
	public Color OceanColorNight = new Color(5, 7, 13);
	public Color OceanColorDay = new Color(44, 51, 78);
	public Color FogRegular;
	public Color FogDay;
	public Color FogNight;
	public Transform cityNightLightsContainer;
	private double SunValue = 0;

	[Header("World Things")]
	public AdminList adminList;
	public UdonSharpBehaviour[] registeredUpdates;
	public Renderer MainIsland;
	public Material GrassMaterial;
	public Material SnowMaterial;
	public GameObject SnowTrees;
	public GameObject Trees;
	public Renderer Fog;

	[Header("DayNight")]
	public GameObject Sky;
	public Animator SkyAnimation;
	public Light Sun;
	public Renderer LowCloud;
	public Renderer HighCloud;
	public Renderer Stars;
	public Renderer Moon;
	public ReflectionProbe Probe;
	public Cubemap DawnCubemap;
	public Cubemap DayCubemap;
	public Cubemap DuskCubemap;
	public Cubemap NightCubemap;
	public Renderer Ocean;

	[Header("Weather")]
	public Transform Wind;
	public GameObject StormClouds;
	public Renderer StormCloudsLayer1;
	public Renderer StormCloudsLayer2;
	public Renderer StormCloudsLayer3;
	public Renderer StormCloudsShadow;
	public ParticleSystem RainEffect;
	public ParticleSystem SnowEffect;
	public ParticleSystem PedalsEffect;
	public ParticleSystem FallLeavesEffect;

	[Header("Holidays")]
	public GameObject Christmas;
	public GameObject Easter;
	public GameObject Thanksgiving;
	public GameObject Halloween;
	public GameObject Birthday;

	[HideInInspector] public bool isOwner = false;

	void Start() {
		SunValue = Sun.intensity;
		isOwner = Networking.GetOwner(gameObject) == Networking.LocalPlayer;
		if(isOwner) {
			double currentSeconds = UnixSeconds();
			UnityEngine.Random.InitState((int)Time.time);
			dayNightStart = currentSeconds - UnityEngine.Random.Range(0, (float)dayNightLengthInMinutes * 60);
			double yearTotalDays = (new DateTime(DateTime.Now.Year, 12, 31) - new DateTime(DateTime.Now.Year, 1, 1)).TotalDays;
			seasonStart = -(DateTime.Now.DayOfYear / yearTotalDays) * yearLengthInMinutes * 60;
			RequestSerialization();
			OnDeserialization();
		}
		
		dynamicUpdateLoop();

		SendCustomEventDelayedFrames(nameof(sendUpdates), 2);
	}

	public void registerForUpdates(UdonSharpBehaviour udonBehaviour) {
		Utils.Append(ref registeredUpdates, udonBehaviour);
	}

	public override void OnOwnershipTransferred(VRCPlayerApi newPlayer) {
		isOwner = newPlayer == Networking.LocalPlayer;
	}

	public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner) {
		return adminList.containsAdmin(requestedOwner.displayName);
	}

	public void setPerformanceMode(bool value) {
		localPerformanceMode = value;
		Sun.gameObject.SetActive(!localPerformanceMode);
		sendUpdates();
	}

	public void setFogMode(int value) {
		localFogMode = value;
		Fog.gameObject.SetActive(localFogMode != 0);
		if(localFogMode == 1) Fog.material.SetFloat("_Intensity", 1);
		if(localFogMode == 2) Fog.material.SetFloat("_Intensity", 10);
		dynamicUpdate();
	}

	public void setParticleEffects(bool value) {
		localParticleEffects = value;
		dynamicUpdate();
	}

	public bool isDayNightChange() {
		return dayNightStart > 0;
	}

	public void setDayNightNetworked(bool change, double percent = -1) {
		if(!adminList.isAdmin) return;
		double currentSeconds = Utils.UnixSeconds();
		double offset;
		if(percent != -1) offset = percent * dayNightLengthInMinutes * 60;
		else if(dayNightStart > 0) offset = (currentSeconds - dayNightStart) % (dayNightLengthInMinutes * 60);
		else offset = -dayNightStart;
		dayNightStart = change ? currentSeconds - offset : -offset;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void setDayNightLengthNetworked(double lengthInMinutes) {
		if(!adminList.isAdmin) return;
		double currentSeconds = Utils.UnixSeconds();
		double dayNightTimeOffset = dayNightStart <= 0 ? -dayNightStart : currentSeconds - dayNightStart;
		double dayNightPercent = (dayNightTimeOffset / (dayNightLengthInMinutes * 60)) % 1;
		dayNightLengthInMinutes = lengthInMinutes;
		if(dayNightStart <= 0) dayNightStart = -dayNightPercent * dayNightLengthInMinutes * 60;
		else dayNightStart = currentSeconds - dayNightPercent * dayNightLengthInMinutes * 60;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public bool isSeasonChange() {
		return seasonStart > 0;
	}

	public void setSeasonNetworked(bool change, double percent = -1) {
		if(!adminList.isAdmin) return;
		double currentSeconds = Utils.UnixSeconds();
		double offset;
		if(percent != -1) offset = percent * yearLengthInMinutes * 60;
		else if(seasonStart > 0) offset = (currentSeconds - seasonStart) % (yearLengthInMinutes * 60);
		else offset = -seasonStart;
		seasonStart = change ? currentSeconds - offset : -offset;
		Debug.Log(seasonStart + " : " + change);
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public void setYearLengthNetworked(double lengthInMinutes) {
		if(!adminList.isAdmin) return;
		double currentSeconds = Utils.UnixSeconds();
		double seasonTimeOffset = seasonStart <= 0 ? -seasonStart : currentSeconds - seasonStart;
		double seasonPercent = (seasonTimeOffset / (yearLengthInMinutes * 60)) % 1;
		yearLengthInMinutes = lengthInMinutes;
		if(seasonStart <= 0) seasonStart = -seasonPercent * yearLengthInMinutes * 60;
		else seasonStart = currentSeconds - seasonPercent * yearLengthInMinutes * 60;
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	public bool isWeatherChange() {
		return weatherInfo > 0;
	}

	public void setWeatherNetworked(bool change, double percent = -1) {
		if(!adminList.isAdmin) return;
		if(change) weatherInfo = 1;
		else if(percent == -1) {
			double currentSeconds = Utils.UnixSeconds();
			double weather = getWeather(currentSeconds, out Vector3 wind);
			weatherInfo = -weather;
		}
		else {
			weatherInfo = -percent;
		}
		Networking.SetOwner(Networking.LocalPlayer, gameObject);
		RequestSerialization();
		OnDeserialization();
	}

	private void setDayNight(double percent, double speedRPS, double weather) {
		double foldedPercent = percent > 0.5 ? 1 - percent : percent;
		Sky.transform.localRotation = Quaternion.Euler(0, 0, -((float)percent * 360f) + 90);

		SkyAnimation.Play("SkySmoothTurn", 0, 0);
		SkyAnimation.SetFloat("speed", (float)speedRPS);

		if(localFogMode == 1) Fog.material.SetColor("_Color", FogRegular);
		else if(localFogMode == 2) {
			Color fogColor = Utils.Gradient(foldedPercent, new double[] { 0.2, 0.35 }, new Color[] { FogNight, FogDay });
			fogColor = Utils.Gradient(weather, new double[] { 0.4, 0.8 }, new Color[] { fogColor, FogNight });
			Fog.material.SetColor("_Color", fogColor);
		}

		Color sunColor = Utils.Gradient(foldedPercent, new double[] { 0.23, 0.25, 0.35 }, new Color[] { Color.black, SunColorMid, SunColorDay });
		sunColor = Utils.Gradient(weather, new double[] { 0.4, 0.8 }, new Color[] { sunColor, Color.black });
		Sun.color = sunColor;

		double sunInensity = Utils.Gradient(foldedPercent, new double[] { 0.23, 0.25 }, new double[] { 0.002, SunValue });
		sunInensity = Utils.Gradient(weather, new double[] { 0.4, 0.8 }, new double[] { sunInensity, 0.002 });
		Sun.intensity = (float)sunInensity;

		Color ambientColor = Utils.Gradient(foldedPercent, new double[] { 0.2, 0.25, 0.35 }, new Color[] { AmbientColorNight, AmbientColorMid, AmbientColorDay });
		ambientColor = Utils.Gradient(weather, new double[] { 0.3, 0.8 }, new Color[] { ambientColor, AmbientColorNight });
		RenderSettings.ambientLight = ambientColor;

		Color cloudColor = Utils.Gradient(foldedPercent, new double[] { 0.2, 0.25, 0.35 }, new Color[] { CloudColorNight, CloudColorMid, CloudColorDay });
		LowCloud.material.SetColor("_CloudColor", cloudColor);
		HighCloud.material.SetColor("_CloudColor", cloudColor);

		Stars.gameObject.SetActive(foldedPercent <= 0.35);
		Stars.material.SetColor("_EmissionColor", Utils.Gradient(foldedPercent, new double[] { 0.25, 0.35 }, new Color[] { Color.white, Color.black }));
		Moon.material.color = Utils.Gradient(foldedPercent, new double[] { 0.2, 0.25 }, new Color[] { MoonColorNight, MoonColorMid });

		if(percent < 0.25) Probe.customBakedTexture = NightCubemap;
		else if(percent < 0.35) Probe.customBakedTexture = DawnCubemap;
		else if(percent < 0.65) Probe.customBakedTexture = DayCubemap;
		else Probe.customBakedTexture = DuskCubemap;

		Ocean.material.SetColor("_Color", Utils.Gradient(foldedPercent, new double[] { 0.2, 0.25 }, new Color[] { OceanColorNight, OceanColorDay }));

		double nightCenteredPercent = (percent + 0.5) % 1;
		for(int i = 0; i < cityNightLightsContainer.childCount; i++) {
			bool isOn = false;

			UnityEngine.Random.InitState(i);
			double lightRandom = UnityEngine.Random.value;

			if(foldedPercent < 0.2 + lightRandom * 0.05) {
				double interval = lightRandom * 0.05 + 0.05; //0.05 - 0.1
				if((nightCenteredPercent % interval) < 0.25) isOn = true;
			}

			cityNightLightsContainer.GetChild(i).gameObject.SetActive(isOn);
		}
	}

	private void setSeason(DateTime date, double weather, Vector3 wind) {
		bool winter = date.Month == 12 || date.Month < 3;
		bool sping = date.Month >= 3 && date.Month < 6;
		bool summer = date.Month >= 6 && date.Month < 9;
		bool fall = date.Month >= 9 && date.Month < 12;

		bool christmas = date.Month == 12;
		bool easter = date.Month == 4;
		bool thanksgiving = date.Month == 11 && date.Day >= 15;
		bool halloween = date.Month == 10;

		Material[] materials = MainIsland.materials;
		materials[1] = winter ? SnowMaterial : GrassMaterial;
		MainIsland.materials = materials;
		SnowTrees.SetActive(winter);
		Trees.SetActive(!winter);

		RainEffect.gameObject.SetActive(localParticleEffects && weather > 0.2 && !winter);
		SnowEffect.gameObject.SetActive(localParticleEffects && weather > 0.2 && winter);
		PedalsEffect.gameObject.SetActive(localParticleEffects && sping || summer);
		FallLeavesEffect.gameObject.SetActive(localParticleEffects && fall);

		Christmas.SetActive(christmas);
		Easter.SetActive(easter);
		Thanksgiving.SetActive(thanksgiving);
		Halloween.SetActive(halloween);

		StormClouds.SetActive(weather > 0.3);

		float stormCloudAlpha = (float)Utils.Gradient(weather, new double[] { 0.3, 1 }, new double[] { 0, 1 });
		StormCloudsLayer1.material.SetColor("_CloudColor", new Color(0.01f, 0.01f, 0.01f, stormCloudAlpha));
		StormCloudsLayer2.material.SetColor("_CloudColor", new Color(0.1f, 0.1f, 0.1f, stormCloudAlpha));
		StormCloudsLayer3.material.SetColor("_CloudColor", new Color(0.2f, 0.2f, 0.2f, stormCloudAlpha));
		StormCloudsShadow.material.color = new Color(0f, 0f, 0f, stormCloudAlpha * 0.995f);

		var rainEmission = RainEffect.emission;
		rainEmission.rateOverTime = Utils.Gradient(weather, new double[] { 0.2, 1 }, new double[] { 0, 900 });
		var snowEmission = SnowEffect.emission;
		snowEmission.rateOverTime = Utils.Gradient(weather, new double[] { 0.2, 1 }, new double[] { 0, 900 });

		Vector3 windCompensation = -wind * 4;
		Vector3 playerPos = Networking.LocalPlayer == null ? Vector3.zero : Networking.LocalPlayer.GetPosition();
		float fallDistance = SnowEffect.transform.position.y - playerPos.y;
		windCompensation.y = 0;
		RainEffect.transform.localPosition = windCompensation * Mathf.Sqrt(fallDistance / (9.8f * RainEffect.main.gravityModifier.constant - wind.y));
		SnowEffect.transform.localPosition = windCompensation * Mathf.Sqrt(fallDistance / (9.8f * SnowEffect.main.gravityModifier.constant - wind.y));
		PedalsEffect.transform.localPosition = windCompensation * Mathf.Sqrt(fallDistance / (9.8f * PedalsEffect.main.gravityModifier.constant - wind.y));
		FallLeavesEffect.transform.localPosition = windCompensation * Mathf.Sqrt(fallDistance / (9.8f * FallLeavesEffect.main.gravityModifier.constant - wind.y));

		float scale = wind.magnitude;
		if(scale < 0.01) scale = 0;
		Wind.localScale = new Vector3(scale, scale, scale);
		Wind.rotation = Quaternion.LookRotation(wind);
	}

	public DateTime getDate(double currentSeconds, out double seasonPercent, out double timeInDays) {
		double yearTotalDays = (new DateTime(DateTime.Now.Year, 12, 31) - new DateTime(DateTime.Now.Year, 1, 1)).TotalDays;

		double seasonTimeOffset = seasonStart <= 0 ? -seasonStart : currentSeconds - seasonStart;
		seasonPercent = (seasonTimeOffset / (yearLengthInMinutes * 60)) % 1;
		timeInDays = seasonPercent * yearTotalDays;
		DateTime worldNow = new DateTime(DateTime.Now.Year + 800, 1, 1).AddDays(timeInDays - 1);
		return worldNow;
	}

	public double getWeather(double currentSeconds, out Vector3 wind) {
		float weatherNoisePosition = (float)Utils.PingPong((currentSeconds + weatherInfo) / (weatherScaleInMinutes * 60.0f), 500);

		double weather = -weatherInfo;
		if(weatherInfo > 0) {
			weather = Math.Pow(PerlinNoise3Octives(weatherNoisePosition), 2);
			//eye of the storm
			weather = Utils.Gradient(weather, new double[] { 0, 0.9, 0.93 }, new double[] { 0, 0.9, 0.05 });
		}

		wind = new Vector3(
			PerlinNoise3Octives(weatherNoisePosition * 10 + 45.28f), 0, 0
		) * Utils.Gradient(weather, new double[] { 0, 0.5, 1 }, new double[] { 0.2, 0.2, 1 });
		wind = Quaternion.AngleAxis(PerlinNoise3Octives(weatherNoisePosition * 50 + 12.76f) * 120 - 90, Vector3.right) * wind;
		wind = Quaternion.AngleAxis(PerlinNoise3Octives(weatherNoisePosition * 50 + 92.41f) * 360, Vector3.up) * wind;

		return weather;
	}

	private double dT = 0;
	public void dynamicUpdateLoop() {
		dynamicUpdate();
		dT = 0.5;
		SendCustomEventDelayedSeconds(nameof(dynamicUpdateLoop), (float)dT);
	}

	[HideInInspector] public double latestSeasonPercent = 0;
	[HideInInspector] public double latestdayNightPercent = 0;
	[HideInInspector] public double latestWeatherPercent = 0;
	[HideInInspector] public DateTime latestWorldDate = DateTime.Now;
	[HideInInspector] public Vector3 latestWind = Vector3.zero;

	public void dynamicUpdate(double dT = 0.0) {
		double currentSeconds = UnixSeconds();

		double dayNightTimeOffset = dayNightStart <= 0 ? -dayNightStart : currentSeconds - dayNightStart;
		latestdayNightPercent = (dayNightTimeOffset / (dayNightLengthInMinutes * 60)) % 1;

		latestWorldDate = getDate(currentSeconds, out latestSeasonPercent, out double timeInDays);
		latestWeatherPercent = getWeather(currentSeconds, out latestWind);

		setSeason(latestWorldDate, latestWeatherPercent, latestWind);
		setDayNight(latestdayNightPercent, 1 / (dayNightLengthInMinutes * 60), latestWeatherPercent);

		sendUpdates();
	}

	public void sendUpdates() {
		foreach(UdonSharpBehaviour registeredUpdate in registeredUpdates) {
			registeredUpdate.SendCustomEvent("OnTimeCoreUpdate");
		}
	}

	bool isInitialLoad = true;
	public override void OnDeserialization() {
		//if(isInitialLoad) {
		//	isInitialLoad = false;
		//}

		dynamicUpdate();
	}
}
