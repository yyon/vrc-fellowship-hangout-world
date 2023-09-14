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
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoController : VideoCoreEventListener
    {
        [Header("Main")]
        [SerializeField]
        VideoCore core;

        [SerializeField, HideInInspector]
        GameObject _canvas1;
        [SerializeField, HideInInspector]
        GameObject _progress;
        [SerializeField, HideInInspector]
        Slider _progressSlider;
        [SerializeField, HideInInspector]
        GameObject _progressSliderHandle;
        [SerializeField, HideInInspector]
        GameObject _lockOn;
        [SerializeField, HideInInspector]
        Button _lockOnButton;
        [SerializeField, HideInInspector]
        GameObject _lockOff;
        [SerializeField, HideInInspector]
        Button _lockOffButton;
        [SerializeField, HideInInspector]
        GameObject _backward;
        [SerializeField, HideInInspector]
        Button _backwardButton;
        [SerializeField, HideInInspector]
        GameObject _pauseOn;
        [SerializeField, HideInInspector]
        Button _pauseOnButton;
        [SerializeField, HideInInspector]
        GameObject _pauseOff;
        [SerializeField, HideInInspector]
        Button _pauseOffButton;
        [SerializeField, HideInInspector]
        GameObject _stop;
        [SerializeField, HideInInspector]
        Button _stopButton;
        [SerializeField, HideInInspector]
        GameObject _forward;
        [SerializeField, HideInInspector]
        Button _forwardButton;
        [SerializeField, HideInInspector]
        GameObject _message;
        [SerializeField, HideInInspector]
        GameObject _messageText;
        [SerializeField, HideInInspector]
        Text _messageTextText;
        [SerializeField, HideInInspector]
        GameObject _messageTime;
        [SerializeField, HideInInspector]
        Text _messageTimeText;
        [SerializeField, HideInInspector]
        GameObject _muteOn;
        [SerializeField, HideInInspector]
        GameObject _muteOff;
        [SerializeField, HideInInspector]
        GameObject _volume;
        [SerializeField, HideInInspector]
        Slider _volumeSlider;
        [SerializeField, HideInInspector]
        GameObject _reload;
        [SerializeField, HideInInspector]
        GameObject _loopOn;
        [SerializeField, HideInInspector]
        Button _loopOnButton;
        [SerializeField, HideInInspector]
        GameObject _loopOff;
        [SerializeField, HideInInspector]
        Button _loopOffButton;
        [SerializeField, HideInInspector]
        GameObject _optionsOn;
        [SerializeField, HideInInspector]
        GameObject _optionsOff;

        [SerializeField, HideInInspector]
        GameObject _canvas2;
        [SerializeField, HideInInspector]
        Text _masterText;
        [SerializeField, HideInInspector]
        Text _offsetTimeText;
        [SerializeField, HideInInspector]
        Slider _minVolumeSlider;
        [SerializeField, HideInInspector]
        Slider _maxVolumeSlider;
        [SerializeField, HideInInspector]
        GameObject _speedLL;
        [SerializeField, HideInInspector]
        Button _speedLLButton;
        [SerializeField, HideInInspector]
        GameObject _speedL;
        [SerializeField, HideInInspector]
        Button _speedLButton;
        [SerializeField, HideInInspector]
        Text _speedText;
        [SerializeField, HideInInspector]
        GameObject _speedR;
        [SerializeField, HideInInspector]
        Button _speedRButton;
        [SerializeField, HideInInspector]
        GameObject _speedRR;
        [SerializeField, HideInInspector]
        Button _speedRRButton;
        [SerializeField, HideInInspector]
        GameObject _speedClear;
        [SerializeField, HideInInspector]
        Button _speedClearButton;
        [SerializeField, HideInInspector]
        Dropdown _maxResolutionDropdown;

        VRCPlayerApi _localPlayer;
        VRCPlayerApi _masterPlayer;
        bool _progressDrag = false;
        bool _volumeDrag = false;
        //bool _minVolumeDrag = false;
        //bool _maxVolumeDrag = false;
        bool _options = false;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Started `{nameof(VideoController)}`.");

            core.AddListener(this);

            _localPlayer = Networking.LocalPlayer;
        }

        #region EventListener
        VideoControllerEventListener[] _listeners;

        public void AddListener(VideoControllerEventListener listener)
        {
            if (_listeners == null)
                _listeners = new VideoControllerEventListener[0];
            var array = new VideoControllerEventListener[_listeners.Length + 1];
            _listeners.CopyTo(array, 0);
            array[_listeners.Length] = listener;
            _listeners = array;
            core.AddListener(listener);
        }
        #endregion

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            _masterPlayer = FindMasterPlayer();
            if (isOwner)
                RequestSerialization();
            ValidateView();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            _masterPlayer = FindMasterPlayer();
            ValidateView();
        }
        #endregion

        VRCPlayerApi FindMasterPlayer()
        {
            foreach (var x in VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]))
            {
                if (x.isMaster)
                    return x;
            }
            return Networking.LocalPlayer;
        }

        #region VideoEvent
        public override void OnVideoEnd()
        {
            if (core.isOwner)
                core.RequestSerialization();
            ValidateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            switch (core.error)
            {
                case VideoError.Unknown:
                    _messageTextText.text = $"Playback failed due to unknown error.";
                    break;
                case VideoError.InvalidURL:
                    _messageTextText.text = $"Invalid URL.";
                    break;
                case VideoError.AccessDenied:
                    _messageTextText.text = $"Not allowed untrusted url.";
                    break;
                case VideoError.PlayerError:
                    _messageTextText.text = $"Video loading failed.";
                    if (0 < core.errorRetry)
                        _messageTextText.text = $"{_messageTextText.text} Retry {core.errorRetry} more times.";
                    break;
                case VideoError.RateLimited:
                    _messageTextText.text = $"Rate limited.";
                    if (0 < core.errorRetry)
                        _messageTextText.text = $"{_messageTextText.text} Retry {core.errorRetry} more times.";
                    break;
            }
            ValidateView();
        }

        public override void OnVideoLoop()
        {
            if (core.isOwner)
                core.RequestSerialization();
            ValidateView();
        }

        public override void OnVideoReady()
        {
            ValidateView();
        }

        public override void OnVideoStart()
        {
            if (core.isOwner)
                core.RequestSerialization();
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
            _messageTextText.text = $"Loading Now";
            if (core.isOwner)
                core.RequestSerialization();
            ValidateView();
        }

        public override void OnChangeLoop()
        {
            ValidateView();
        }

        public override void OnChangeLive()
        {
            ValidateView();
        }

        public override void OnChangeSpeed()
        {
            ValidateView();
        }

        public override void OnChangeMaximumResolution()
        {
            ValidateView();
        }

        public override void OnChangeMessage()
        {
            ValidateView();
        }

        public override void OnChangeLock()
        {
            ValidateView();
        }

        public override void OnChangeMute()
        {
            ValidateView();
        }

        public override void OnChangeVolume()
        {
            ValidateView();
        }
        #endregion

        public void TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public bool isOwner => Networking.IsOwner(gameObject);

        public void ValidateView()
        {
            var locked = core.locked;
            var master = Networking.IsMaster || (core.isAllowInstanceOwner && _localPlayer.isInstanceOwner);
            var privilege = (locked && master) || !locked;

            var isPrepared = core.isPrepared;
            var isPlaying = core.isPlaying;
            var isError = core.isError;
            var flag1 = isPrepared || isPlaying || isError;
            var isLive = core.isLive;
            var flag2 = !isLive && core.duration != 0f;
            var nisError = !isError;
            var flag3 = isPlaying && (flag2 || isLive) && nisError;
            var nlocked = !locked;
            var flag4 = isPlaying && flag2 && nisError;
            var paused = core.paused;
            var npaused = !paused;
            var flag5 = (isPlaying && paused) && flag2 && nisError;
            var flag6 = (isPlaying && npaused) && flag2 && nisError;
            var flag7 = isPrepared || isError || (isPlaying && !string.IsNullOrEmpty(core.Message));
            var muted = core.muted;
            var nmuted = !muted;
            var flag8 = isPlaying || (isError && core.errorRetry == 0);
            var loop = core.loop;
            var nloop = !loop;
            var noptions = !_options;
            var flag9 = flag1 && _options;
            var flag10 = core.isModeVideo && isPlaying && nisError;
            var flag11 = privilege && flag2 && core.allowSeeking;

            if (_canvas1.activeSelf != flag1)
                _canvas1.SetActive(flag1);
            if (_progress.activeSelf != flag3)
                _progress.SetActive(flag3);
            if (_progressSlider.interactable != flag11)
                _progressSlider.interactable = flag11;
            if (_progressSliderHandle.activeSelf != flag2)
                _progressSliderHandle.SetActive(flag2);
            if (_lockOn.activeSelf != nlocked)
                _lockOn.SetActive(nlocked);
            if (_lockOnButton.interactable != master)
                _lockOnButton.interactable = master;
            if (_lockOff.activeSelf != locked)
                _lockOff.SetActive(locked);
            if (_lockOffButton.interactable != master)
                _lockOffButton.interactable = master;
            if (_backward.activeSelf != flag4)
                _backward.SetActive(flag4);
            if (_backwardButton.interactable != privilege)
                _backwardButton.interactable = privilege;
            if (_pauseOn.activeSelf != flag5)
                _pauseOn.SetActive(flag5);
            if (_pauseOnButton.interactable != privilege)
                _pauseOnButton.interactable = privilege;
            if (_pauseOff.activeSelf != flag6)
                _pauseOff.SetActive(flag6);
            if (_pauseOffButton.interactable != privilege)
                _pauseOffButton.interactable = privilege;
            if (_stop.activeSelf != flag1)
                _stop.SetActive(flag1);
            if (_stopButton.interactable != privilege)
                _stopButton.interactable = privilege;
            if (_forward.activeSelf != flag4)
                _forward.SetActive(flag4);
            if (_forwardButton.interactable != privilege)
                _forwardButton.interactable = privilege;
            if (_messageText.activeSelf != flag7)
                _messageText.SetActive(flag7);
            if (_messageTime.activeSelf != isPlaying)
                _messageTime.SetActive(isPlaying);
            if (_muteOn.activeSelf != nmuted)
                _muteOn.SetActive(nmuted);
            if (_muteOff.activeSelf != muted)
                _muteOff.SetActive(muted);
            _volumeSlider.minValue = core.minVolume;
            _volumeSlider.maxValue = core.maxVolume;
            _volumeSlider.value = core.volume;
            if (_reload.activeSelf != flag8)
                _reload.SetActive(flag8);
            if (_loopOn.activeSelf != nloop)
                _loopOn.SetActive(nloop);
            if (_loopOnButton.interactable != privilege)
                _loopOnButton.interactable = privilege;
            if (_loopOff.activeSelf != loop)
                _loopOff.SetActive(loop);
            if (_loopOffButton.interactable != privilege)
                _loopOffButton.interactable = privilege;
            if (_optionsOn.activeSelf != noptions)
                _optionsOn.SetActive(noptions);
            if (_optionsOff.activeSelf != _options)
                _optionsOff.SetActive(_options);

            if (_canvas2.activeSelf != flag9)
                _canvas2.SetActive(flag9);
            _masterText.text = _masterPlayer == null ? string.Empty : $"{_masterPlayer.displayName}({_masterPlayer.playerId})";
            _offsetTimeText.text = $"{core.offsetTime / 1000f}";
            _minVolumeSlider.value = core.minVolume;
            _maxVolumeSlider.value = core.maxVolume;
            if (_speedLL.activeSelf != flag10)
                _speedLL.SetActive(flag10);
            if (_speedLLButton.interactable != privilege)
                _speedLLButton.interactable = privilege;
            if (_speedL.activeSelf != flag10)
                _speedL.SetActive(flag10);
            if (_speedLButton.interactable != privilege)
                _speedLButton.interactable = privilege;
            _speedText.text = $"×{core.speed:0.00}";
            if (_speedR.activeSelf != flag10)
                _speedR.SetActive(flag10);
            if (_speedRButton.interactable != privilege)
                _speedRButton.interactable = privilege;
            if (_speedRR.activeSelf != flag10)
                _speedRR.SetActive(flag10);
            if (_speedRRButton.interactable != privilege)
                _speedRRButton.interactable = privilege;
            if (_speedClear.activeSelf != flag10)
                _speedClear.SetActive(flag10);
            if (_speedClearButton.interactable != privilege)
                _speedClearButton.interactable = privilege;
            _maxResolutionDropdown.value = Array.IndexOf(
                #if COMPILER_UDONSHARP
                core.MAXIMUM_RESOLUTIONS
                #else
                VideoCore.MAXIMUM_RESOLUTIONS
                #endif
            , core.maximumResolution);
        }

        private void Update()
        {
            if (!core.isPlaying)
                return;

            if (core.isLive)
            {
                _messageTextText.text = core.Message;
                _messageTimeText.text = "Live";
                _progressSlider.value = 1f;
            }
            else
            {
                var duration = core.duration;
                if (core.duration != 0f)
                {
                    var format = core.timeFormat;
                    var time = core.time;
                    _messageTextText.text = core.Message;
                    _messageTimeText.text = $"{TimeSpan.FromSeconds(time).ToString(format)}/{TimeSpan.FromSeconds(duration).ToString(format)}";
                    if (!_progressDrag)
                        _progressSlider.value = Mathf.Clamp(time / duration, 0f, 1f);
                }
            }
        }

        void UpdateTime()
        {
            core.TakeOwnership();
            core.RequestSerialization();
        }

        public void OnProgressBeginDrag()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Progress has started dragging.");
            _progressDrag = true;
        }

        public void OnProgressEndDrag()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Progress drag is finished.");
            _progressDrag = false;
            UpdateTime();
        }

        public void OnProgressChanged()
        {
            if (!_progressDrag)
                return;
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.time = core.duration * _progressSlider.value;
        }

        public void LockOn()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a lock on event.");
            core.TakeOwnership();
            core.locked = true;
            core.RequestSerialization();
        }

        public void LockOff()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a lock off event.");
            core.TakeOwnership();
            core.locked = false;
            core.RequestSerialization();
        }

        public void Backward()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a backward event.");
            core.TakeOwnership();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(-core.seekTimeSeconds);
            core.RequestSerialization();
        }

        public void PauseOn()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a unpause event.");
            core.TakeOwnership();
            core.Play();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(0f);
            core.RequestSerialization();
        }

        public void PauseOff()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a pause event.");
            core.TakeOwnership();
            core.Pause();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(0f);
            core.RequestSerialization();
        }

        public void Stop()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a stop event.");
            core.TakeOwnership();
            core.Stop();
            core.RequestSerialization();
        }

        public void Forward()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a forward event.");
            core.TakeOwnership();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(core.seekTimeSeconds);
            core.RequestSerialization();
        }

        public void MuteOn()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a mute on event.");
            core.muted = true;
        }

        public void MuteOff()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a mute off event.");
            core.muted = false;
        }

        public void OnVolumeBeginDrag()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Volume has started dragging.");
            _volumeDrag = true;
        }

        public void OnVolumeEndDrag()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Volume drag is finished.");
            _volumeDrag = false;
            core.volume = _volumeSlider.value;
        }

        public void OnVolumeChanged()
        {
            if (!_volumeDrag)
                return;
            core.speakerVolume = _volumeSlider.value;
        }

        public void Reload()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a reload event.");
            core.Reload();
        }

        public void LoopOn()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a loop on event.");
            core.TakeOwnership();
            core.loop = true;
            core.RequestSerialization();
            ValidateView();
        }

        public void LoopOff()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Trigger a loop off event.");
            core.TakeOwnership();
            core.loop = false;
            core.RequestSerialization();
            ValidateView();
        }

        public void OptionsOn()
        {
            _options = true;
            ValidateView();
        }

        public void OptionsOff()
        {
            _options = false;
            ValidateView();
        }

        public void OffsetTimeLL()
        {
            core.offsetTime -= 100;
            ValidateView();
        }

        public void OffsetTimeL()
        {
            core.offsetTime -= 10;
            ValidateView();
        }

        public void OffsetTimeR()
        {
            core.offsetTime += 10;
            ValidateView();
        }

        public void OffsetTimeRR()
        {
            core.offsetTime += 100;
            ValidateView();
        }

        public void OffsetTimeClear()
        {
            core.offsetTime = 0;
            ValidateView();
        }

        public void OnMinVolumeChanged()
        {
            if (!_volumeDrag)
                return;
            core.minVolume = _minVolumeSlider.value;
        }

        public void OnMaxVolumeChanged()
        {
            if (!_volumeDrag)
                return;
            core.maxVolume = _maxVolumeSlider.value;
        }

        public void SpeedLL()
        {
            core.TakeOwnership();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(0f);
            core.speed = Mathf.Max(1f, core.speed - 1f);
            core.RequestSerialization();
            ValidateView();
        }

        public void SpeedL()
        {
            core.TakeOwnership();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(0f);
            core.speed = Mathf.Max(1f, core.speed - 0.25f);
            core.RequestSerialization();
            ValidateView();
        }

        public void SpeedR()
        {
            core.TakeOwnership();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(0f);
            core.speed = Mathf.Min(5f, core.speed + 0.25f);
            core.RequestSerialization();
            ValidateView();
        }

        public void SpeedRR()
        {
            core.TakeOwnership();
            core.clockTime = Networking.GetServerTimeInMilliseconds();
            core.Seek(0f);
            core.speed = Mathf.Min(5f, core.speed + 1f);
            core.RequestSerialization();
            ValidateView();
        }

        public void SpeedClear()
        {
            core.speed = 1f;
            ValidateView();
        }

        public void OnMaxResolutionChanged()
        {
            var maximumResolution = int.Parse(_maxResolutionDropdown.captionText.text);
            var index = Array.IndexOf(
                #if COMPILER_UDONSHARP
                core.MAXIMUM_RESOLUTIONS
                #else
                VideoCore.MAXIMUM_RESOLUTIONS
                #endif
            , maximumResolution);
            if (index < 0)
                return;
            if (core.maximumResolution == maximumResolution)
                return;
            core.maximumResolution = maximumResolution;
            core.Reload();
        }
    }
}
