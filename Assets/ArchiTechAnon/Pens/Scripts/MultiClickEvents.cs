
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ArchiTech
{
    // [RequireComponent(typeof(VRC_Pickup))]
    public class MultiClickEvents : UdonSharpBehaviour
    {
        [Tooltip("Script you wish to have the events triggered on.")]
        public UdonSharpBehaviour eventTarget;
        [Tooltip("Time required to pass between clicks for the events to be triggered. Each successive click must occur within the threshold after the previous one to increase the multi-click count. The less this value is the faster one has to click.")]
        public int multiClickThresholdInMs = 400;
        [Tooltip("Enable explicitly sending the normal OnPickupUseDownImmediate/OnPickupUseUpImmediate events to the Event Target, skipping the delay threshold entirely.")]
        public bool sendOnPickupUseEvents = false;
        private int[] clicks = new int[0];
        private bool isHeld = false;
        private bool skipLog = false;

        private void log(string value)
        {
            if (!skipLog) Debug.Log("[<color=#44ff66>MultiClickEvents</color>] " + value);
        }

        void Start()
        {
            if (eventTarget == null)
                log("ERROR: Must specify what script you wish to receive the events.");
        }

        void Update()
        {
            if (clicks.Length == 0) return; // skip checks if there are no clicks
            var netTime = Networking.GetServerTimeInMilliseconds();
            var clickTime = clicks[clicks.Length - 1];
            if (netTime > clickTime)
            {
                var e = "OnPickupUse" + clicks.Length + (isHeld ? "Hold" : "");
                log("Event triggered: " + e);
                clicks = new int[0]; // clear clicks list
                eventTarget.SendCustomEvent(e);
            }
        }

        new void OnPickupUseDown()
        {
            isHeld = true;
            var newClicks = new int[clicks.Length + 1];
            for (int i = 0; i < clicks.Length; i++)
            {
                newClicks[i] = clicks[i];
            }
            newClicks[newClicks.Length - 1] = Networking.GetServerTimeInMilliseconds() + multiClickThresholdInMs;
            clicks = newClicks;
            log("Clicked count: " + newClicks.Length);
            if (sendOnPickupUseEvents)
            {
                eventTarget.SendCustomEvent("OnPickupUseDownImmediate");
            }
        }

        new void OnPickupUseUp()
        {
            isHeld = false;
            if (sendOnPickupUseEvents)
            {
                eventTarget.SendCustomEvent("OnPickupUseUpImmediate");
            }
        }
    }
}
