using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(DesktopBar))]
    internal class DesktopBarEditor : IwaSync3EditorBase
    {
        DesktopBar _target;

        SerializedProperty _iwaSync3Property;

        SerializedProperty _desktopOnlyProperty;

        protected new void FindProperties()
        {
            base.FindProperties();

            _target = target as DesktopBar;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");

            _desktopOnlyProperty = serializedObject.FindProperty("desktopOnly");
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
                EditorGUILayout.PropertyField(_desktopOnlyProperty);
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
            var controller = iwaSync3.GetComponentInChildren<Udon.VideoController>(true);

            var self = _target.GetComponentInChildren<Udon.DesktopBar>(true);
            self.SetPublicVariable("core", core);
            self.SetPublicVariable("controller", controller);

            self.SetPublicVariable("desktopOnly", _desktopOnlyProperty.boolValue);

            var canvas1 = self.transform.Find("Canvas").gameObject;
            self.SetPublicVariable("_canvas1", canvas1);
            var lockOn = self.transform.Find("Canvas/Panel/Help/Group/Lock/Value/Icon/On").gameObject;
            self.SetPublicVariable("_lockOn", lockOn);
            var lockOff = self.transform.Find("Canvas/Panel/Help/Group/Lock/Value/Icon/Off").gameObject;
            self.SetPublicVariable("_lockOff", lockOff);
            var video = self.transform.Find("Canvas/Panel/Help/Group/Mode/Value/Icon/Video").gameObject;
            self.SetPublicVariable("_video", video);
            var live = self.transform.Find("Canvas/Panel/Help/Group/Mode/Value/Icon/Live").gameObject;
            self.SetPublicVariable("_live", live);
            var muteOn = self.transform.Find("Canvas/Panel/Help/Group/Mute/Value/Icon/On").gameObject;
            self.SetPublicVariable("_muteOn", muteOn);
            var muteOff = self.transform.Find("Canvas/Panel/Help/Group/Mute/Value/Icon/Off").gameObject;
            self.SetPublicVariable("_muteOff", muteOff);
            var volume = self.transform.Find("Canvas/Panel/Help/Group/Volume").gameObject;
            self.SetPublicVariable("_volume", volume);
            self.SetPublicVariable("_volumeText", volume.transform.Find("Value").GetComponent<Text>());
            var pauseOn = self.transform.Find("Canvas/Panel/Help/Group (1)/Pause/Value/Icon/On").gameObject;
            self.SetPublicVariable("_pauseOn", pauseOn);
            var pauseOff = self.transform.Find("Canvas/Panel/Help/Group (1)/Pause/Value/Icon/Off").gameObject;
            self.SetPublicVariable("_pauseOff", pauseOff);
            var loopOn = self.transform.Find("Canvas/Panel/Help/Group (1)/Loop/Value/Icon/On").gameObject;
            self.SetPublicVariable("_loopOn", loopOn);
            var loopOff = self.transform.Find("Canvas/Panel/Help/Group (1)/Loop/Value/Icon/Off").gameObject;
            self.SetPublicVariable("_loopOff", loopOff);

            var canvas2 = self.transform.Find("Canvas (1)").gameObject;
            self.SetPublicVariable("_canvas2", canvas2);
            self.SetPublicVariable("_canvas2Rect", canvas2.GetComponent<RectTransform>());
            var address = self.transform.Find("Canvas (1)/Address").gameObject;
            self.SetPublicVariable("_address", address);
            self.SetPublicVariable("_addressInput", address.GetComponent<VRCUrlInputField>());

            var canvasPIP = self.transform.Find("Canvas (PIP)").gameObject;
            self.SetPublicVariable("_canvasPIP", canvasPIP);
            var image1 = canvasPIP.transform.Find("Panel/RawImage");
            self.SetPublicVariable("_image1", image1.GetComponent<RawImage>());
            self.SetPublicVariable("_image1Rect", image1.GetComponent<RectTransform>());
            self.SetPublicVariable("_image1Aspect", image1.GetComponent<AspectRatioFitter>());

            var canvasFullScreen = self.transform.Find("Canvas (FullScreen)").gameObject;
            self.SetPublicVariable("_canvasFullScreen", canvasFullScreen);
            var image2 = canvasFullScreen.transform.Find("Panel/RawImage");
            self.SetPublicVariable("_image2", image2.GetComponent<RawImage>());
            self.SetPublicVariable("_image2Rect", image2.GetComponent<RectTransform>());
            self.SetPublicVariable("_image2Aspect", image2.GetComponent<AspectRatioFitter>());
        }
    }
}
