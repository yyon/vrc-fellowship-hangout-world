using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Video.Components.AVPro;
using static VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(Speaker))]
    [CanEditMultipleObjects]
    internal class SpeakerEditor : IwaSync3EditorBase
    {
        Speaker _target;

        SerializedProperty _iwaSync3Property;
        SerializedProperty _maskProperty;
        SerializedProperty _primaryProperty;

        SerializedProperty _maxDistanceProperty;

        SerializedProperty _spatializeProperty;

        SerializedProperty _modeProperty;

        protected override void FindProperties()
        {
            base.FindProperties();

            _target = target as Speaker;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");
            _maskProperty = serializedObject.FindProperty("mask");
            _primaryProperty = serializedObject.FindProperty("primary");

            _maxDistanceProperty = serializedObject.FindProperty("maxDistance");

            _spatializeProperty = serializedObject.FindProperty("spatialize");

            _modeProperty = serializedObject.FindProperty("mode");
        }

        public override void OnInspectorGUI()
        {
            FindProperties();

            base.OnInspectorGUI();

            serializedObject.Update();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Main", _italicStyle);
                var iwaSync3 = GetMainIwaSync3(null);
                if (iwaSync3)
                    EditorGUILayout.LabelField(_iwaSync3Property.displayName, "Automatically set by Script");
                else
                    EditorGUILayout.PropertyField(_iwaSync3Property);
                var mask = (TrackModeMask)Enum.ToObject(typeof(TrackModeMask), _maskProperty.intValue);
                mask = (TrackModeMask)EditorGUILayout.EnumFlagsField(_maskProperty.displayName, mask);
                _maskProperty.intValue = (int)mask;
                EditorGUILayout.PropertyField(_primaryProperty);
            }

            EditorGUILayout.Space();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Audio", _italicStyle);
                EditorGUILayout.PropertyField(_maxDistanceProperty);
            }

            EditorGUILayout.Space();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Spatialize", _italicStyle);
                EditorGUILayout.PropertyField(_spatializeProperty);
            }

            EditorGUILayout.Space();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Options", _italicStyle);
                EditorGUILayout.PropertyField(_modeProperty);
                EditorGUILayout.HelpBox("この設定はLiveで再生している時だけ機能します", MessageType.Warning);
            }

            if (serializedObject.ApplyModifiedProperties())
                ApplyModifiedProperties();
        }

        internal override void ApplyModifiedProperties()
        {
            FindProperties();

            var iwaSync3 = GetMainIwaSync3(_iwaSync3Property);
            if (iwaSync3 == null)
                return;
            var core = iwaSync3.GetComponentInChildren<Udon.VideoCore>(true);

            var audioSource = _target.GetComponentInChildren<AudioSource>(true);
            audioSource.mute = new SerializedObject(iwaSync3).FindProperty("defaultMute").boolValue;
            audioSource.volume = new SerializedObject(iwaSync3).FindProperty("defaultVolume").floatValue;
            audioSource.maxDistance = _maxDistanceProperty.floatValue;

            var spatial = _target.GetComponentInChildren<VRCSpatialAudioSource>(true);
            spatial.EnableSpatialization = _spatializeProperty.boolValue;

            var mask = (TrackModeMask)Enum.ToObject(typeof(TrackModeMask), _maskProperty.intValue);
            if((mask & TrackModeMask.Live) != 0)
            {
                var avProVideoSpeaker = audioSource.gameObject.GetComponent<VRCAVProVideoSpeaker>();
                if(avProVideoSpeaker == null)
                    avProVideoSpeaker = audioSource.gameObject.AddComponent<VRCAVProVideoSpeaker>();
                avProVideoSpeaker.SetVideoPlayer(core.GetComponentInChildren<VRCAVProVideoPlayer>(true));
                avProVideoSpeaker.SetMode((ChannelMode)Enum.ToObject(typeof(ChannelMode), _modeProperty.enumValueIndex));
            }
        }
    }
}
