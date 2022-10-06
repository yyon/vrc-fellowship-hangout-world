
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class blinkingLights : UdonSharpBehaviour
{
    public DayNightCycleController_v2 controller;
    public GameObject[] lights;
    int time;


    void Start()
    {
        
    }

    void FixedUpdate()
    {
        time++;

        for (int i = 0; i < lights.Length; i++) {
            GameObject light = lights[i];

            int updateTime = (time + i*57);
            int intervalTime = (100 + ((i*53)%100));
            if (updateTime % intervalTime == 0) {
                int updateTick = updateTime / intervalTime;
                bool onOff = (updateTick % 2) == 0;

                if (controller.CurrentTimeOfDay >= .25 && controller.CurrentTimeOfDay <= .75) {
                    onOff = false;
                }

                light.SetActive(onOff);
            }
        }
    }
}
