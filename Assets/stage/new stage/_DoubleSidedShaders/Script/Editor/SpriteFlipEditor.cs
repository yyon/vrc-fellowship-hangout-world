using UnityEngine;
using UnityEditor;
using System.Collections;


namespace MLSpace
{
    [CustomEditor(typeof(SpriteFlip))]
    public class SpriteFlipEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            SpriteFlip script = (SpriteFlip)target;

            DrawDefaultInspector();

            bool flip = (bool)EditorGUILayout.Toggle("Flip", script.flippedX);

            script.flip(flip);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
                serializedObject.ApplyModifiedProperties();
            }
        }
    } 

}
