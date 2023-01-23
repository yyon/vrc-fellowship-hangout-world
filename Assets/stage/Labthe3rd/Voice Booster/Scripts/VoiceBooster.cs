
/*
 * Programmer:  Labthe3rd
 * Date:        01/26/22
 * Description: Stage boosting script which can be triggered multiple ways
 * 
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;


namespace Labthe3rd.VoiceBooster
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VoiceBooster : UdonSharpBehaviour
    {
        [Header("Voice Values When Amplifier Is Turned On")]
        public float targetNear = 39;
        public float targetFar = 40;
        public float targetGain = 20;
        public bool targetLowpass = false;

        [Space]

        [Header("Default Voice Values In Your World")]
        public float defaultNear = 0;
        public float defaultFar = 25;
        public float defaultGain = 15;
        public bool defaultLowpass = true;

        //Local VRChat Player We Grab At Start
        private VRCPlayerApi localplayer;
        //Local Player ID Which We Will Use To Sync
        private int localPlayerID;

        //public variable to keep track of script amplification state for buttons, UI, etc
        [HideInInspector]public bool toggleState = false;

        //Keep track of player that was amplified and not amplified
        private VRCPlayerApi amplifyPlayer;
        private VRCPlayerApi noAmplifyPlayer;


        //Values we will be syncing to determine amplified and not amplified player
        [UdonSynced, FieldChangeCallback(nameof(amplifyID))]
        private int _amplifyID;
        [UdonSynced, FieldChangeCallback(nameof(noAmplifyID))]
        private int _noAmplifyID;


        void Start()
        {
            if (Utilities.IsValid(Networking.LocalPlayer))
            {
                localplayer = Networking.LocalPlayer;
                localPlayerID = Networking.LocalPlayer.playerId;
            }
        }

        public void Trigger()
        {

            if (Networking.IsClogged == false && Networking.IsNetworkSettled == true) //make sure network is ready
            {
                if (Networking.IsOwner(gameObject) == false)
                {
                    Networking.SetOwner(localplayer, gameObject);
                }

                if (Networking.IsOwner(gameObject) == true)
                {
                    if (toggleState == false)
                    {

                        toggleState = true;

                        if (localPlayerID == amplifyID)
                        {
                            Debug.Log("ID already matches amplifyID, forcing sync");
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "AmplifyVoice");
                        }
                        else
                        {
                            amplifyID = localPlayerID;
                            Debug.Log("Setting AmplifyID to " + amplifyID);
                            RequestSerialization();
                        }

                    }

                    else
                    {
                        toggleState = false;
                        if (localPlayerID == noAmplifyID)
                        {
                            Debug.Log("ID already matches noAmplifyID, forcing sync");
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DeAmplifyVoice");
                        }
                        else
                        {
                            noAmplifyID = localPlayerID;
                            Debug.Log("Setting noAmplifyID to " + noAmplifyID);
                            RequestSerialization();
                        }

                    }
                }

            }




        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsOwner(this.gameObject))
            {
                if (Networking.IsClogged == false && Networking.IsNetworkSettled)
                {
                    Debug.Log("Network not clogged and settled, owner serializing booster");
                    RequestSerialization();
                }
                else
                {
                    Debug.Log("Network clogged or not settled, voice booster serialization failed");
                }

            }
        }

        public void AmplifyVoice()
        {
            if (Utilities.IsValid(VRCPlayerApi.GetPlayerById(amplifyID)))
            {
                amplifyPlayer = VRCPlayerApi.GetPlayerById(amplifyID);
                Debug.Log("Amplifying " + amplifyPlayer.displayName + "'s voice");
                amplifyPlayer.SetVoiceDistanceFar(40f);
                amplifyPlayer.SetVoiceDistanceNear(39f);
                amplifyPlayer.SetVoiceGain(18f);

            }

        }

        public void DeAmplifyVoice()
        {
            if (Utilities.IsValid(VRCPlayerApi.GetPlayerById(noAmplifyID)))
            {
                noAmplifyPlayer = VRCPlayerApi.GetPlayerById(noAmplifyID);
                Debug.Log("DeAmplifying " + noAmplifyPlayer.displayName + "'s voice");
                noAmplifyPlayer.SetVoiceDistanceFar(defaultFar);
                noAmplifyPlayer.SetVoiceDistanceNear(defaultNear);
                noAmplifyPlayer.SetVoiceGain(defaultGain);
            }

        }

        public int amplifyID
        {
            set
            {
                _amplifyID = value;
                Debug.Log("Amplify ID set to " + _amplifyID);
                AmplifyVoice();

            }

            get => _amplifyID;
        }

        public int noAmplifyID
        {
            set
            {
                _noAmplifyID = value;
                Debug.Log("Noamplify ID set to " + _noAmplifyID);
                DeAmplifyVoice();

            }

            get => _noAmplifyID;
        }



        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            return true;
        }
    }
}

