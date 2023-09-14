using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace HoshinoLabs.IwaSync3.Udon
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ListTab : VideoControllerEventListener
    {
        #region UExtenderLite
        bool[] _ArrayUtility_Add_boolarray_bool(bool[] _0, bool _1)
        {
            var result = new bool[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }
        #endregion

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [Header("Options")]
        [SerializeField]
        bool allowSwitchOff = false;
        [SerializeField]
        bool multiView = false;

        [SerializeField, HideInInspector]
        GameObject _lockOn;
        [SerializeField, HideInInspector]
        Button _lockOnButton;
        [SerializeField, HideInInspector]
        GameObject _lockOff;
        [SerializeField, HideInInspector]
        Button _lockOffButton;
        [SerializeField, HideInInspector]
        Transform _content;

        [SerializeField, HideInInspector]
        int _content_length;
        //[SerializeField, HideInInspector]
        //GameObject[] _content_objs;
        [SerializeField, HideInInspector]
        Toggle[] _content_toggles;
        [SerializeField, HideInInspector]
        GameObject[] _content_lists;

        [SerializeField, HideInInspector]
        GameObject _message;
        [SerializeField, HideInInspector]
        Text _messageText;

        [UdonSynced, HideInInspector, SerializeField]
        bool[] _tabs;

        bool _updatingTabs = false;

        VRCPlayerApi _localPlayer;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>iwaSync3</color>] Started `{nameof(ListTab)}`.");

            core.AddListener(this);

            _localPlayer = Networking.LocalPlayer;
        }

        public override void OnDeserialization()
        {
            ValidateView();
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ValidateView();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
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

        public override void OnVideoStart()
        {
            ValidateView();
        }
        #endregion

        #region VideoCoreEvent
        public override void OnChangeURL()
        {
            _messageText.text = $"Loading Now";
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

            var nlocked = !locked;
            var isPreparedANDnisError = core.isPrepared && !core.isError;

            if (_lockOn.activeSelf != nlocked)
                _lockOn.SetActive(nlocked);
            if (_lockOnButton.interactable != master)
                _lockOnButton.interactable = master;
            if (_lockOff.activeSelf != locked)
                _lockOff.SetActive(locked);
            if (_lockOffButton.interactable != master)
                _lockOffButton.interactable = master;
            if (_message.activeSelf != isPreparedANDnisError)
                _message.SetActive(isPreparedANDnisError);

            UpdateTabs();
        }

        public void LockOn()
        {
            controller.LockOn();
        }

        public void LockOff()
        {
            controller.LockOff();
        }

        public void OnButtonClicked()
        {
            if (_updatingTabs)
                return;

            var sender = FindSender();
            if (sender < 0)
                return;

            TakeOwnership();
            ChoiceTab(sender);
            RequestSerialization();
            ValidateView();
        }

        int FindSender()
        {
            for (var i = 0; i < _content_length; i++)
            {
                if (!_content_toggles[i].enabled)
                    return i;
            }
            return -1;
        }

        void UpdateTabs()
        {
            _updatingTabs = true;

            var locked = core.locked;
            var master = Networking.IsMaster || (core.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            for (var i = 0; i < _tabs.Length; i++)
            {
                var obj = _content.GetChild(i + 1);
                if (!obj.gameObject.activeSelf)
                    continue;
                var toggle = obj.Find("Toggle").gameObject;
                var toggleToggle = toggle.GetComponent<Toggle>();
                toggleToggle.interactable = privilege;
                toggleToggle.isOn = _tabs[i];
                var active = _tabs[i];
                if (_content_lists[i] != null && _content_lists[i].activeSelf != active)
                    _content_lists[i].SetActive(active);
            }

            _updatingTabs = false;
        }

        void ChoiceTab(int tab)
        {
            if (!allowSwitchOff)
            {
                var cnt = 0;
                for (var i = 0; i < _tabs.Length; i++)
                    cnt += i == tab ? (_tabs[i] ? 0 : 1) : (_tabs[i] ? 1 : 0);
                if (cnt <= 0)
                    return;
            }
            for (var i = 0; i < _tabs.Length; i++)
                _tabs[i] = multiView || i == tab ? _tabs[i] : false;
            _tabs[tab] = !_tabs[tab];
        }
    }
}
