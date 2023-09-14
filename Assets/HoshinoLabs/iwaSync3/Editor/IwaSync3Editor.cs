using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using static VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(IwaSync3))]
    internal class IwaSync3Editor : IwaSync3EditorBase
    {
        IwaSync3 _target;

        SerializedProperty _iwaSync3Property;

        SerializedProperty _defaultModeProperty;
        SerializedProperty _defaultUrlProperty;
        SerializedProperty _defaultDelayProperty;
        SerializedProperty _allowSeekingProperty;
        SerializedProperty _defaultLoopProperty;
        SerializedProperty _seekTimeSecondsProperty;
        SerializedProperty _timeFormatProperty;

        SerializedProperty _syncFrequencyProperty;
        SerializedProperty _syncThresholdProperty;

        SerializedProperty _maxErrorRetryProperty;
        SerializedProperty _timeoutUnknownErrorProperty;
        SerializedProperty _timeoutPlayerErrorProperty;
        SerializedProperty _timeoutRateLimitedProperty;
        SerializedProperty _allowErrorReduceMaxResolutionProperty;

        SerializedProperty _defaultLockProperty;
        SerializedProperty _allowInstanceOwnerProperty;

        SerializedProperty _maximumResolutionProperty;

        SerializedProperty _defaultMuteProperty;
        SerializedProperty _defaultMinVolumeProperty;
        SerializedProperty _defaultMaxVolumeProperty;
        SerializedProperty _defaultVolumeProperty;

        SerializedProperty _useLowLatencyProperty;

        protected override void FindProperties()
        {
            base.FindProperties();

            _target = target as IwaSync3;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");

            _defaultModeProperty = serializedObject.FindProperty("defaultMode");
            _defaultUrlProperty = serializedObject.FindProperty("defaultUrl");
            _defaultDelayProperty = serializedObject.FindProperty("defaultDelay");
            _allowSeekingProperty = serializedObject.FindProperty("allowSeeking");
            _defaultLoopProperty = serializedObject.FindProperty("defaultLoop");
            _seekTimeSecondsProperty = serializedObject.FindProperty("seekTimeSeconds");
            _timeFormatProperty = serializedObject.FindProperty("timeFormat");

            _syncFrequencyProperty = serializedObject.FindProperty("syncFrequency");
            _syncThresholdProperty = serializedObject.FindProperty("syncThreshold");

            _maxErrorRetryProperty = serializedObject.FindProperty("maxErrorRetry");
            _timeoutUnknownErrorProperty = serializedObject.FindProperty("timeoutUnknownError");
            _timeoutPlayerErrorProperty = serializedObject.FindProperty("timeoutPlayerError");
            _timeoutRateLimitedProperty = serializedObject.FindProperty("timeoutRateLimited");
            _allowErrorReduceMaxResolutionProperty = serializedObject.FindProperty("allowErrorReduceMaxResolution");

            _defaultLockProperty = serializedObject.FindProperty("defaultLock");
            _allowInstanceOwnerProperty = serializedObject.FindProperty("allowInstanceOwner");

            _maximumResolutionProperty = serializedObject.FindProperty("maximumResolution");

            _defaultMuteProperty = serializedObject.FindProperty("defaultMute");
            _defaultMinVolumeProperty = serializedObject.FindProperty("defaultMinVolume");
            _defaultMaxVolumeProperty = serializedObject.FindProperty("defaultMaxVolume");
            _defaultVolumeProperty = serializedObject.FindProperty("defaultVolume");

            _useLowLatencyProperty = serializedObject.FindProperty("useLowLatency");
        }

        public override void OnInspectorGUI()
        {
            FindProperties();

            base.OnInspectorGUI();

            serializedObject.Update();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Main", _italicStyle);
                EditorGUILayout.PropertyField(_iwaSync3Property);
            }

            if (_iwaSync3Property.objectReferenceValue == null)
            {
                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Control", _italicStyle);
                    EditorGUILayout.PropertyField(_defaultModeProperty);
                    EditorGUILayout.PropertyField(_defaultUrlProperty);
                    EditorGUILayout.PropertyField(_defaultDelayProperty);
                    EditorGUILayout.PropertyField(_allowSeekingProperty);
                    EditorGUILayout.PropertyField(_defaultLoopProperty);
                    EditorGUILayout.PropertyField(_seekTimeSecondsProperty);
                    EditorGUILayout.PropertyField(_timeFormatProperty);
                }

                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Sync", _italicStyle);
                    EditorGUILayout.PropertyField(_syncFrequencyProperty);
                    EditorGUILayout.PropertyField(_syncThresholdProperty);
                }

                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Error Handling", _italicStyle);
                    EditorGUILayout.PropertyField(_maxErrorRetryProperty);
                    EditorGUILayout.PropertyField(_timeoutUnknownErrorProperty);
                    EditorGUILayout.PropertyField(_timeoutPlayerErrorProperty);
                    EditorGUILayout.PropertyField(_timeoutRateLimitedProperty);
                    EditorGUILayout.PropertyField(_allowErrorReduceMaxResolutionProperty);
                }

                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Lock", _italicStyle);
                    EditorGUILayout.PropertyField(_defaultLockProperty);
                    EditorGUILayout.PropertyField(_allowInstanceOwnerProperty);
                }

                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Video", _italicStyle);
                    var displayedOptions = Udon.VideoCore.MAXIMUM_RESOLUTIONS;
                    var selectedIndex = Array.IndexOf(displayedOptions, _maximumResolutionProperty.intValue);
                    selectedIndex = EditorGUILayout.Popup(_maximumResolutionProperty.displayName, selectedIndex, displayedOptions.Select(x => $"{x}").ToArray());
                    if (0 <= selectedIndex)
                        _maximumResolutionProperty.intValue = displayedOptions[selectedIndex];
                }

                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Audio", _italicStyle);
                    EditorGUILayout.PropertyField(_defaultMuteProperty);
                    EditorGUILayout.LabelField(_defaultMinVolumeProperty.displayName, $"{_defaultMinVolumeProperty.floatValue:0.00}");
                    EditorGUILayout.LabelField(_defaultMaxVolumeProperty.displayName, $"{_defaultMaxVolumeProperty.floatValue:0.00}");
                    var defaultMinVolume = _defaultMinVolumeProperty.floatValue;
                    var defaultMaxVolume = _defaultMaxVolumeProperty.floatValue;
                    EditorGUILayout.MinMaxSlider("Default Min Max Volume", ref defaultMinVolume, ref defaultMaxVolume, 0f, 1f);
                    _defaultMinVolumeProperty.floatValue = defaultMinVolume;
                    _defaultMaxVolumeProperty.floatValue = defaultMaxVolume;
                    EditorGUILayout.PropertyField(_defaultVolumeProperty);
                }

                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Extra", _italicStyle);
                    EditorGUILayout.PropertyField(_useLowLatencyProperty);
                }
            }

            if (serializedObject.ApplyModifiedProperties())
                ApplyModifiedProperties();
        }

        internal override void ApplyModifiedProperties()
        {
            FindProperties();

            var core = _target.GetComponentInChildren<Udon.VideoCore>(true);
            if (core != null)
            {
                core.SetPublicVariable("speaker", GetSpeakers((TrackModeMask)(-1)));

                core.SetPublicVariable("defaultMode", ((TrackMode)_defaultModeProperty.intValue).ToVideoCoreMode());
                core.SetPublicVariable("defaultUrl", new VRCUrl(_defaultUrlProperty.stringValue));
                core.SetPublicVariable("defaultDelay", _defaultDelayProperty.floatValue);
                core.SetPublicVariable("defaultLoop", _defaultLoopProperty.boolValue);
                core.SetPublicVariable("syncFrequency", _syncFrequencyProperty.floatValue);
                core.SetPublicVariable("syncThreshold", _syncThresholdProperty.floatValue);
                core.SetPublicVariable("maxErrorRetry", _maxErrorRetryProperty.intValue);
                core.SetPublicVariable("timeoutUnknownError", _timeoutUnknownErrorProperty.floatValue);
                core.SetPublicVariable("timeoutPlayerError", _timeoutPlayerErrorProperty.floatValue);
                core.SetPublicVariable("timeoutRateLimited", _timeoutRateLimitedProperty.floatValue);
                core.SetPublicVariable("defaultMaximumResolution", _maximumResolutionProperty.intValue);
                core.SetPublicVariable("allowErrorReduceMaxResolution", _allowErrorReduceMaxResolutionProperty.boolValue);
                core.SetPublicVariable("defaultLock", _defaultLockProperty.boolValue);
                core.SetPublicVariable("allowSeeking", _allowSeekingProperty.boolValue);
                core.SetPublicVariable("seekTimeSeconds", _seekTimeSecondsProperty.floatValue);
                core.SetPublicVariable("timeFormat", _timeFormatProperty.stringValue);
                core.SetPublicVariable("allowInstanceOwner", _allowInstanceOwnerProperty.boolValue);
                core.SetPublicVariable("defaultMute", _defaultMuteProperty.boolValue);
                core.SetPublicVariable("defaultMinVolume", _defaultMinVolumeProperty.floatValue);
                core.SetPublicVariable("defaultMaxVolume", _defaultMaxVolumeProperty.floatValue);
                core.SetPublicVariable("defaultVolume", _defaultVolumeProperty.floatValue);

                var unityVideoPlayer = _target.GetComponentInChildren<VRCUnityVideoPlayer>(true);
                unityVideoPlayer.SetLoop(_defaultLoopProperty.boolValue);
                unityVideoPlayer.SetTargetAudioSources(GetSpeakers(TrackModeMask.Video));
                unityVideoPlayer.SetMaximumResolution(_maximumResolutionProperty.intValue);

                var avProVideoPlayer = _target.GetComponentInChildren<VRCAVProVideoPlayer>(true);
                avProVideoPlayer.SetLoop(_defaultLoopProperty.boolValue);
                avProVideoPlayer.SetMaximumResolution(_maximumResolutionProperty.intValue);
                avProVideoPlayer.SetUseLowLatency(_useLowLatencyProperty.boolValue);

                core.SetPublicVariable("_unityVideoPlayer", unityVideoPlayer);
                var unityVideoRenderer = core.transform.Find("UnityVideo").GetComponent<Renderer>();
                core.SetPublicVariable("_unityVideoRenderer", unityVideoRenderer);
                core.SetPublicVariable("_avProVideoPlayer", avProVideoPlayer);
                var avProVideoRenderer = core.transform.Find("AVProVideo").GetComponent<Renderer>();
                core.SetPublicVariable("_avProVideoRenderer", avProVideoRenderer);
                var animator = core.GetComponent<Animator>();
                core.SetPublicVariable("_animator", animator);
            }

            if (_iwaSync3Property.objectReferenceValue != null)
            {
                var iwaSync3 = GetMainIwaSync3(_iwaSync3Property);
                core = iwaSync3.GetComponentInChildren<Udon.VideoCore>(true);
            }

            var controller = _target.GetComponentInChildren<Udon.VideoController>(true);
            if (controller != null)
            {
                if (core != null)
                    controller.SetPublicVariable("core", core);

                var canvas1 = controller.transform.Find("Canvas").gameObject;
                controller.SetPublicVariable("_canvas1", canvas1);
                var progress = controller.transform.Find("Canvas/Panel/Progress").gameObject;
                controller.SetPublicVariable("_progress", progress);
                var progressSlider = progress.GetComponent<Slider>();
                controller.SetPublicVariable("_progressSlider", progress.GetComponent<Slider>());
                var progressSliderHandle = progressSlider.handleRect.gameObject;
                controller.SetPublicVariable("_progressSliderHandle", progressSliderHandle);
                var lockOn = controller.transform.Find("Canvas/Panel/Lock/On").gameObject;
                controller.SetPublicVariable("_lockOn", lockOn);
                controller.SetPublicVariable("_lockOnButton", lockOn.transform.Find("Button").GetComponent<Button>());
                var lockOff = controller.transform.Find("Canvas/Panel/Lock/Off").gameObject;
                controller.SetPublicVariable("_lockOff", lockOff);
                controller.SetPublicVariable("_lockOffButton", lockOff.transform.Find("Button").GetComponent<Button>());
                var backward = controller.transform.Find("Canvas/Panel/Backward").gameObject;
                controller.SetPublicVariable("_backward", backward);
                controller.SetPublicVariable("_backwardButton", backward.transform.Find("Button").GetComponent<Button>());
                var pauseOn = controller.transform.Find("Canvas/Panel/Pause/On").gameObject;
                controller.SetPublicVariable("_pauseOn", pauseOn);
                controller.SetPublicVariable("_pauseOnButton", pauseOn.transform.Find("Button").GetComponent<Button>());
                var pauseOff = controller.transform.Find("Canvas/Panel/Pause/Off").gameObject;
                controller.SetPublicVariable("_pauseOff", pauseOff);
                controller.SetPublicVariable("_pauseOffButton", pauseOff.transform.Find("Button").GetComponent<Button>());
                var stop = controller.transform.Find("Canvas/Panel/Stop").gameObject;
                controller.SetPublicVariable("_stop", stop);
                controller.SetPublicVariable("_stopButton", stop.transform.Find("Button").GetComponent<Button>());
                var forward = controller.transform.Find("Canvas/Panel/Forward").gameObject;
                controller.SetPublicVariable("_forward", forward);
                controller.SetPublicVariable("_forwardButton", forward.transform.Find("Button").GetComponent<Button>());
                var message = controller.transform.Find("Canvas/Panel/Message").gameObject;
                controller.SetPublicVariable("_message", message);
                var messageText = message.transform.Find("Text").gameObject;
                controller.SetPublicVariable("_messageText", messageText);
                controller.SetPublicVariable("_messageTextText", messageText.GetComponent<Text>());
                var messageTime = message.transform.Find("Time").gameObject;
                controller.SetPublicVariable("_messageTime", messageTime);
                controller.SetPublicVariable("_messageTimeText", messageTime.GetComponent<Text>());
                var muteOn = controller.transform.Find("Canvas/Panel/Mute/On").gameObject;
                controller.SetPublicVariable("_muteOn", muteOn);
                var muteOff = controller.transform.Find("Canvas/Panel/Mute/Off").gameObject;
                controller.SetPublicVariable("_muteOff", muteOff);
                var volume = controller.transform.Find("Canvas/Panel/Volume").gameObject;
                controller.SetPublicVariable("_volume", volume);
                controller.SetPublicVariable("_volumeSlider", volume.GetComponent<Slider>());
                var reload = controller.transform.Find("Canvas/Panel/Reload").gameObject;
                controller.SetPublicVariable("_reload", reload);
                var loopOn = controller.transform.Find("Canvas/Panel/Loop/On").gameObject;
                controller.SetPublicVariable("_loopOn", loopOn);
                controller.SetPublicVariable("_loopOnButton", loopOn.transform.Find("Button").GetComponent<Button>());
                var loopOff = controller.transform.Find("Canvas/Panel/Loop/Off").gameObject;
                controller.SetPublicVariable("_loopOff", loopOff);
                controller.SetPublicVariable("_loopOffButton", loopOff.transform.Find("Button").GetComponent<Button>());
                var optionsOn = controller.transform.Find("Canvas/Panel/Options/On").gameObject;
                controller.SetPublicVariable("_optionsOn", optionsOn);
                var optionsOff = controller.transform.Find("Canvas/Panel/Options/Off").gameObject;
                controller.SetPublicVariable("_optionsOff", optionsOff);

                var canvas2 = controller.transform.Find("Canvas (1)").gameObject;
                controller.SetPublicVariable("_canvas2", canvas2);
                controller.SetPublicVariable("_masterText", controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/Master").GetComponent<Text>());
                controller.SetPublicVariable("_offsetTimeText", controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/OffsetTime/Time/Text").GetComponent<Text>());
                controller.SetPublicVariable("_minVolumeSlider", controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/MinVolume").GetComponent<Slider>());
                controller.SetPublicVariable("_maxVolumeSlider", controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/MaxVolume").GetComponent<Slider>());
                var speedLL = controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/Speed/LL/Button").gameObject;
                controller.SetPublicVariable("_speedLL", speedLL);
                controller.SetPublicVariable("_speedLLButton", speedLL.GetComponent<Button>());
                var speedL = controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/Speed/L/Button").gameObject;
                controller.SetPublicVariable("_speedL", speedL);
                controller.SetPublicVariable("_speedLButton", speedL.GetComponent<Button>());
                controller.SetPublicVariable("_speedText", controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/Speed/Speed/Text").GetComponent<Text>());
                var speedR = controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/Speed/R/Button").gameObject;
                controller.SetPublicVariable("_speedR", speedR);
                controller.SetPublicVariable("_speedRButton", speedR.GetComponent<Button>());
                var speedRR = controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/Speed/RR/Button").gameObject;
                controller.SetPublicVariable("_speedRR", speedRR);
                controller.SetPublicVariable("_speedRRButton", speedRR.GetComponent<Button>());
                var speedClear = controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/Speed/Clear/Button").gameObject;
                controller.SetPublicVariable("_speedClear", speedClear);
                controller.SetPublicVariable("_speedClearButton", speedClear.GetComponent<Button>());
                controller.SetPublicVariable("_maxResolutionDropdown", controller.transform.Find("Canvas (1)/Panel/Layout/Layout/Value/Layout/MaxResolution/Dropdown").GetComponent<Dropdown>());
            }

            var iwasync = _target.GetComponentInChildren<Udon.IwaSync3>(true);
            if (iwasync != null)
            {
                if (core != null)
                    iwasync.SetPublicVariable("core", core);
                if (controller != null)
                    iwasync.SetPublicVariable("controller", controller);

                var canvas1 = iwasync.transform.Find("Canvas").gameObject;
                iwasync.SetPublicVariable("_canvas1", canvas1);
                var lockOn = iwasync.transform.Find("Canvas/Panel/Lock/On").gameObject;
                iwasync.SetPublicVariable("_lockOn", lockOn);
                iwasync.SetPublicVariable("_lockOnButton", lockOn.transform.Find("Button").GetComponent<Button>());
                var lockOff = iwasync.transform.Find("Canvas/Panel/Lock/Off").gameObject;
                iwasync.SetPublicVariable("_lockOff", lockOff);
                iwasync.SetPublicVariable("_lockOffButton", lockOff.transform.Find("Button").GetComponent<Button>());
                iwasync.SetPublicVariable("_videoButton", iwasync.transform.Find("Canvas/Panel/Video/Button").GetComponent<Button>());
                iwasync.SetPublicVariable("_liveButton", iwasync.transform.Find("Canvas/Panel/Live/Button").GetComponent<Button>());

                var canvas2 = iwasync.transform.Find("Canvas (1)").gameObject;
                iwasync.SetPublicVariable("_canvas2", canvas2);
                var address = iwasync.transform.Find("Canvas (1)/Panel/Address").gameObject;
                iwasync.SetPublicVariable("_address", address);
                var addressInput = address.GetComponent<VRCUrlInputField>();
                addressInput.textComponent = address.transform.Find("Text").GetComponent<Text>();// SDK bug countermeasures
                iwasync.SetPublicVariable("_addressInput", addressInput);
                var message = iwasync.transform.Find("Canvas (1)/Panel/Message").gameObject;
                iwasync.SetPublicVariable("_message", message);
                iwasync.SetPublicVariable("_messageText", message.GetComponent<Text>());
                var close = iwasync.transform.Find("Canvas (1)/Panel/Close").gameObject;
                iwasync.SetPublicVariable("_close", close);
                iwasync.SetPublicVariable("_closeButton", close.transform.Find("Button").GetComponent<Button>());
            }
        }

        AudioSource[] GetSpeakers(TrackModeMask mask)
        {
            return FindObjectsOfType<Speaker>(true)
                .Where(x => GetMainIwaSync3(x) == _target)
                .OrderBy(x => (ChannelMode)Enum.ToObject(typeof(ChannelMode), new SerializedObject(x).FindProperty("mode").enumValueIndex))
                .OrderByDescending(x => new SerializedObject(x).FindProperty("primary").boolValue)
                .Where(x => ((TrackModeMask)Enum.ToObject(typeof(TrackModeMask), new SerializedObject(x).FindProperty("mask").intValue) & TrackModeMask.Video) != 0)
                .SelectMany(x => x.GetComponentsInChildren<AudioSource>(true))
                .Distinct()
                .ToArray();
        }
    }
}
