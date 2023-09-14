using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    internal abstract class IwaSync3EditorBase : Editor
    {
        Texture2D _splash;

        protected GUIStyle _headerTitleStyle;
        protected GUIStyle _headerVersionStyle;
        protected GUIStyle _italicStyle;

        protected virtual void FindProperties()
        {
            _splash = Resources.Load<Texture2D>($"{Udon.IwaSync3.APP_NAME}_Splash");

            _headerTitleStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic,
                fontSize = 14,
            };
            _headerTitleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            _headerVersionStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic,
            };
            _headerVersionStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            _italicStyle = new GUIStyle()
            {
                fontStyle = FontStyle.Italic,
            };
            _italicStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
        }

        void OnInspectorHeader()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.Label($"{Udon.IwaSync3.APP_NAME}", _headerTitleStyle);
                GUILayout.Label($"{Udon.IwaSync3.APP_VERSION}", _headerVersionStyle);
            }
        }

        void OnInspectorSplash()
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(_splash.height));
            rect.xMin = Mathf.Max((rect.width - _splash.width + 18f) / 2f, 0f);
            rect.width = _splash.width;
            rect.height = _splash.height;
            GUI.DrawTexture(rect, _splash, ScaleMode.ScaleToFit, true, 0f);
        }

        bool OnInspectorVersionCheck()
        {
            if (!VersionChecker.VersionChecked)
                return false;

            var message = string.Empty;
            if(VersionChecker.LatestVersion == null)
            {
                message = string.Join("\n",
                    $"最新のバージョンが取得できませんでした",
                    $"BOOTH から新しいバージョンを取得して更新して下さい"
                );
            }
            else
            {
                if (VersionChecker.CurrentVersion < VersionChecker.LatestVersion)
                {
                    message = string.Join("\n",
                        $"最新のバージョン v{VersionChecker.LatestVersion.ToString(3)} がリリースされています",
                        $"BOOTH から新しいバージョンを取得して更新して下さい"
                    );
                }
            }

            if (string.IsNullOrEmpty(message))
                return false;

            EditorGUILayout.HelpBox(message, MessageType.Warning);
            if (GUILayout.Button("BOOTH ダウンロードページ"))
                Application.OpenURL("https://hoshinolabs.booth.pm/items/2666275");

            return true;
        }

        public override void OnInspectorGUI()
        {
            OnInspectorHeader();
            EditorGUILayout.Space();

            OnInspectorSplash();
            EditorGUILayout.Space();

            if (OnInspectorVersionCheck())
                EditorGUILayout.Space();
        }

        internal IwaSync3 GetMainIwaSync3(SerializedProperty property)
        {
            var iwaSync3 = ((Component)target).GetComponentInParent<IwaSync3>();
            if (iwaSync3)
            {
                while (true)
                {
                    property = new SerializedObject(iwaSync3).FindProperty("iwaSync3");
                    if (property == null || property.objectReferenceValue == null)
                        return iwaSync3;
                    var obj = (IwaSync3)property.objectReferenceValue;
                    if (obj == null || obj == iwaSync3)
                        return iwaSync3;
                    iwaSync3 = obj;
                }
            }
            var objs = FindObjectsOfType<IwaSync3>(true);
            if (objs.Length == 1)
                return objs.First();
            if (property != null && property.objectReferenceValue != null)
            {
                iwaSync3 = (IwaSync3)property.objectReferenceValue;
                while (true)
                {
                    property = new SerializedObject(iwaSync3).FindProperty("iwaSync3");
                    if (property == null || property.objectReferenceValue == null)
                        return iwaSync3;
                    var obj = (IwaSync3)property.objectReferenceValue;
                    if (obj == null || obj == iwaSync3)
                        return iwaSync3;
                    iwaSync3 = obj;
                }
            }
            if (property == null)
                return null;
            return objs.FirstOrDefault();
        }

        internal IwaSync3 GetMainIwaSync3<T>(T target) where T : IwaSync3Base
        {
            var property = (SerializedProperty)null;
            var iwaSync3 = target.GetComponentInParent<IwaSync3>();
            if (iwaSync3)
            {
                while (true)
                {
                    property = new SerializedObject(iwaSync3).FindProperty("iwaSync3");
                    if (property == null || property.objectReferenceValue == null)
                        return iwaSync3;
                    var obj = (IwaSync3)property.objectReferenceValue;
                    if (obj == null || obj == iwaSync3)
                        return iwaSync3;
                    iwaSync3 = obj;
                }
            }
            var objs = FindObjectsOfType<IwaSync3>(true);
            if (objs.Length == 1)
                return objs.First();
            property = new SerializedObject(target).FindProperty("iwaSync3");
            if (property != null && property.objectReferenceValue != null)
            {
                iwaSync3 = (IwaSync3)property.objectReferenceValue;
                while (true)
                {
                    property = new SerializedObject(iwaSync3).FindProperty("iwaSync3");
                    if (property == null || property.objectReferenceValue == null)
                        return iwaSync3;
                    var obj = (IwaSync3)property.objectReferenceValue;
                    if (obj == null || obj == iwaSync3)
                        return iwaSync3;
                    iwaSync3 = obj;
                }
            }
            if (property == null)
                return null;
            return objs.FirstOrDefault();
        }

        internal T[] FindObjectsOfType<T>(bool includeInactive) where T : Component
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(x => AssetDatabase.GetAssetOrScenePath(x).EndsWith(".unity"))
                .Where(x => includeInactive || x.gameObject.activeInHierarchy)
                .OrderBy(x => x.gameObject.transform.root.GetSiblingIndex())
                .ToArray();
        }

        internal virtual void ApplyModifiedProperties() { }
    }
}
