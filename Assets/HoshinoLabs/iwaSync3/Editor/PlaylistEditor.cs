using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace HoshinoLabs.IwaSync3
{
    [CustomEditor(typeof(Playlist))]
    internal class PlaylistEditor : IwaSync3EditorBase
    {
        Playlist _target;

        SerializedProperty _iwaSync3Property;

        SerializedProperty _defaultShuffleProperty;
        SerializedProperty _defaultRepeatProperty;
        SerializedProperty _playOnAwakeProperty;

        SerializedProperty _playlistUrlProperty;
        SerializedProperty _playlistPrefixProperty;
        SerializedProperty _playlistLimitCountProperty;
        SerializedProperty _playlistExcludeShortVideoProperty;

        SerializedProperty _tracksProperty;

        ReorderableList _tracksList;

        static Task _task = null;

        [Serializable]
        public class PlaylistJson
        {
            public Track[] tracks;
        }

        protected override void FindProperties()
        {
            base.FindProperties();

            _target = target as Playlist;

            _iwaSync3Property = serializedObject.FindProperty("iwaSync3");

            _defaultShuffleProperty = serializedObject.FindProperty("defaultShuffle");
            _defaultRepeatProperty = serializedObject.FindProperty("defaultRepeat");
            _playOnAwakeProperty = serializedObject.FindProperty("playOnAwake");

            _playlistUrlProperty = serializedObject.FindProperty("playlistUrl");
            _playlistPrefixProperty = serializedObject.FindProperty("playlistPrefix");
            _playlistLimitCountProperty = serializedObject.FindProperty("playlistLimitCount");
            _playlistExcludeShortVideoProperty = serializedObject.FindProperty("playlistExcludeShortVideo");

            var tracksProperty = serializedObject.FindProperty("tracks");
            if (_tracksList == null || _tracksProperty.serializedObject != _tracksProperty.serializedObject)
            {
                _tracksProperty = tracksProperty;
                _tracksList = new ReorderableList(serializedObject, tracksProperty)
                {
                    drawHeaderCallback = (rect) =>
                    {
                        EditorGUI.LabelField(rect, tracksProperty.displayName);
                    },
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        EditorGUI.PropertyField(rect, tracksProperty.GetArrayElementAtIndex(index));
                    },
                    elementHeightCallback = (index) =>
                    {
                        return EditorGUI.GetPropertyHeight(tracksProperty.GetArrayElementAtIndex(index)) + EditorGUIUtility.standardVerticalSpacing;
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
                EditorGUILayout.LabelField("Tools", _italicStyle);
                EditorGUILayout.HelpBox(string.Join("\n",
                        "試験的な機能です",
                        "使用前にシーンのバックアップを取るなどの対応をお願いします",
                        "現在はYoutubeのみ対応しています",
                        "モードは取得できないので必要に応じて手動設定して下さい",
                        "文字によっては文字化けする場合があります"
                    ), MessageType.Info
                );
                var exists = YoutubeDLHelper.Exists;
                if (!exists)
                {
                    EditorGUILayout.HelpBox(string.Join("\n",
                            "yt-dlp.exeが見つからないためこの機能は使用できません",
                            "この機能の追加ツールの導入が必要です。",
                            "「yt-dlpの更新」ボタンから自動設定するか、",
                            "手動で「yt-dlp.exe」をダウンロードしプロジェクトのトップ(Assetsフォルダなどがあるところ)に配置して下さい",
                            "https://github.com/yt-dlp/yt-dlp/releases"
                        ), MessageType.Warning
                    );
                }
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("★yt-dlpの自動設定", _italicStyle);
                    if(_task == null)
                    {
                        if (GUILayout.Button("Run"))
                        {
                            var url = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
                            var tcs = new TaskCompletionSource<UnityWebRequest>();
                            var request = UnityWebRequest.Get(url);
                            var async = request.SendWebRequest();
                            async.completed += _ =>
                            {
                                try
                                {
                                    if (request.isHttpError || request.isNetworkError)
                                        throw new Exception();

                                    File.WriteAllBytes(YoutubeDLHelper.ToolPath, request.downloadHandler.data);
                                }
                                catch (Exception)
                                {
                                    Debug.LogError($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] yt-dlpのダウンロードに失敗しました。");
                                }

                                _task = null;

                                Debug.Log($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] yt-dlpのダウンロードが完了しました。");
                            };
                            _task = tcs.Task;
                        }
                    }
                    else
                    {
                        using (new EditorGUI.DisabledGroupScope(true))
                            GUILayout.Button("Downloading...");
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!exists))
                {
                    using (new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.LabelField("★プレイリスト(Youtube)から自動取得", _italicStyle);
                        EditorGUILayout.PropertyField(_playlistUrlProperty);
                        EditorGUILayout.PropertyField(_playlistPrefixProperty);
                        EditorGUILayout.PropertyField(_playlistLimitCountProperty);
                        EditorGUILayout.PropertyField(_playlistExcludeShortVideoProperty);
                        if (PlaylistTools.IsCompleted)
                        {
                            var valid = PlaylistTools.IsValidPlayListURL(_playlistUrlProperty.stringValue);
                            if(!valid && !string.IsNullOrEmpty(_playlistUrlProperty.stringValue))
                            {
                                EditorGUILayout.HelpBox(string.Join("\n",
                                        "YoutubeのプレイリストURLではないようです"
                                    ), MessageType.Warning
                                );
                            }
                            using (new EditorGUI.DisabledGroupScope(!valid))
                            {
                                if (GUILayout.Button("Run"))
                                    _ = PlaylistTools.ResolvePlaylist(_target);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Abort"))
                                PlaylistTools.Cancel();
                        }
                    }
                }
                using (new EditorGUI.DisabledGroupScope(!exists))
                {
                    using (new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.LabelField("★動画のタイトルを自動取得", _italicStyle);
                        if (PlaylistTools.IsCompleted)
                        {
                            if (GUILayout.Button("Run"))
                                _ = PlaylistTools.ResolveTracks(_target);
                        }
                        else
                        {
                            if (GUILayout.Button("Abort"))
                                PlaylistTools.Cancel();
                        }
                    }
                }
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("★プレイリストをjsonファイルへ書き出し", _italicStyle);
                    if (GUILayout.Button("Run"))
                    {
                        var path = EditorUtility.SaveFilePanel("Save Json", Directory.GetCurrentDirectory(), "playlist", "json");
                        if (string.IsNullOrEmpty(path))
                            return;

                        try
                        {
                            var tracks = Enumerable.Range(0, _tracksProperty.arraySize)
                                .Select(x => _tracksProperty.GetArrayElementAtIndex(x))
                                .Select(x => new Track
                                {
                                    mode = (TrackMode)x.FindPropertyRelative("mode").intValue,
                                    title = x.FindPropertyRelative("title").stringValue,
                                    url = x.FindPropertyRelative("url").stringValue
                                })
                                .ToArray();

                            var data = new PlaylistJson { tracks = tracks };
                            var json = JsonUtility.ToJson(data);

                            File.WriteAllText(path, json);

                            Debug.Log($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] プレイリストをjsonファイルへ書き出しました。");
                        }
                        catch (Exception)
                        {
                            Debug.LogError($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] プレイリストをjsonファイルへ書き出しに失敗しました。");
                        }
                    }
                }
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("★プレイリストをjsonファイルから読み込み", _italicStyle);
                    if (GUILayout.Button("Run"))
                    {
                        var path = EditorUtility.OpenFilePanel("Open Json", Directory.GetCurrentDirectory(), "json");
                        if (string.IsNullOrEmpty(path))
                            return;

                        try
                        {
                            var json = File.ReadAllText(path);
                            var data = JsonUtility.FromJson<PlaylistJson>(json);

                            _tracksProperty.arraySize = data.tracks.Length;
                            for (var i = 0; i < data.tracks.Length; i++)
                            {
                                var trackProperty = _tracksProperty.GetArrayElementAtIndex(i);
                                var track = data.tracks[i];
                                trackProperty.FindPropertyRelative("mode").intValue = (int)track.mode;
                                trackProperty.FindPropertyRelative("title").stringValue = track.title;
                                trackProperty.FindPropertyRelative("url").stringValue = track.url;
                            }

                            Debug.Log($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] プレイリストをjsonファイルから読み込みました。");
                        }
                        catch (Exception)
                        {
                            Debug.LogError($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] プレイリストをjsonファイルから読み込みに失敗しました。");
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Options", _italicStyle);
                EditorGUILayout.PropertyField(_defaultShuffleProperty);
                EditorGUILayout.PropertyField(_defaultRepeatProperty);
                EditorGUILayout.PropertyField(_playOnAwakeProperty);
            }

            EditorGUILayout.Space();

            _tracksList.DoLayoutList();

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

            var self = _target.GetComponentInChildren<Udon.Playlist>(true);
            self.SetPublicVariable("core", core);
            self.SetPublicVariable("controller", controller);
            self.SetPublicVariable("defaultShuffle", _defaultShuffleProperty.boolValue);
            self.SetPublicVariable("defaultRepeat", _defaultRepeatProperty.boolValue);
            self.SetPublicVariable("playOnAwake", _playOnAwakeProperty.boolValue);

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
            var orderOn = self.transform.Find("Canvas/Panel/Header/Order/On").gameObject;
            self.SetPublicVariable("_orderOn", orderOn);
            self.SetPublicVariable("_orderOnButton", orderOn.transform.Find("Button").GetComponent<Button>());
            var orderOff = self.transform.Find("Canvas/Panel/Header/Order/Off").gameObject;
            self.SetPublicVariable("_orderOff", orderOff);
            self.SetPublicVariable("_orderOffButton", orderOff.transform.Find("Button").GetComponent<Button>());
            var shuffleOn = self.transform.Find("Canvas/Panel/Header/Shuffle/On").gameObject;
            self.SetPublicVariable("_shuffleOn", shuffleOn);
            self.SetPublicVariable("_shuffleOnButton", shuffleOn.transform.Find("Button").GetComponent<Button>());
            var shuffleOff = self.transform.Find("Canvas/Panel/Header/Shuffle/Off").gameObject;
            self.SetPublicVariable("_shuffleOff", shuffleOff);
            self.SetPublicVariable("_shuffleOffButton", shuffleOff.transform.Find("Button").GetComponent<Button>());
            var repeatOn = self.transform.Find("Canvas/Panel/Header/Repeat/On").gameObject;
            self.SetPublicVariable("_repeatOn", repeatOn);
            self.SetPublicVariable("_repeatOnButton", repeatOn.transform.Find("Button").GetComponent<Button>());
            var repeatOff = self.transform.Find("Canvas/Panel/Header/Repeat/Off").gameObject;
            self.SetPublicVariable("_repeatOff", repeatOff);
            self.SetPublicVariable("_repeatOffButton", repeatOff.transform.Find("Button").GetComponent<Button>());
            var content = self.transform.Find("Canvas/Panel/Scroll View/Scroll View/Viewport/Content");
            self.SetPublicVariable("_content", content);

            var template = self.transform.Find("Canvas/Panel/Scroll View/Scroll View/Viewport/Content/Template");

            for (var i = content.childCount - 1; 0 < i; i--)
            {
                var item = content.GetChild(i);
                if (item == template)
                    continue;
                DestroyImmediate(item.gameObject);
            }

            //var content_objs = Array.Empty<GameObject>();
            var content_buttons = Array.Empty<Button>();
            var content_sliders = Array.Empty<Slider>();
            var content_titles = Array.Empty<string>();
            var content_modes = Array.Empty<uint>();
            var content_urls = Array.Empty<VRCUrl>();

            for (var i = 0; i < _tracksProperty.arraySize; i++)
            {
                var track = _tracksProperty.GetArrayElementAtIndex(i);
                var title = track.FindPropertyRelative("title").stringValue;
                var mode = ((TrackMode)track.FindPropertyRelative("mode").intValue).ToVideoCoreMode();
                var url = new VRCUrl(track.FindPropertyRelative("url").stringValue);

                var obj = Instantiate(template.gameObject, content, false);
                obj.SetActive(true);
                var buttonText = obj.transform.Find("Button/Text").GetComponent<Text>();
                buttonText.text = title;
                var buttonButton = obj.transform.Find("Button").GetComponent<Button>();
                var buttonSlider = obj.transform.Find("Button/Progress").GetComponent<Slider>();
                GameObjectUtility.EnsureUniqueNameForSibling(obj);

                //ArrayUtility.Add(ref content_objs, obj);
                ArrayUtility.Add(ref content_buttons, buttonButton);
                ArrayUtility.Add(ref content_sliders, buttonSlider);
                ArrayUtility.Add(ref content_titles, title);
                ArrayUtility.Add(ref content_modes, mode);
                ArrayUtility.Add(ref content_urls, url);
            }

            self.SetPublicVariable("_content_length", _tracksProperty.arraySize);
            //self.SetPublicVariable("_content_objs", content_objs);
            self.SetPublicVariable("_content_buttons", content_buttons);
            self.SetPublicVariable("_content_sliders", content_sliders);
            self.SetPublicVariable("_content_titles", content_titles);
            self.SetPublicVariable("_content_modes", content_modes);
            self.SetPublicVariable("_content_urls", content_urls);

            var message = self.transform.Find("Canvas/Panel/Scroll View/Message").gameObject;
            self.SetPublicVariable("_message", message);
            self.SetPublicVariable("_messageText", message.transform.Find("Text").GetComponent<Text>());
        }
    }
}
