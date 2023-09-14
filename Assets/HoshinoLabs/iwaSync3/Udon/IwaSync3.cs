using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace HoshinoLabs.IwaSync3.Udon
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class IwaSync3 : VideoControllerEventListener
    {
        public const string APP_NAME = "iwaSync3";
        public const string APP_VERSION = "V3.5.6(U#1.0)";

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [SerializeField, HideInInspector]
        GameObject _canvas1;
        [SerializeField, HideInInspector]
        GameObject _lockOn;
        [SerializeField, HideInInspector]
        Button _lockOnButton;
        [SerializeField, HideInInspector]
        GameObject _lockOff;
        [SerializeField, HideInInspector]
        Button _lockOffButton;
        [SerializeField, HideInInspector]
        Button _videoButton;
        [SerializeField, HideInInspector]
        Button _liveButton;

        [SerializeField, HideInInspector]
        GameObject _canvas2;
        [SerializeField, HideInInspector]
        GameObject _address;
        [SerializeField, HideInInspector]
        VRCUrlInputField _addressInput;
        [SerializeField, HideInInspector]
        GameObject _message;
        [SerializeField, HideInInspector]
        Text _messageText;
        [SerializeField, HideInInspector]
        GameObject _close;
        [SerializeField, HideInInspector]
        Button _closeButton;

        VRCPlayerApi _localPlayer;
        bool _local = false;
        uint _mode;
        [UdonSynced, FieldChangeCallback(nameof(on))]
        bool _on = false;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] Started `{nameof(IwaSync3)}`.");

            core.AddListener(this);

            _localPlayer = Networking.LocalPlayer;
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ValidateView();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (isOwner && _on && !_local)
                Close();
            ValidateView();
        }
        #endregion

        #region VideoEvent
        public override void OnVideoEnd()
        {
            ValidateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            ValidateView();
        }

        public override void OnVideoLoop()
        {
            ValidateView();
        }

        public override void OnVideoReady()
        {
            ValidateView();
        }

        public override void OnVideoStart()
        {
            ValidateView();
        }
        #endregion

        #region VideoCoreEvent
        public override void OnPlayerPlay()
        {
            ValidateView();
        }

        public override void OnPlayerPause()
        {
            ValidateView();
        }

        public override void OnPlayerStop()
        {
            ValidateView();
        }

        public override void OnChangeURL()
        {
            ValidateView();
        }

        public override void OnChangeLock()
        {
            ValidateView();
        }
        #endregion

        public void TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public bool isOwner => Networking.IsOwner(gameObject);

        void ValidateView()
        {
            var locked = core.locked;
            var master = Networking.IsMaster || (core.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            var flag1 = !core.isPrepared && !core.isPlaying && !core.isError;
            var flag2 = flag1 && !_on;
            var nlocked = !locked;
            var flag3 = flag1 && _on;
            var nisOwner = !isOwner;
            var flag4 = isOwner || privilege;

            if (_canvas1.activeSelf != flag2)
                _canvas1.SetActive(flag2);
            if (_lockOn.activeSelf != nlocked)
                _lockOn.SetActive(nlocked);
            if (_lockOnButton.interactable != master)
                _lockOnButton.interactable = master;
            if (_lockOff.activeSelf != locked)
                _lockOff.SetActive(locked);
            if (_lockOffButton.interactable != master)
                _lockOffButton.interactable = master;
            if (_videoButton.interactable != privilege)
                _videoButton.interactable = privilege;
            if (_liveButton.interactable != privilege)
                _liveButton.interactable = privilege;

            if (_canvas2.activeSelf != flag3)
                _canvas2.SetActive(flag3);
            if (_address.activeSelf != isOwner)
                _address.SetActive(isOwner);
            if (_message.activeSelf != nisOwner)
                _message.SetActive(nisOwner);
            var player = Networking.GetOwner(gameObject);
            _messageText.text = $"Entering the URL... ({(player == null ? string.Empty : player.displayName)})";
            if (_closeButton.interactable != flag4)
                _closeButton.interactable = flag4;
        }

        public void LockOn()
        {
            controller.LockOn();
        }

        public void LockOff()
        {
            controller.LockOff();
        }

        public bool on
        {
            get => _on;
            private set
            {
                _on = value;
                UpdateOn();
            }
        }

        void UpdateOn()
        {
            ValidateView();
        }

        public void ModeVideo()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] The mode has changed to `MODE_VIDEO`.");
            TakeOwnership();
            _local = true;
            _mode =
                #if COMPILER_UDONSHARP
                core.MODE_VIDEO
                #else
                VideoCore.MODE_VIDEO
                #endif
            ;
            _on = true;
            RequestSerialization();
            ValidateView();
        }

        public void ModeLive()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] The mode has changed to `MODE_STREAM`.");
            TakeOwnership();
            _local = true;
            _mode =
                #if COMPILER_UDONSHARP
                core.MODE_STREAM
                #else
                VideoCore.MODE_STREAM
                #endif
            ;
            _on = true;
            RequestSerialization();
            ValidateView();
        }

        public void OnURLChanged()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] The url has changed to `{_addressInput.GetUrl().Get()}`.");
            core.TakeOwnership();
            core.PlayURL(_mode, _addressInput.GetUrl());
            core.RequestSerialization();
            ValidateView();
        }

        public void ClearURL()
        {
            _addressInput.SetUrl(VRCUrl.Empty);
        }

        public void Close()
        {
            Debug.Log($"[<color=#47F1FF>{APP_NAME}</color>] Trigger a close event.");
            TakeOwnership();
            _local = false;
            _on = false;
            RequestSerialization();
            ValidateView();
        }
    }
}
