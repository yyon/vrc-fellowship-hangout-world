using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MirrorPanel : UdonSharpBehaviour {
	public MirrorGlobal global;

	public Color buttonsOff;
	public Color buttonsOn;

	public Image mirrorStatus;

	public Image mirrorOff;
	public Image mirrorCutout;
	public Image mirrorFull;

	public Image resolutionX256;
	public Image resolutionX512;
	public Image resolutionX1024;
	public Image resolutionAuto;

	public Slider opacitySlider;
	public TextMeshProUGUI opacityValueText;

	public Image autoOffNear;
	public Image autoOffFar;
	public Image autoOffOff;

	public Transform mirrorContainer;

	public MirrorCollider nearCollider;
	public MirrorCollider farCollider;
	public MirrorRenderCheck renderCheck;

	public GameObject editorPreview;

	void Start() {
		global.registerForUpdates(this);
#if UNITY_EDITOR
		editorPreview.SetActive(false);
#endif
	}

	public void mirrorOffClick() { global.mode = MirrorMode.Off; global.DidUpdate(); }
	public void mirrorCutoutClick() { global.mode = MirrorMode.Cutout; global.DidUpdate(); }
	public void mirrorFullClick() { global.mode = MirrorMode.Full; global.DidUpdate(); }

	public void resolutionX256Click() { global.resolution = MirrorResolution.X256; global.DidUpdate(); }
	public void resolutionX512Click() { global.resolution = MirrorResolution.X512; global.DidUpdate(); }
	public void resolutionX1024Click() { global.resolution = MirrorResolution.X1024; global.DidUpdate(); }
	public void resolutionAutoClick() { global.resolution = MirrorResolution.Auto; global.DidUpdate(); }

	public void opacityUpdate() { global.opacity = opacitySlider.value; global.DidUpdate(); }

	public void autoOffNearClick() { global.autoOff = MirrorAutoOff.Near; global.DidUpdate(); }
	public void autoOffFarClick() { global.autoOff = MirrorAutoOff.Far; global.DidUpdate(); }
	public void autoOffOffClick() { global.autoOff = MirrorAutoOff.Off; global.DidUpdate(); }

	public void OnUpdate() {
		mirrorOff.color = global.mode == MirrorMode.Off ? buttonsOn : buttonsOff;
		mirrorCutout.color = global.mode == MirrorMode.Cutout ? buttonsOn : buttonsOff;
		mirrorFull.color = global.mode == MirrorMode.Full ? buttonsOn : buttonsOff;

		resolutionX256.color = global.resolution == MirrorResolution.X256 ? buttonsOn : buttonsOff;
		resolutionX512.color = global.resolution == MirrorResolution.X512 ? buttonsOn : buttonsOff;
		resolutionX1024.color = global.resolution == MirrorResolution.X1024 ? buttonsOn : buttonsOff;
		resolutionAuto.color = global.resolution == MirrorResolution.Auto ? buttonsOn : buttonsOff;

		opacitySlider.SetValueWithoutNotify(global.opacity);
		opacityValueText.text = string.Format("{0,0:000}%", global.opacity * 100);

		autoOffNear.color = global.autoOff == MirrorAutoOff.Near ? buttonsOn : buttonsOff;
		autoOffFar.color = global.autoOff == MirrorAutoOff.Far ? buttonsOn : buttonsOff;
		autoOffOff.color = global.autoOff == MirrorAutoOff.Off ? buttonsOn : buttonsOff;

		renderCheck.SetOn(global.mode != MirrorMode.Off);
	}
}
