using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(Queuelist))]
    internal class QueuelistEditor : IwaSync3EditorBase
    {
        Queuelist _target;

        SerializedProperty _iwaSync3Property;

        protected override void FindProperties()
        {
            base.FindProperties();

            _target = target as Queuelist;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");
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

            var self = _target.GetComponentInChildren<Udon.Queuelist>(true);
            self.SetPublicVariable("core", core);
            self.SetPublicVariable("controller", controller);

            var lockOn = self.transform.Find("Canvas/Panel/Header/Lock/On").gameObject;
            self.SetPublicVariable("_lockOn", lockOn);
            self.SetPublicVariable("_lockOnButton", lockOn.transform.Find("Button").GetComponent<Button>());
            var lockOff = self.transform.Find("Canvas/Panel/Header/Lock/Off").gameObject;
            self.SetPublicVariable("_lockOff", lockOff);
            self.SetPublicVariable("_lockOffButton", lockOff.transform.Find("Button").GetComponent<Button>());
            var playOn = self.transform.Find("Canvas/Panel/Header/Play/On").gameObject;
            self.SetPublicVariable("_playOn", playOn);
            self.SetPublicVariable("_playOnButton", playOn.transform.Find("Button").GetComponent<Button>());
            var playOff = self.transform.Find("Canvas/Panel/Header/Play/Off").gameObject;
            self.SetPublicVariable("_playOff", playOff);
            self.SetPublicVariable("_playOffButton", playOff.transform.Find("Button").GetComponent<Button>());
            var forward = self.transform.Find("Canvas/Panel/Header/Forward").gameObject;
            self.SetPublicVariable("_forward", forward);
            self.SetPublicVariable("_forwardButton", forward.transform.Find("Button").GetComponent<Button>());
            var content = self.transform.Find("Canvas/Panel/Scroll View/Scroll View/Viewport/Content");
            self.SetPublicVariable("_content", content);

            var template = self.transform.Find("Canvas/Panel/Scroll View/Scroll View/Viewport/Content/Template").gameObject;
            self.SetPublicVariable("_template", template);
            var address = template.transform.Find("Panel (2)/Panel/Address").gameObject;
            var addressInput = address.GetComponent<VRCUrlInputField>();
            addressInput.textComponent = address.transform.Find("Text").GetComponent<Text>();// SDK bug countermeasures

            var message = self.transform.Find("Canvas/Panel/Scroll View/Message").gameObject;
            self.SetPublicVariable("_message", message);
            self.SetPublicVariable("_messageText", message.transform.Find("Text").GetComponent<Text>());
        }
    }
}
