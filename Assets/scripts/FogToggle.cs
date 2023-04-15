using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class FogToggle : UdonSharpBehaviour {
	public TimeCore timeCore;
	public Color offColor;
	public Color onColor;
	public TextMeshPro statusText;
	public Renderer statusRenderer;
	public TextMeshProUGUI statusTextCanvas;
	public Image statusImageColor;

	private string[] statuses = new string[] { "Off", "Regular", "Ultra" };

	public void Start() {
		timeCore.registerForUpdates(this);
	}

	public void OnTimeCoreUpdate() {
		if(statusText) statusText.text = statuses[timeCore.localFogMode];
		if(statusRenderer) statusRenderer.material.SetColor("_EmissionColor", timeCore.localFogMode == 0 ? offColor : onColor);
		if(statusTextCanvas) statusTextCanvas.text = statuses[timeCore.localFogMode];
		if(statusImageColor) statusImageColor.color = timeCore.localFogMode == 0 ? offColor : onColor;
	}

	public override void Interact() {
		timeCore.setFogMode((timeCore.localFogMode + 1) % 3);
	}
}
