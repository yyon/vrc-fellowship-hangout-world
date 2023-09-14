using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.w55eaf4xlnuu")]
    public class IwaSync3 : IwaSync3Base
    {
#pragma warning disable CS0414
        // main
        [SerializeField]
        IwaSync3 iwaSync3;

        // control
        [SerializeField]
        [Tooltip("デフォルト再生を利用する場合の再生モードを指定します")]
        TrackMode defaultMode = TrackMode.Video;
        [SerializeField]
        [Tooltip("デフォルト再生で再生する動画のURLを指定します")]
        string defaultUrl;
        [SerializeField]
        [Tooltip("デフォルト再生を遅延させる秒数を指定します")]
        float defaultDelay = 0f;
        [SerializeField]
        [Tooltip("シーク(スライダで再生位置変更)を許可するか")]
        bool allowSeeking = true;
        [SerializeField]
        [Tooltip("デフォルトで１ループをオンにするか")]
        bool defaultLoop = false;
        [SerializeField]
        [Tooltip("左右早送りボタンを押したときに移動する秒数")]
        float seekTimeSeconds = 10f;
        [SerializeField]
        [Tooltip("時間表示のフォーマット")]
        string timeFormat = @"hh\:mm\:ss\:ff";

        // sync
        [SerializeField]
        [Tooltip("再生時間の同期修正を行う間隔")]
        float syncFrequency = 9.2f;
        [SerializeField]
        [Tooltip("再生時間の同期修正を行う閾値")]
        float syncThreshold = 0.92f;

        // error handling
        [SerializeField]
        [Tooltip("エラー発生時のリトライ最大回数")]
        [Range(0, 10)]
        int maxErrorRetry = 3;
        [SerializeField]
        [Tooltip("動画の再生が正しく行えないと認識するまでの時間")]
        [Range(10f, 30f)]
        float timeoutUnknownError = 10f;
        [SerializeField]
        [Tooltip("動画の再生に失敗した時にリトライするまでの待機時間")]
        [Range(6f, 30f)]
        float timeoutPlayerError = 6f;
        [SerializeField]
        [Tooltip("動画のレート制限が発生した時リトライするまでの待機時間")]
        [Range(6f, 30f)]
        float timeoutRateLimited = 6f;
        [SerializeField]
        [Tooltip("エラー発生時に最大解像度を下げることを許可する")]
        bool allowErrorReduceMaxResolution = true;

        // lock
        [SerializeField]
        [Tooltip("デフォルトでマスターロック状態にするか")]
        bool defaultLock = false;
        [SerializeField]
        [Tooltip("マスターロック状態をインスタンスのオーナーに拡大するか")]
        bool allowInstanceOwner = true;

        // video
        [SerializeField]
        [Tooltip("再生した動画の解像度が選択可能なとき選ぶ最大解像度")]
        int maximumResolution = 720;

        // audio
        [SerializeField]
        [Tooltip("デフォルトでミュート状態にするか")]
        bool defaultMute = false;
        [SerializeField]
        [Tooltip("デフォルトの最小音量")]
        [Range(0f, 1f)]
        float defaultMinVolume = 0f;
        [SerializeField]
        [Tooltip("デフォルトの最大音量")]
        [Range(0f, 1f)]
        float defaultMaxVolume = 0.5f;
        [SerializeField]
        [Tooltip("デフォルトの音量")]
        [Range(0f, 1f)]
        float defaultVolume = 0.184f;

        // extra
        [SerializeField]
        //[Tooltip("Low latency playback of live stream")]
        [Tooltip("ライブで低遅延再生を利用するか")]
        bool useLowLatency = false;
#pragma warning restore CS0414
    }
}
