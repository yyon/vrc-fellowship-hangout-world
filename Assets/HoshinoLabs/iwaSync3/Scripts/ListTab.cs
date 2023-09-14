using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    public class ListTab : ListBase
    {
        [Serializable]
        public class Tab
        {
            public string title;
            public ListBase list;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Tab))]
        public class TabDrawer : PropertyDrawer
        {
            SerializedProperty _titleProperty;
            SerializedProperty _listProperty;

            void FindProperties(SerializedProperty property)
            {
                _titleProperty = property.FindPropertyRelative("title");
                _listProperty = property.FindPropertyRelative("list");
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                FindProperties(property);

                return EditorGUI.GetPropertyHeight(_titleProperty) + EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(_listProperty);
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                FindProperties(property);

                using (new EditorGUI.PropertyScope(position, label, property))
                {
                    EditorGUIUtility.labelWidth = 64f;
                    PropertyField(ref position, _titleProperty);
                    PropertyField(ref position, _listProperty);
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
        bool allowSwitchOff = true;
        [SerializeField]
        bool multiView = false;

        [SerializeField]
        Tab[] tabs;
#pragma warning restore CS0414
    }
}
