using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    internal class RealtimeEmissiveGammaShaderGUI : ShaderGUI
    {
        Material _target;

        void FindProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _target = materialEditor.target as Material;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(materialEditor, properties);

            _target.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            base.OnGUI(materialEditor, properties);
        }
    }
}
