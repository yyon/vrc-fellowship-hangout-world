using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.hp0a2t9tw8cw")]
    public class Playlist : ListBase
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Track))]
        public class TrackDrawer : PropertyDrawer
        {
            SerializedProperty _modeProperty;
            SerializedProperty _titleProperty;
            SerializedProperty _urlProperty;

            void FindProperties(SerializedProperty property)
            {
                _modeProperty = property.FindPropertyRelative("mode");
                _titleProperty = property.FindPropertyRelative("title");
                _urlProperty = property.FindPropertyRelative("url");
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                FindProperties(property);

                return EditorGUI.GetPropertyHeight(_modeProperty) + EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(_titleProperty) + EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(_urlProperty);
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                FindProperties(property);

                using (new EditorGUI.PropertyScope(position, label, property))
                {
                    EditorGUIUtility.labelWidth = 64f;
                    PropertyField(ref position, _modeProperty);
                    PropertyField(ref position, _titleProperty);
                    PropertyField(ref position, _urlProperty);
                }
            }

            void PropertyField(ref Rect position, SerializedProperty property)
            {
                position.height = EditorGUI.GetPropertyHeight(property);
                EditorGUI.PropertyField(position, property);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }
#endif

#pragma warning disable CS0414
        [SerializeField]
        IwaSync3 iwaSync3;

        [SerializeField]
        bool defaultShuffle = false;
        [SerializeField]
        bool defaultRepeat = true;
        [SerializeField]
        bool playOnAwake = false;

        [SerializeField]
        string playlistUrl;
        [SerializeField]
        [Tooltip("取得したURLの先頭に設定する文字列を指定します")]
        string playlistPrefix;
        [SerializeField]
        [Tooltip("プレイリストから取得する上限を指定します(0で制限無し)")]
        int playlistLimitCount;
        [SerializeField]
        [Tooltip("ショート動画を除外します")]
        bool playlistExcludeShortVideo;

        [SerializeField]
        Track[] tracks;
#pragma warning restore CS0414
    }
}
