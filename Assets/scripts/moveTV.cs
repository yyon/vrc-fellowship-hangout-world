
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class moveTV : UdonSharpBehaviour
{
    public GameObject TV;
    public GameObject moveTo;

    void Start()
    {
        
    }

    void OnPlayerTriggerEnter(VRCPlayerApi player) {
        if (player.isLocal) {
            TV.transform.position = moveTo.transform.position;
            TV.transform.rotation = moveTo.transform.rotation;
        }
    }
}
