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
    public class Playlist : VideoControllerEventListener
    {
        #region UExtenderLite
        int[] _ArrayUtility_Add_intarray_int(int[] _0, int _1)
        {
            var result = new int[_0.Length + 1];
            _0.CopyTo(result, 0);
            result[_0.Length] = _1;
            return result;
        }

        bool _ArrayUtility_Contains_intarray_int(int[] _0, int _1)
        {
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    return true;
            }
            return false;
        }

        int ArrayUtility_IndexOf_intarray_int(int[] _0, int _1)
        {
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    return i;
            }
            return -1;
        }
        int ArrayUtility_IndexOf_VRCUrlarray_VRCUrl(VRCUrl[] _0, VRCUrl _1)
        {
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i].Get() == _1.Get())
                    return i;
            }
            return -1;
        }

        int[] _ArrayUtility_Remove_intarray_int(int[] _0, int _1)
        {
            var cnt = 0;
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    continue;
                cnt++;
            }
            var result = new int[cnt];
            var idx = 0;
            for (var i = 0; i < _0.Length; i++)
            {
                if (_0[i] == _1)
                    continue;
                result[idx] = _0[i];
                idx++;
            }
            return result;
        }
        #endregion

        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [Header("Options")]
        [SerializeField, UdonSynced]
        bool defaultOrder = false;
        [SerializeField, UdonSynced]
        bool defaultShuffle = false;
        [SerializeField, UdonSynced]
        bool defaultRepeat = true;
        [SerializeField]
        bool playOnAwake = false;

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
        GameObject _orderOn;
        [SerializeField, HideInInspector]
        Button _orderOnButton;
        [SerializeField, HideInInspector]
        GameObject _orderOff;
        [SerializeField, HideInInspector]
        Button _orderOffButton;
        [SerializeField, HideInInspector]
        GameObject _shuffleOn;
        [SerializeField, HideInInspector]
        Button _shuffleOnButton;
        [SerializeField, HideInInspector]
        GameObject _shuffleOff;
        [SerializeField, HideInInspector]
        Button _shuffleOffButton;
        [SerializeField, HideInInspector]
        GameObject _repeatOn;
        [SerializeField, HideInInspector]
        Button _repeatOnButton;
        [SerializeField, HideInInspector]
        GameObject _repeatOff;
        [SerializeField, HideInInspector]
        Button _repeatOffButton;
        [SerializeField, HideInInspector]
        Transform _content;

        [SerializeField, HideInInspector]
        int _content_length;
        //[SerializeField, HideInInspector]
        //GameObject[] _content_objs;
        [SerializeField, HideInInspector]
        Button[] _content_buttons;
        [SerializeField, HideInInspector]
        Slider[] _content_sliders;
        [SerializeField, HideInInspector]
        string[] _content_titles;
        [SerializeField, HideInInspector]
        uint[] _content_modes;
        [SerializeField, HideInInspector]
        VRCUrl[] _content_urls;

        [SerializeField, HideInInspector]
        GameObject _message;
        [SerializeField, HideInInspector]
        Text _messageText;

        Slider _progressSlider;

        [UdonSynced]
        bool _controlled = false;
        bool _wait = false;

        [UdonSynced]
        int[] _tracks;
        [UdonSynced]
        int _track;

        VRCPlayerApi _localPlayer;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Started `{nameof(Playlist)}`.");

            core.AddListener(this);

            _localPlayer = Networking.LocalPlayer;

            ClearTracks();
        }

        public override void OnDeserialization()
        {
            ValidateView();
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.IsMaster/* && isOwner*/ && player.isLocal && playOnAwake && !IsTrackDoneAll())
                PlayOn();
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
            if (_controlled)
            {
                var idx = ArrayUtility_IndexOf_VRCUrlarray_VRCUrl(_content_urls, core.url);
                if (0 <= idx)
                    core.Message = _content_titles[idx];
            }
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
            if (_controlled && isOwner && !core.isReload && !_wait)
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
            var ndefaultOrder = !defaultOrder;
            var ndefaultShuffle = !defaultShuffle;
            var ndefaultRepeat = !defaultRepeat;
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
            if (_orderOn.activeSelf != defaultOrder)
                _orderOn.SetActive(defaultOrder);
            if (_orderOnButton.interactable != privilege)
                _orderOnButton.interactable = privilege;
            if (_orderOff.activeSelf != ndefaultOrder)
                _orderOff.SetActive(ndefaultOrder);
            if (_orderOffButton.interactable != privilege)
                _orderOffButton.interactable = privilege;
            if (_shuffleOn.activeSelf != ndefaultShuffle)
                _shuffleOn.SetActive(ndefaultShuffle);
            if (_shuffleOnButton.interactable != privilege)
                _shuffleOnButton.interactable = privilege;
            if (_shuffleOff.activeSelf != defaultShuffle)
                _shuffleOff.SetActive(defaultShuffle);
            if (_shuffleOffButton.interactable != privilege)
                _shuffleOffButton.interactable = privilege;
            if (_repeatOn.activeSelf != ndefaultRepeat)
                _repeatOn.SetActive(ndefaultRepeat);
            if (_repeatOnButton.interactable != privilege)
                _repeatOnButton.interactable = privilege;
            if (_repeatOff.activeSelf != defaultRepeat)
                _repeatOff.SetActive(defaultRepeat);
            if (_repeatOffButton.interactable != privilege)
                _repeatOffButton.interactable = privilege;
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
            _track = -1;
            ClearTracks();
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
            if (IsTrackDoneAll() && defaultRepeat)
            {
                _track = -1;
                ClearTracks();
                ReorderTracks();
            }
            PlayTracks(GetTrackNext());
            RequestSerialization();
            ValidateView();
        }

        public void OrderOn()
        {
            TakeOwnership();
            defaultOrder = true;
            RequestSerialization();
            ValidateView();
        }

        public void OrderOff()
        {
            TakeOwnership();
            defaultOrder = false;
            RequestSerialization();
            ValidateView();
        }

        public void ShuffleOn()
        {
            TakeOwnership();
            defaultShuffle = true;
            RequestSerialization();
            ValidateView();
        }

        public void ShuffleOff()
        {
            TakeOwnership();
            defaultShuffle = false;
            RequestSerialization();
            ValidateView();
        }

        public void RepeatOn()
        {
            TakeOwnership();
            defaultRepeat = true;
            RequestSerialization();
            ValidateView();
        }

        public void RepeatOff()
        {
            TakeOwnership();
            defaultRepeat = false;
            RequestSerialization();
            ValidateView();
        }

        public void OnButtonClicked()
        {
            var sender = FindSender();
            if (sender < 0)
                return;

            TakeOwnership();
            ClearTracks();
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

            var dones = GetTrackDones();
            var playings = GetTrackPlayings();
            for (var i = 0; i < _content_length; i++)
            {
                var button = _content_buttons[i];
                var interactable = privilege && !dones[i] && !playings[i];
                if (button.interactable != interactable)
                    button.interactable = interactable;
                var slider = _content_sliders[i];
                var value = dones[i] ? 1f : 0f;
                if (slider.value != value)
                    slider.value = value;
            }
        }

        void ClearTracks()
        {
            _tracks = new int[_content_length];
            for (var i = 0; i < _content_length; i++)
                _tracks[i] = i;
        }

        void ReorderTracks()
        {
            if (!IsTrackAvailable(_track))
                return;
            var idx = ArrayUtility_IndexOf_intarray_int(_tracks, _track);
            var tmp = new int[_tracks.Length];
            Array.Copy(_tracks, idx, tmp, 0, _tracks.Length - idx);
            Array.Copy(_tracks, 0, tmp, _tracks.Length - idx, idx);
            _tracks = tmp;
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
                UpdateTrack();
            }
            ReorderTracks();
            PlayTrack();
        }

        void StopTracks()
        {
            var playing = IsTrackPlaying(_track);
            _controlled = false;
            _wait = false;
            StopTrack(playing);
            ClearTracks();
            UpdateTracks();
        }

        void UpdateTrack()
        {
            if (!_controlled || !IsTrackAvailable(_track))
                return;

            _progressSlider = _content_sliders[_track];
        }

        void PlayTrack()
        {
            if (!_controlled || !IsTrackAvailable(_track))
                return;

            core.TakeOwnership();
            core.PlayURL(_content_modes[_track], _content_urls[_track]);
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
            return _tracks == null || _tracks.Length == 0;
        }

        bool IsTrackAvailable(int track)
        {
            if (IsTrackDoneAll())
                return false;
            return _ArrayUtility_Contains_intarray_int(_tracks, track);
        }

        bool[] GetTrackAvailables()
        {
            var results = new bool[_content_length];
            if (IsTrackDoneAll())
                return results;
            for (var i = 0; i < _tracks.Length; i++)
                results[_tracks[i]] = true;
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
            var results = new bool[_content_length];
            if (IsTrackDoneAll())
                return results;
            if (!core.isPrepared && !core.isPlaying && !core.isError)
                return results;
            var availables = GetTrackAvailables();
            for (var i = 0; i < _content_length; i++)
                results[i] = _controlled && availables[i] && i == _track;
            return results;
        }

        bool IsTrackDone(int track)
        {
            if (IsTrackDoneAll())
                return false;
            return !_ArrayUtility_Contains_intarray_int(_tracks, track);
        }

        bool[] GetTrackDones()
        {
            var results = new bool[_content_length];
            if (IsTrackDoneAll())
                return results;
            for (var i = 0; i < _content_length; i++)
                results[i] = true;
            for (var i = 0; i < _tracks.Length; i++)
                results[_tracks[i]] = false;
            return results;
        }

        void TrackDone()
        {
            if (IsTrackDoneAll())
                return;
            _tracks = _ArrayUtility_Remove_intarray_int(_tracks, _track);
        }

        int GetTrackNext()
        {
            if (IsTrackDoneAll())
                return -1;
            if (defaultShuffle)
                return _tracks[UnityEngine.Random.Range(0, _tracks.Length)];
            if (defaultOrder)
            {
                if (_track < 0)
                    return _tracks[_tracks.Length - 1];
                var idx = ArrayUtility_IndexOf_intarray_int(_tracks, _track);
                return _tracks[idx <= 0 ? _tracks.Length - 1 : idx - 1];
            }
            return _tracks[0];
        }
    }
}
