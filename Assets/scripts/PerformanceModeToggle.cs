using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

public class PerformanceModeToggle : UdonSharpBehaviour {
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
		if(statusText) statusText.text = timeCore.localPerformanceMode ? "Enabled" : "Disabled";
		if(statusRenderer) statusRenderer.material.SetColor("_EmissionColor", timeCore.localPerformanceMode ? onColor : offColor);
		if(statusTextCanvas) statusTextCanvas.text = timeCore.localPerformanceMode ? "Enabled" : "Disabled";
		if(statusImageColor) statusImageColor.color = timeCore.localPerformanceMode ? onColor : offColor;
	}

	public override void Interact() {
        timeCore.setPerformanceMode(!timeCore.localPerformanceMode);
    }
}
