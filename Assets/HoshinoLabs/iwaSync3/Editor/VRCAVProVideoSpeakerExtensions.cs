using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;
using static VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker;

namespace HoshinoLabs.IwaSync3
{
    public static class VRCAVProVideoSpeakerExtensions
    {
        public static void SetVideoPlayer(this VRCAVProVideoSpeaker self, VRCAVProVideoPlayer videoPlayer)
        {
            var field = self.GetType().GetField("videoPlayer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, videoPlayer);
        }

        public static void SetMode(this VRCAVProVideoSpeaker self, ChannelMode mode)
        {
            var field = self.GetType().GetField("mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, mode);
        }
    }
}
