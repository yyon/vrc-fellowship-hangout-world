using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Video.Components;

namespace HoshinoLabs.IwaSync3
{
    public static class VRCUnityVideoPlayerExtensions
    {
        public static void SetLoop(this VRCUnityVideoPlayer self, bool loop)
        {
            var field = self.GetType().GetField("loop", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, loop);
        }

        public static void SetTargetAudioSources(this VRCUnityVideoPlayer self, AudioSource[] targetAudioSources)
        {
            var field = self.GetType().GetField("targetAudioSources", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, targetAudioSources);
        }

        public static void SetMaximumResolution(this VRCUnityVideoPlayer self, int maximumResolution)
        {
            var field = self.GetType().GetField("maximumResolution", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(self, maximumResolution);
        }
    }
}
