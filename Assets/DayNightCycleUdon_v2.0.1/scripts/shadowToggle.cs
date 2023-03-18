
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class shadowToggle : UdonSharpBehaviour
{
    public Light light;
    public Toggle toggle;
    
    public void ShadowToggle()
    {
        if(toggle.isOn)
        {
            light.shadows = LightShadows.Soft;
        }
        else
        {
            light.shadows = LightShadows.None;
        }
    }
}
