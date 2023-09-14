using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(ListTab))]
    internal class ListTabEditor : IwaSync3EditorBase
    {
        ListTab _target;

        SerializedProperty _iwaSync3Property;

        SerializedProperty _allowSwitchOffProperty;
        SerializedProperty _multiViewProperty;

        SerializedProperty _tabsProperty;

        ReorderableList _tabsList;

        protected override void FindProperties()
        {
            base.FindProperties();

            _target = target as ListTab;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");

            _allowSwitchOffProperty = serializedObject.FindProperty("allowSwitchOff");
            _multiViewProperty = serializedObject.FindProperty("multiView");

            var tabsProperty = serializedObject.FindProperty("tabs");
            if (_tabsList == null || _tabsProperty.serializedObject != _tabsProperty.serializedObject)
            {
                _tabsProperty = tabsProperty;
                _tabsList = new ReorderableList(serializedObject, tabsProperty)
                {
                    drawHeaderCallback = (rect) =>
                    {
                        EditorGUI.LabelField(rect, tabsProperty.displayName);
                    },
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        EditorGUI.PropertyField(rect, tabsProperty.GetArrayElementAtIndex(index));
                    },
                    elementHeightCallback = (index) =>
                    {
                        return EditorGUI.GetPropertyHeight(tabsProperty.GetArrayElementAtIndex(index)) + EditorGUIUtility.standardVerticalSpacing;
                    },
                    onReorderCallback = (list) =>
                    {
                        ApplyModifiedProperties();
                    }
                };
            }
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
                EditorGUILayout.PropertyField(_allowSwitchOffProperty);
                EditorGUILayout.PropertyField(_multiViewProperty);
            }

            EditorGUILayout.Space();

            _tabsList.DoLayoutList();

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
            var controller = iwaSync3.GetComponentInChildren<Udon.VideoController>(true);

            var lists = Enumerable.Range(0, _tabsProperty.arraySize)
                .Select(x => _tabsProperty.GetArrayElementAtIndex(x))
                .Select(x => (ListBase)x.FindPropertyRelative("list").objectReferenceValue)
                .Select(x => x?.gameObject)
                .ToArray();
            var tabs = lists
                .Select(x => x == null ? false : x.activeSelf)
                .ToArray();

            if (!_multiViewProperty.boolValue)
            {
                var active = false;
                for (var i = 0; i < tabs.Length; i++)
                {
                    active = !active && tabs[i];
                    tabs[i] = active;
                }
            }

            var self = _target.GetComponentInChildren<Udon.ListTab>(true);
            self.SetPublicVariable("core", core);
            self.SetPublicVariable("controller", controller);
            self.SetPublicVariable("allowSwitchOff", _allowSwitchOffProperty.boolValue);
            self.SetPublicVariable("multiView", _multiViewProperty.boolValue);

            var lockOn = self.transform.Find("Canvas/Panel/Header/Lock/On").gameObject;
            self.SetPublicVariable("_lockOn", lockOn);
            self.SetPublicVariable("_lockOnButton", lockOn.transform.Find("Button").GetComponent<Button>());
            var lockOff = self.transform.Find("Canvas/Panel/Header/Lock/Off").gameObject;
            self.SetPublicVariable("_lockOff", lockOff);
            self.SetPublicVariable("_lockOffButton", lockOff.transform.Find("Button").GetComponent<Button>());
            var content = self.transform.Find("Canvas/Panel/Scroll View/Scroll View/Viewport/Content");
            self.SetPublicVariable("_content", content);

            var template = content.Find("Template");

            for (var i = content.childCount - 1; 0 < i; i--)
            {
                var item = content.GetChild(i);
                if (item == template)
                    continue;
                DestroyImmediate(item.gameObject);
            }

            //var content_objs = Array.Empty<GameObject>();
            var content_toggles = Array.Empty<Toggle>();
            var content_lists = Array.Empty<GameObject>();

            for (var i = 0; i < _tabsProperty.arraySize; i++)
            {
                var track = _tabsProperty.GetArrayElementAtIndex(i);
                var title = track.FindPropertyRelative("title").stringValue;
                var list = (ListBase)track.FindPropertyRelative("list").objectReferenceValue;

                var obj = Instantiate(template.gameObject, content, false);
                obj.SetActive(true);
                var toggleText = obj.transform.Find("Toggle/Label").GetComponent<Text>();
                toggleText.text = title;
                var toggleToggle = obj.transform.Find("Toggle").GetComponent<Toggle>();
                GameObjectUtility.EnsureUniqueNameForSibling(obj);

                //ArrayUtility.Add(ref content_objs, obj);
                ArrayUtility.Add(ref content_toggles, toggleToggle);
                ArrayUtility.Add(ref content_lists, list?.gameObject);
            }

            self.SetPublicVariable("_content_length", _tabsProperty.arraySize);
            //self.SetPublicVariable("_content_objs", content_objs);
            self.SetPublicVariable("_content_toggles", content_toggles);
            self.SetPublicVariable("_content_lists", content_lists);

            var message = self.transform.Find("Canvas/Panel/Scroll View/Message").gameObject;
            self.SetPublicVariable("_message", message);
            self.SetPublicVariable("_messageText", message.transform.Find("Text").GetComponent<Text>());

            self.SetPublicVariable("_tabs", tabs);
        }
    }
}
