using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class ColoredToggle : UdonSharpBehaviour {
	public bool isOn;

	[Header("Settings")]
	public Color offColor;
	public Color onColor;
	public string onText;
	public string offText;
	public TextMeshPro statusText;
	public Renderer statusRenderer;
	public TextMeshProUGUI statusTextCanvas;
	public Image statusImageColor;

	public GameObject[] enableObjects;
	public GameObject[] disableObjects;

	void Start() {
		updateVisuals();
	}

	public void updateVisuals() {
		if(statusText) statusText.text = isOn ? onText : offText;
		if(statusRenderer) statusRenderer.material.SetColor("_EmissionColor", isOn ? onColor : offColor);
		if(statusTextCanvas) statusTextCanvas.text = isOn ? onText : offText;
		if(statusImageColor) statusImageColor.color = isOn ? onColor : offColor;

		foreach(GameObject obj in enableObjects) obj.SetActive(isOn);
		foreach(GameObject obj in disableObjects) obj.SetActive(!isOn);
	}

	public override void Interact() {
		isOn = !isOn;
		updateVisuals();
	}
}
