using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class SeasonChangeToggle : UdonSharpBehaviour {
	public TimeCore timeCore;
	public Color offColor;
	public Color onColor;
	public TextMeshPro statusText;
	public Renderer statusRenderer;
	public TextMeshProUGUI statusTextCanvas;
	public Image statusImageColor;

	public void Start() {
		timeCore.registerForUpdates(this);
	}

	public void OnTimeCoreUpdate() {
		if(statusText) statusText.text = timeCore.isSeasonChange() ? "Enabled" : "Disabled";
		if(statusRenderer) statusRenderer.material.SetColor("_EmissionColor", timeCore.isSeasonChange() ? onColor : offColor);
		if(statusTextCanvas) statusTextCanvas.text = timeCore.isSeasonChange() ? "Enabled" : "Disabled";
		if(statusImageColor) statusImageColor.color = timeCore.isSeasonChange() ? onColor : offColor;
	}

	public override void Interact() {
		timeCore.setSeasonNetworked(!timeCore.isSeasonChange());
	}
}
