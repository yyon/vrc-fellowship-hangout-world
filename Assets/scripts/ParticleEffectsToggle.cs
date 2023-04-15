using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class ParticleEffectsToggle : UdonSharpBehaviour {
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
		if(statusText) statusText.text = timeCore.localParticleEffects ? "Enabled" : "Disabled";
		if(statusRenderer) statusRenderer.material.SetColor("_EmissionColor", timeCore.localParticleEffects ? onColor : offColor);
		if(statusTextCanvas) statusTextCanvas.text = timeCore.localParticleEffects ? "Enabled" : "Disabled";
		if(statusImageColor) statusImageColor.color = timeCore.localParticleEffects ? onColor : offColor;
	}

	public override void Interact() {
		timeCore.setParticleEffects(!timeCore.localParticleEffects);
	}
}
