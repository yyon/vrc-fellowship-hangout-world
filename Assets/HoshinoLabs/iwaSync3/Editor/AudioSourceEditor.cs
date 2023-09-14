using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(AudioSource))]
    [CanEditMultipleObjects]
    internal class AudioSourceEditor : Editor
    {
        Editor _editor;

        bool _controlled;

        private void OnEnable()
        {
            var editorType = typeof(Editor).Assembly.GetType("UnityEditor.AudioSourceInspector");
            _editor = Editor.CreateEditor(target, editorType);

            var audioSource = target as AudioSource;
            var speaker = audioSource.GetComponentInParent<Speaker>();
            _controlled = speaker?.GetComponentInChildren<AudioSource>() == audioSource;
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
