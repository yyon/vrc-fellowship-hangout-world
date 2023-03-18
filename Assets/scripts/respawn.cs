
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class respawn : UdonSharpBehaviour
{
    public GameObject obj;
    public GameObject moveTo;

    void Interact() {
        Networking.SetOwner(Networking.LocalPlayer, obj);
        obj.transform.position = moveTo.transform.position;
        obj.transform.rotation = moveTo.transform.rotation;
	}
}
