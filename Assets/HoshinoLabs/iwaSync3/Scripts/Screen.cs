using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.v8xuael90eyq")]
    public class Screen : IwaSync3Base
    {
#pragma warning disable CS0414
        [SerializeField]
        [Tooltip("制御元のiwaSync3を指定します")]
        IwaSync3 iwaSync3;

        [SerializeField]
        [Tooltip("反映先のマテリアルが複数ある場合はインデックスを指定します")]
        int materialIndex = 0;
        [SerializeField]
        [Tooltip("反映したいマテリアルプロパティの名前を指定します")]
        string textureProperty = "_MainTex";
        [SerializeField]
        [Tooltip("アイドル時に表示をオフにするか")]
        bool idleScreenOff = false;
        [SerializeField]
        [Tooltip("アイドル時に指定したテクスチャを表示する場合は設定")]
        Texture idleScreenTexture = null;
        [SerializeField]
        [Tooltip("スクリーンのアスペクト比を指定")]
        float aspectRatio = 1.777778f;
        [SerializeField]
        [Tooltip("ミラーに映る映像を反転するかを指定します")]
        bool defaultMirror = true;
        [SerializeField]
        [Tooltip("明るさの倍率を指定します")]
        [Range(0f, 5f)]
        float defaultEmissiveBoost = 1f;
        [SerializeField]
        [Tooltip("描画先のレンダラを指定します")]
        Renderer screen;
#pragma warning restore CS0414
    }
}
