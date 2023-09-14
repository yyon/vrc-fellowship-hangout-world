using System;
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
    public class Queuelist : VideoControllerEventListener
    {
        #region UExtenderLite
        uint[] _ArrayUtility_Add_uintarray_uint(uint[] _0, uint _1)
        {
            var result = new uint[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }
        VRCUrl[] _ArrayUtility_Add_VRCUrlarray_VRCUrl(VRCUrl[] _0, VRCUrl _1)
        {
            var result = new VRCUrl[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }
        GameObject[] _ArrayUtility_Add_GameObjectarray_GameObject(GameObject[] _0, GameObject _1)
        {
            var result = new GameObject[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }
        Button[] _ArrayUtility_Add_Buttonarray_Button(Button[] _0, Button _1)
        {
            var result = new Button[_0.Length + 1];
            Array.Copy(_0, result, _0.Length);
            result[_0.Length] = _1;
            return result;
        }

        uint[] _ArrayUtility_RemoveAt_uintarray_int(uint[] _0, int _1)
        {
            var result = new uint[_0.Length - 1];
            Array.Copy(_0, result, _1);
            Array.Copy(_0, _1 + 1, result, _1, _0.Length - _1 - 1);
            return result;
        }
        GameObject[] _ArrayUtility_RemoveAt_GameObjectarray_int(GameObject[] _0, int _1)
        {
            var result = new GameObject[_0.Length - 1];
            Array.Copy(_0, result, _1);
            Array.Copy(_0, _1 + 1, result, _1, _0.Length - _1 - 1);
            return result;
        }
        Button[] _ArrayUtility_RemoveAt_Buttonarray_int(Button[] _0, int _1)
        {
            var result = new Button[_0.Length - 1];
            Array.Copy(_0, result, _1);
            Array.Copy(_0, _1 + 1, result, _1, _0.Length - _1 - 1);
            return result;
        }
        VRCUrl[] _ArrayUtility_RemoveAt_VRCUrlarray_int(VRCUrl[] _0, int _1)
        {
            var result = new VRCUrl[_0.Length - 1];
            Array.Copy(_0, result, _1);
            Array.Copy(_0, _1 + 1, result, _1, _0.Length - _1 - 1);
            return result;
        }

        GameObject _Instantiate(GameObject _0, Transform _1, bool _2)
        {
            var instantiatedObject = Instantiate(_0);
            var objectTransform = instantiatedObject.transform;
            objectTransform.SetParent(_1, _2);

            if (!_2)
            {
                var originalTransform = _0.transform;
                var originalPosition = originalTransform.position;
                var originalRotation = originalTransform.rotation;
                instantiatedObject.transform.SetPositionAndRotation(originalPosition, originalRotation);
                instantiatedObject.transform.localScale = originalTransform.localScale;// May not behave the same as C#
            }

            return instantiatedObject;
        }
        #endregion

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [SerializeField, HideInInspector]
        GameObject _lockOn;
        [SerializeField, HideInInspector]
        Button _lockOnButton;
        [SerializeField, HideInInspector]
        GameObject _lockOff;
        [SerializeField, HideInInspector]
        Button _lockOffButton;
        [SerializeField, HideInInspector]
        GameObject _playOn;
        [SerializeField, HideInInspector]
        Button _playOnButton;
        [SerializeField, HideInInspector]
        GameObject _playOff;
        [SerializeField, HideInInspector]
        Button _playOffButton;
        [SerializeField, HideInInspector]
        GameObject _forward;
        [SerializeField, HideInInspector]
        Button _forwardButton;
        [SerializeField, HideInInspector]
        Transform _content;

        [SerializeField, HideInInspector]
        GameObject _template;

        int _content_length = 0;
        GameObject[] _content_objs = new GameObject[0];
        Button[] _content_buttons = new Button[0];

        [SerializeField, HideInInspector]
        GameObject _message;
        [SerializeField, HideInInspector]
        Text _messageText;

        GameObject _obj = null;
        VRCUrlInputField _addressInput;

        Slider _progressSlider;

        [UdonSynced]
        bool _controlled = false;
        bool _wait = false;

        [UdonSynced]
        uint[] _modes = new uint[0];
        [UdonSynced]
        VRCUrl[] _urls = new VRCUrl[0];
        [UdonSynced]
        int _track;

        VRCPlayerApi _localPlayer;
        bool _local = false;
        uint _mode;
        [UdonSynced]
        bool _on = false;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Started `{nameof(Queuelist)}`.");

            core.AddListener(this);

            _obj = GenerateChildItem();
            _addressInput = (VRCUrlInputField)_obj.transform.Find("Panel (2)/Panel/Address").GetComponent(typeof(VRCUrlInputField));

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
            if (isOwner && on && !_local)
                Close();
            ValidateView();
        }
        #endregion

        #region VideoEvent
        public override void OnVideoEnd()
        {
            if (isOwner && _controlled)
                Forward();
            ValidateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            if (isOwner && _controlled && core.errorRetry == 0)
                Forward();
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
            if (isOwner && _controlled)
                PlayOff();
            ValidateView();
        }

        public override void OnChangeURL()
        {
            _messageText.text = $"Loading Now";
            if (isOwner && _controlled && !core.isReload && !_wait)
                PlayOff();
            _wait = false;
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

            var nlocked = !locked;
            var ncontrolled = !_controlled;
            var isPreparedANDnisError = core.isPrepared && !core.isError;
            var privilegeANDnisTrackDoneAll = privilege && !IsTrackDoneAll();
            var privilegeANDIsTrackPlaying = privilege && IsTrackPlaying(_track);

            if (_lockOn.activeSelf != nlocked)
                _lockOn.SetActive(nlocked);
            if (_lockOnButton.interactable != master)
                _lockOnButton.interactable = master;
            if (_lockOff.activeSelf != locked)
                _lockOff.SetActive(locked);
            if (_lockOffButton.interactable != master)
                _lockOffButton.interactable = master;
            if (_playOn.activeSelf != ncontrolled)
                _playOn.SetActive(ncontrolled);
            if (_playOnButton.interactable != privilegeANDnisTrackDoneAll)
                _playOnButton.interactable = privilegeANDnisTrackDoneAll;
            if (_playOff.activeSelf != _controlled)
                _playOff.SetActive(_controlled);
            if (_playOffButton.interactable != privilege)
                _playOffButton.interactable = privilege;
            if (_forward.activeSelf != _controlled)
                _forward.SetActive(_controlled);
            if (_forwardButton.interactable != privilegeANDIsTrackPlaying)
                _forwardButton.interactable = privilegeANDIsTrackPlaying;
            if (_message.activeSelf != isPreparedANDnisError)
                _message.SetActive(isPreparedANDnisError);

            UpdateTracks();
            UpdateTrack();
        }

        private void Update()
        {
            if (!_controlled)
                return;

            if (!core.isPlaying)
                return;

            if (core.isLive)
            {
                _progressSlider.value = 1f;
            }
            else
            {
                var duration = core.duration;
                if (core.duration != 0f)
                {
                    var time = core.time;
                    _progressSlider.value = time / duration;
                }
            }
        }

        public void LockOn()
        {
            controller.LockOn();
        }

        public void LockOff()
        {
            controller.LockOff();
        }

        public void PlayOn()
        {
            TakeOwnership();
            PlayTracks(GetTrackNext());
            RequestSerialization();
            ValidateView();
        }

        public void PlayOff()
        {
            TakeOwnership();
            StopTracks();
            RequestSerialization();
            ValidateView();
        }

        public void Forward()
        {
            TakeOwnership();
            TrackDone();
            PlayTracks(GetTrackNext());
            RequestSerialization();
            ValidateView();
        }

        public void OnButtonClicked()
        {
            var sender = FindSender();
            if (sender < 0)
                return;

            TakeOwnership();
            PlayTracks(sender);
            RequestSerialization();
            ValidateView();
        }

        int FindSender()
        {
            for (var i = 0; i < _content_length; i++)
            {
                if (!_content_buttons[i].enabled)
                    return i;
            }
            return -1;
        }

        void UpdateTracks()
        {
            var locked = core.locked;
            var master = Networking.IsMaster || (core.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            var playings = GetTrackPlayings();
            for (var i = 0; i < _urls.Length; i++)
            {
                if (_content_length <= i)
                {
                    var content_obj = GenerateChildItem();
                    _content_objs = _ArrayUtility_Add_GameObjectarray_GameObject(_content_objs, content_obj);
                    _content_length++;
                    var content_button = content_obj.transform.Find("Panel/Button").GetComponent<Button>();
                    _content_buttons = _ArrayUtility_Add_Buttonarray_Button(_content_buttons, content_button);
                }
                var obj = _content_objs[i];
                obj.transform.SetSiblingIndex(i);
                RefreshChildItem(obj, privilege, playings[i], _urls[i]);
            }
            _obj.transform.SetSiblingIndex(_urls.Length);
            RefreshChildLastItem(_obj, privilege);
            for (var i = _content_length - 1; _urls.Length <= i; i--)
                DestroyChildItem(i);
        }

        GameObject GenerateChildItem()
        {
            var obj = _Instantiate(_template, _content, false);
            obj.SetActive(true);
            return obj;
        }

        void RefreshChildItem(GameObject obj, bool privilege, bool playing, VRCUrl url)
        {
            var panel1 = obj.transform.Find("Panel").gameObject;
            panel1.SetActive(true);
            var buttonButton = panel1.transform.Find("Button").GetComponent<Button>();
            buttonButton.interactable = privilege && !playing;
            var progressSlider = panel1.transform.Find("Button/Progress").GetComponent<Slider>();
            progressSlider.value = 0f;
            var trackText = panel1.transform.Find("Button/Panel/Text").GetComponent<Text>();
            trackText.text = url.Get();
            var removeButton = panel1.transform.Find("Button/Panel/Remove/Button").GetComponent<Button>();
            removeButton.interactable = privilege;

            var panel2 = obj.transform.Find("Panel (1)").gameObject;
            panel2.SetActive(false);

            var panel3 = obj.transform.Find("Panel (2)").gameObject;
            panel3.SetActive(false);
        }

        void RefreshChildLastItem(GameObject obj, bool privilege)
        {
            var panel1 = obj.transform.Find("Panel").gameObject;
            panel1.SetActive(false);

            var panel2 = obj.transform.Find("Panel (1)").gameObject;
            panel2.SetActive(!_on);
            var videoButton = panel2.transform.Find("Panel/Video/Button").GetComponent<Button>();
            videoButton.interactable = privilege;
            var liveButton = panel2.transform.Find("Panel/Live/Button").GetComponent<Button>();
            liveButton.interactable = privilege;

            var panel3 = obj.transform.Find("Panel (2)").gameObject;
            panel3.SetActive(_on);
            var address = panel3.transform.Find("Panel/Address").gameObject;
            address.SetActive(isOwner);
            var message = panel3.transform.Find("Panel/Message").gameObject;
            message.SetActive(!isOwner);
            var messageText = message.GetComponent<Text>();
            var player = Networking.GetOwner(gameObject);
            messageText.text = $"Entering the URL... ({(player == null ? string.Empty : player.displayName)})";
            var closeButton = panel3.transform.Find("Panel/Close/Button").GetComponent<Button>();
            closeButton.interactable = isOwner || privilege;
        }

        void DestroyChildItem(int index)
        {
            var obj = _content_objs[index];
            Destroy(obj.gameObject);
            _content_objs = _ArrayUtility_RemoveAt_GameObjectarray_int(_content_objs, index);
            _content_length--;
            _content_buttons = _ArrayUtility_RemoveAt_Buttonarray_int(_content_buttons, index);
        }

        void ReorderTracks()
        {
            if (!IsTrackAvailable(_track))
                return;
            var tmp1 = new uint[_modes.Length];
            Array.Copy(_modes, _track, tmp1, 0, _modes.Length - _track);
            Array.Copy(_modes, 0, tmp1, _modes.Length - _track, _track);
            _modes = tmp1;
            var tmp2 = new VRCUrl[_urls.Length];
            Array.Copy(_urls, _track, tmp2, 0, _urls.Length - _track);
            Array.Copy(_urls, 0, tmp2, _urls.Length - _track, _track);
            _urls = tmp2;
        }

        void PlayTracks(int track)
        {
            _controlled = false;
            _wait = false;
            StopTrack(true);
            if (!IsTrackDoneAll() && IsTrackAvailable(track))
            {
                _controlled = true;
                _wait = true;
                _track = track;
                ReorderTracks();
                _track = GetTrackNext();
                UpdateTrack();
            }
            PlayTrack();
        }

        void StopTracks()
        {
            var playing = IsTrackPlaying(_track);
            _controlled = false;
            _wait = false;
            StopTrack(playing);
            UpdateTracks();
        }

        void UpdateTrack()
        {
            if (!_controlled || !IsTrackAvailable(_track))
                return;

            var obj = _content_objs[_track];
            var panel1 = obj.transform.Find("Panel").gameObject;
            _progressSlider = panel1.transform.Find("Button/Progress").GetComponent<Slider>();
        }

        void PlayTrack()
        {
            if (!_controlled || !IsTrackAvailable(_track))
                return;

            core.TakeOwnership();
            core.PlayURL(_modes[_track], _urls[_track]);
            core.RequestSerialization();
        }

        void StopTrack(bool force)
        {
            if (!force && !IsTrackPlaying(_track))
                return;

            core.TakeOwnership();
            core.Stop();
            core.RequestSerialization();
        }

        bool IsTrackDoneAll()
        {
            return _urls == null || _urls.Length == 0;
        }

        bool IsTrackAvailable(int track)
        {
            if (IsTrackDoneAll())
                return false;
            return 0 <= track && track < _urls.Length;
        }

        bool[] GetTrackAvailables()
        {
            var results = new bool[_urls.Length];
            if (IsTrackDoneAll())
                return results;
            for (var i = 0; i < _urls.Length; i++)
                results[i] = true;
            return results;
        }

        bool IsTrackPlaying(int track)
        {
            if (IsTrackDoneAll())
                return false;
            if (!core.isPrepared && !core.isPlaying && !core.isError)
                return false;
            return _controlled && IsTrackAvailable(track) && track == _track;
        }

        bool[] GetTrackPlayings()
        {
            var results = new bool[_urls.Length];
            if (IsTrackDoneAll())
                return results;
            if (!core.isPrepared && !core.isPlaying && !core.isError)
                return results;
            var availables = GetTrackAvailables();
            for (var i = 0; i < _urls.Length; i++)
                results[i] = _controlled && availables[i] && i == _track;
            return results;
        }

        void TrackDone()
        {
            if (IsTrackDoneAll())
                return;
            TrackDoneAt(_track);
        }

        void TrackDoneAt(int track)
        {
            if (IsTrackDoneAll())
                return;
            _modes = _ArrayUtility_RemoveAt_uintarray_int(_modes, track);
            _urls = _ArrayUtility_RemoveAt_VRCUrlarray_int(_urls, track);
        }

        int GetTrackNext()
        {
            if (IsTrackDoneAll())
                return -1;
            return 0;
        }

        public void OnURLRemoved()
        {
            var sender = FindSender();
            if (sender < 0)
                return;

            var track = sender;
            if (IsTrackPlaying(track))
            {
                Forward();
                return;
            }
            TakeOwnership();
            TrackDoneAt(track);
            UpdateTracks();
            RequestSerialization();
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
            var url = _addressInput.GetUrl();
            if (!core.IsValidURL(url.Get()))
                return;

            TakeOwnership();
            _modes = _ArrayUtility_Add_uintarray_uint(_modes, _mode);
            _urls = _ArrayUtility_Add_VRCUrlarray_VRCUrl(_urls, url);
            _local = false;
            _on = false;
            if(!core.isPrepared && !core.isPlaying && !core.isError && _urls.Length == 1)
                PlayTracks(GetTrackNext());
            RequestSerialization();
            ValidateView();
        }

        public void ClearURL()
        {
            _addressInput.SetUrl(VRCUrl.Empty);
        }

        public void Close()
        {
            TakeOwnership();
            _local = false;
            _on = false;
            RequestSerialization();
            ValidateView();
        }
    }
}
