using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.qzpvauo31rqw")]
    public class Speaker : IwaSync3Base
    {
#pragma warning disable CS0414
        [SerializeField]
        [Tooltip("制御元のiwaSync3を指定します")]
        IwaSync3 iwaSync3;
        [SerializeField]
        [Tooltip("再生モードごとに使用するかのマスク設定\nそれぞれのモードで使用するかを指定します")]
        TrackModeMask mask = (TrackModeMask)(-1);
        [SerializeField]
        [Tooltip("優先スピーカーに設定するか指定します\nVideoの時このスピーカーを優先して使用します")]
        bool primary = false;

        [SerializeField]
        [Tooltip("音声が最大で聴こえる距離を指定します")]
        float maxDistance = 12f;

        [SerializeField]
        [Tooltip("立体音響を有効にするか指定します")]
        bool spatialize = false;

        [SerializeField]
        [Tooltip("スピーカーに出力する音声信号の種類を指定します")]
        ChannelMode mode;
#pragma warning restore CS0414
    }
}
