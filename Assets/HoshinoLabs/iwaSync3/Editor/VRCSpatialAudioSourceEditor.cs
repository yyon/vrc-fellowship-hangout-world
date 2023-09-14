using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(VRCSpatialAudioSource))]
    [CanEditMultipleObjects]
    internal class VRCSpatialAudioSourceEditor : Editor
    {
        Editor _editor;

        bool _controlled;

        private void OnEnable()
        {
            var editorType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetType("VRC.SDK3.Editor.VRC_SpatialAudioSourceEditor3"))
                .Where(x => x != null)
                .FirstOrDefault();
            _editor = Editor.CreateEditor(target, editorType);

            var spatial = target as VRCSpatialAudioSource;
            var speaker = spatial?.GetComponentInParent<Speaker>();
            _controlled = speaker?.GetComponentInChildren<VRCSpatialAudioSource>(true) == spatial;
        }

        private void OnDisable()
        {
            GameObject.DestroyImmediate(_editor);
        }

        public override void OnInspectorGUI()
        {
            if (_controlled)
                EditorGUILayout.HelpBox($"このコンポーネントの一部パラメーターは{Udon.IwaSync3.APP_NAME}によって制御されています", MessageType.Warning);

            _editor.OnInspectorGUI();
        }
    }
}
