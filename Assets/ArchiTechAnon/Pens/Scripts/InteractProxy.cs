
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace ArchiTech
{
    public class InteractProxy : UdonSharpBehaviour
    {
        [Tooltip("Udon Behavior that receives the custom event")]
        public UdonBehaviour target;
        public string eventName;
        new void Interact() => target.SendCustomEvent(eventName);
    }
}