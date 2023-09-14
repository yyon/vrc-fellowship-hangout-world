using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace HoshinoLabs.IwaSync3.Udon
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DesktopBar : VideoControllerEventListener
    {
        [Header("Main")]
        [SerializeField]
        VideoCore core;
        [SerializeField]
        VideoController controller;

        [Header("Options")]
        [SerializeField]
        bool desktopOnly = true;

        [SerializeField, HideInInspector]
        GameObject _canvas1;
        [SerializeField, HideInInspector]
        GameObject _lockOn;
        [SerializeField, HideInInspector]
        GameObject _lockOff;
        [SerializeField, HideInInspector]
        GameObject _video;
        [SerializeField, HideInInspector]
        GameObject _live;
        [SerializeField, HideInInspector]
        GameObject _muteOn;
        [SerializeField, HideInInspector]
        GameObject _muteOff;
        [SerializeField, HideInInspector]
        GameObject _volume;
        [SerializeField, HideInInspector]
        Text _volumeText;
        [SerializeField, HideInInspector]
        GameObject _pauseOn;
        [SerializeField, HideInInspector]
        GameObject _pauseOff;
        [SerializeField, HideInInspector]
        GameObject _loopOn;
        [SerializeField, HideInInspector]
        GameObject _loopOff;
        [SerializeField, HideInInspector]
        GameObject _address;

        [SerializeField, HideInInspector]
        GameObject _canvas2;
        [SerializeField, HideInInspector]
        RectTransform _canvas2Rect;
        [SerializeField, HideInInspector]
        VRCUrlInputField _addressInput;

        [SerializeField, HideInInspector]
        GameObject _canvasPIP;
        [SerializeField, HideInInspector]
        RawImage _image1;
        [SerializeField, HideInInspector]
        RectTransform _image1Rect;
        [SerializeField, HideInInspector]
        AspectRatioFitter _image1Aspect;

        [SerializeField, HideInInspector]
        GameObject _canvasFullScreen;
        [SerializeField, HideInInspector]
        RawImage _image2;
        [SerializeField, HideInInspector]
        RectTransform _image2Rect;
        [SerializeField, HideInInspector]
        AspectRatioFitter _image2Aspect;

        uint _mode;

        private void Start()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] Started `{nameof(DesktopBar)}`.");

            if(desktopOnly && Networking.LocalPlayer != null && Networking.LocalPlayer.IsUserInVR())
            {
                Destroy(gameObject);
                return;
            }

            core.AddListener(this);

            ModeVideo();
        }

        #region RoomEvent
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            ValidateView();
        }
        #endregion

        #region VideoEvent
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

        public override void OnChangeLoop()
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

        public bool isModeVideo => _mode ==
        #if COMPILER_UDONSHARP
        core.MODE_VIDEO
        #else
        VideoCore.MODE_VIDEO
        #endif
        ;
        public bool isModeStream => _mode ==
        #if COMPILER_UDONSHARP
        core.MODE_STREAM
        #else
        VideoCore.MODE_STREAM
        #endif
        ;

        public void ValidateView()
        {
            var locked = core.locked;

            var nlocked = !locked;
            var muted = core.muted;
            var nmuted = !muted;
            var paused = core.paused;
            var npaused = !paused;
            var loop = core.loop;
            var nloop = !loop;

            if (_lockOn.activeSelf != nlocked)
                _lockOn.SetActive(nlocked);
            if (_lockOff.activeSelf != locked)
                _lockOff.SetActive(locked);
            if (_video.activeSelf != isModeVideo)
                _video.SetActive(isModeVideo);
            if (_live.activeSelf != isModeStream)
                _live.SetActive(isModeStream);
            if (_muteOn.activeSelf != nmuted)
                _muteOn.SetActive(nmuted);
            if (_muteOff.activeSelf != muted)
                _muteOff.SetActive(muted);
            _volumeText.text = $"Volume ({Mathf.RoundToInt(core.volume * 100f)}%)";
            if (_pauseOn.activeSelf != paused)
                _pauseOn.SetActive(paused);
            if (_pauseOff.activeSelf != npaused)
                _pauseOff.SetActive(npaused);
            if (_loopOn.activeSelf != nloop)
                _loopOn.SetActive(nloop);
            if (_loopOff.activeSelf != loop)
                _loopOff.SetActive(loop);
            _addressInput.SetUrl(core.url);

            var texture = core.isPlaying ? core.texture : null;
            var ratio = 1f;
            var mode = AspectRatioFitter.AspectMode.None;
            if (core.isPlaying)
            {
                if (texture == null)
                    SendCustomEventDelayedFrames(nameof(ValidateView), 0);
                else
                {
                    ratio = (float)texture.width / (float)texture.height;
                    var screen = _canvas2Rect.sizeDelta;
                    if (ratio < (screen.x / screen.y))
                        mode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                    else
                        mode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                }
            }

            _image1.enabled = core.isPlaying;
            _image1.texture = texture;
            _image1.material.SetInt("_IsAVProVideo", core.isModeVideo ? 0 : 1);
            _image1Rect.localScale = new Vector3(1f, core.isModeVideo ? 1f : -1f, 1f);
            _image1Aspect.aspectMode = mode;
            _image1Aspect.aspectRatio = ratio;

            _image2.enabled = core.isPlaying;
            _image2.texture = texture;
            _image2.material.SetInt("_IsAVProVideo", core.isModeVideo ? 0 : 1);
            _image2Rect.localScale = new Vector3(1f, core.isModeVideo ? 1f : -1f, 1f);
            _image2Aspect.aspectMode = mode;
            _image2Aspect.aspectRatio = ratio;
        }

        private void Update()
        {
            if(/*Input.GetKeyDown(KeyCode.LeftControl) || */Input.GetKeyDown(KeyCode.RightControl))
            {
                _canvas1.SetActive(true);

                _canvas2.SetActive(true);
                _addressInput.ActivateInputField();
                _addressInput.SetUrl(core.url);
            }
            if (/*Input.GetKey(KeyCode.LeftControl) || */Input.GetKey(KeyCode.RightControl))
            {
                var locked = core.locked;
                var master = Networking.IsMaster || (core.isAllowInstanceOwner && Networking.LocalPlayer.isInstanceOwner);
                var privilege = (locked && master) || !locked;

                if (master)
                {
                    if (Input.GetKeyDown(KeyCode.U))
                    {
                        if (core.locked)
                            controller.LockOff();
                        else
                            controller.LockOn();
                    }
                }

                if (privilege)
                {
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        if (isModeVideo)
                            ModeLive();
                        else
                            ModeVideo();
                        ValidateView();
                    }
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        if(!core.isPrepared)
                            OnURLChanged();
                    }
                }

                if (Input.GetKeyDown(KeyCode.M))
                {
                    core.muted = !core.muted;
                    ValidateView();
                }
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    core.volume = core.volume + 0.05f;
                    ValidateView();
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    core.volume = core.volume - 0.05f;
                    ValidateView();
                }

                if (privilege)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        if(core.isPlaying && !core.isLive && !core.isError)
                        {
                            if (core.paused)
                                controller.PauseOn();
                            else
                                controller.PauseOff();
                            ValidateView();
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    if(core.isPlaying || (core.isError && core.errorRetry == 0))
                        controller.Reload();
                }

                if (privilege)
                {
                    if (Input.GetKeyDown(KeyCode.L))
                    {
                        if (core.loop)
                            controller.LoopOff();
                        else
                            controller.LoopOn();
                        ValidateView();
                    }
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    _canvasPIP.SetActive(!_canvasPIP.activeSelf);
                    _canvasFullScreen.SetActive(false);
                }
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    _canvasPIP.SetActive(false);
                    _canvasFullScreen.SetActive(!_canvasFullScreen.activeSelf);
                }

                if (privilege)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if(core.isPlaying && !core.isLive && !core.isError)
                        {
                            controller.Backward();
                            ValidateView();
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        if (core.isPlaying && !core.isLive && !core.isError)
                        {
                            controller.Forward();
                            ValidateView();
                        }
                    }
                }
            }
            if (/*Input.GetKeyUp(KeyCode.LeftControl) || */Input.GetKeyUp(KeyCode.RightControl))
            {
                _canvas2.SetActive(false);
                _addressInput.DeactivateInputField();

                _canvas1.SetActive(false);
            }
        }

        void ModeVideo()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] The mode has changed to `MODE_VIDEO`.");
            _mode =
                #if COMPILER_UDONSHARP
                core.MODE_VIDEO
                #else
                VideoCore.MODE_VIDEO
                #endif
            ;
        }

        void ModeLive()
        {
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] The mode has changed to `MODE_STREAM`.");
            _mode =
                #if COMPILER_UDONSHARP
                core.MODE_STREAM
                #else
                VideoCore.MODE_STREAM
                #endif
            ;
        }

        void OnURLChanged()
        {
            var url = _addressInput.GetUrl();
            if (!core.IsValidURL(url.Get()))
                return;
            Debug.Log($"[<color=#47F1FF>{IwaSync3.APP_NAME}</color>] The url has changed to `{_addressInput.GetUrl().Get()}`.");

            core.TakeOwnership();
            if (core.IsRTSP(url.Get()))
                ModeLive();
            core.PlayURL(_mode, url);
            core.RequestSerialization();
            ValidateView();
        }
    }
}
