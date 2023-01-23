
/*
 * Programmer:  labthe3rd
 * Date:        08/06/22
 * Description: Script that Triggers Event on Interact
 */


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Labthe3rd.InteractToggle
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InteractToggle : UdonSharpBehaviour
    {
        [Header("True State String For Toggle")]
        public string trueStateString;
        [Header("False State String For Toggle")]
        public string falseStateString;
        [Space]
        [Header("Udon Behavior Which Has Event You Want To Trigger")]
        public UdonBehaviour targetUdonBehavior;
        [Header("Event Name")]
        public string targetEvent;
        [Header("Bool You Will Be Watching In Target Udon Script")]
        public string targetBoolName;

        //State of variable we are watching
        private bool toggleState;
        private UdonBehaviour localUdonBehavior;

        void Start()
        {
            localUdonBehavior = this.GetComponent<UdonBehaviour>();
            toggleState = (bool)targetUdonBehavior.GetProgramVariable(targetBoolName);

            if (toggleState)
            {
                localUdonBehavior.InteractionText = trueStateString;
            }
            else
            {
                localUdonBehavior.InteractionText = falseStateString;
            }
        }

        public override void Interact()
        {
            targetUdonBehavior.SendCustomEvent(targetEvent);
            SendCustomEventDelayedSeconds("DelayedUpdate", 0.1f);
        }

        //Delay update slightly so event has a moment to run, this updates interaction text
        public void DelayedUpdate()
        {
            toggleState = (bool)targetUdonBehavior.GetProgramVariable(targetBoolName);

            if (toggleState)
            {
                localUdonBehavior.InteractionText = trueStateString;
            }
            else
            {
                localUdonBehavior.InteractionText = falseStateString;
            }
        }


    }
}

