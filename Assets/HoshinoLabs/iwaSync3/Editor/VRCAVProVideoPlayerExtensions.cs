using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;

namespace HoshinoLabs.IwaSync3
{
    public static class VRCAVProVideoPlayerExtensions
    {
        public static void SetLoop(this VRCAVProVideoPlayer self, bool loop)
        {
            var field = self.GetType().GetField("loop", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, loop);
        }

        public static void SetMaximumResolution(this VRCAVProVideoPlayer self, int maximumResolution)
        {
            var field = self.GetType().GetField("maximumResolution", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, maximumResolution);
        }

        public static void SetUseLowLatency(this VRCAVProVideoPlayer self, bool useLowLatency)
        {
            var field = self.GetType().GetField("useLowLatency", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, useLowLatency);
        }
    }
}
