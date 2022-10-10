
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class perforrmanceMode : UdonSharpBehaviour
{
    public GameObject[] disable;
    public GameObject[] enable;
    public DayNightCycleController_v2 controller;
    public Toggle shadowsToggle;
    public shadowToggle shadowScript;

    bool performanceModeOn = false;

    void Interact()
    {
        this.performanceModeOn = !this.performanceModeOn;

        for (int i = 0; i < disable.Length; i++) {
            GameObject obj = disable[i];
            obj.SetActive(!this.performanceModeOn);
        }

        for (int i = 0; i < enable.Length; i++) {
            GameObject obj = enable[i];
            obj.SetActive(this.performanceModeOn);
        }

        controller.LocalToggle.isOn = this.performanceModeOn;
        controller.LocalUpdated();
        if (this.performanceModeOn) {
            controller.TimeSlider.value = 0;
            controller.SpeedSlider.value = 0;
            controller.SliderUpdated();
        }

        shadowsToggle.isOn = !this.performanceModeOn;
        this.shadowScript.ShadowToggle();
    }
}
