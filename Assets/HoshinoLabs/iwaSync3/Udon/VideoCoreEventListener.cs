using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace HoshinoLabs.IwaSync3.Udon
{
    public abstract class VideoCoreEventListener : UdonSharpBehaviour
    {
        #region VideoEvent
        public override void OnVideoEnd() { }
        public override void OnVideoError(VideoError videoError) { }
        public override void OnVideoLoop() { }
        public override void OnVideoReady() { }
        public override void OnVideoStart() { }
        #endregion

        #region VideoCoreEvent
        public virtual void OnPlayerPlay() { }
        public virtual void OnPlayerPause() { }
        public virtual void OnPlayerStop() { }

        public virtual void OnChangeURL() { }
        public virtual void OnChangeLoop() { }
        public virtual void OnChangeLive() { }
        public virtual void OnChangeSpeed() { }
        public virtual void OnChangeMaximumResolution() { }
        public virtual void OnChangeMessage() { }
        public virtual void OnChangeLock() { }
        public virtual void OnChangeMute() { }
        public virtual void OnChangeVolume() { }
        #endregion
    }
}
