using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(Screen))]
    [CanEditMultipleObjects]
    internal class ScreenEditor : IwaSync3EditorBase
    {
        Screen _target;

        SerializedProperty _iwaSync3Property;

        SerializedProperty _materialIndexProperty;
        SerializedProperty _texturePropertyProperty;
        SerializedProperty _idleScreenOffProperty;
        SerializedProperty _idleScreenTextureProperty;
        SerializedProperty _aspectRatioProperty;
        SerializedProperty _defaultMirrorProperty;
        SerializedProperty _defaultEmissiveBoostProperty;
        SerializedProperty _screenProperty;

        protected override void FindProperties()
        {
            base.FindProperties();

            _target = target as Screen;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");

            _materialIndexProperty = serializedObject.FindProperty("materialIndex");
            _texturePropertyProperty = serializedObject.FindProperty("textureProperty");
            _idleScreenOffProperty = serializedObject.FindProperty("idleScreenOff");
            _idleScreenTextureProperty = serializedObject.FindProperty("idleScreenTexture");
            _aspectRatioProperty = serializedObject.FindProperty("aspectRatio");
            _defaultMirrorProperty = serializedObject.FindProperty("defaultMirror");
            _defaultEmissiveBoostProperty = serializedObject.FindProperty("defaultEmissiveBoost");
            _screenProperty = serializedObject.FindProperty("screen");
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
            }

            EditorGUILayout.Space();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Options", _italicStyle);
                EditorGUILayout.PropertyField(_materialIndexProperty);
                EditorGUILayout.PropertyField(_texturePropertyProperty);
                EditorGUILayout.PropertyField(_idleScreenOffProperty);
                EditorGUILayout.PropertyField(_idleScreenTextureProperty);
                EditorGUILayout.PropertyField(_aspectRatioProperty);
                EditorGUILayout.PropertyField(_defaultMirrorProperty);
                EditorGUILayout.PropertyField(_defaultEmissiveBoostProperty);
                EditorGUILayout.PropertyField(_screenProperty);
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

            var self = _target.GetComponentInChildren<Udon.VideoScreen>(true);
            self.SetPublicVariable("core", core);
            self.SetPublicVariable("materialIndex", _materialIndexProperty.intValue);
            self.SetPublicVariable("textureProperty", _texturePropertyProperty.stringValue);
            self.SetPublicVariable("idleScreenOff", _idleScreenOffProperty.boolValue);
            self.SetPublicVariable("idleScreenTexture", _idleScreenTextureProperty.objectReferenceValue);
            self.SetPublicVariable("aspectRatio", _aspectRatioProperty.floatValue);
            self.SetPublicVariable("defaultMirror", _defaultMirrorProperty.boolValue);
            self.SetPublicVariable("defaultEmissiveBoost", _defaultEmissiveBoostProperty.floatValue);
            self.SetPublicVariable("screen", (Renderer)_screenProperty.objectReferenceValue);
        }
    }
}
