
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class randomTime : UdonSharpBehaviour
{
    public DayNightCycleController_v2 controller;

    void Start()
    {
        if (Networking.IsMaster) {
            float time = Random.Range(0, 99);
            time = time / 100;
            controller.TimeSlider.value = time;
            controller.CurrentTimeOfDay = time;
            controller.SliderUpdated();
        }
    }
}
