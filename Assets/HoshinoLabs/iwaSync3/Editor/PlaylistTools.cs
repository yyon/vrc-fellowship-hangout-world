using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    internal static class PlaylistTools
    {
        static Task _task = null;
        static CancellationTokenSource _cancellationTokenSource = null;

        internal static bool IsCompleted => _task == null || _task.IsCompleted;

        internal static void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            Cancel();
        }

        static bool IsValidURL(string url)
        {
            return url.StartsWith("http://www.youtube.com/", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://www.youtube.com/", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("http://youtu.be/", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://youtu.be/", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsValidVideoURL(string url)
        {
            return IsValidURL(url) && url.Contains("/watch?") && url.Contains("v=");
        }

        internal static bool IsValidPlayListURL(string url)
        {
            return IsValidURL(url) && (url.Contains("list=") || Regex.IsMatch(url, @"\/channel\/[^&\s]+") || Regex.IsMatch(url, @"\/c\/[^&\s]+") || url.EndsWith("/videos"));
        }

        static string ResolveURL(string videoId)
        {
            return $"https://www.youtube.com/watch?v={videoId}";
        }

        static string ResolveVideoID(string url)
        {
            var match = Regex.Match(url, @"v=([^&]+)");
            if (!match.Success)
                return null;
            return match.Groups[1].Value;
        }

        internal static async Task ResolveTracks(Playlist self)
        {
            EditorUtility.DisplayProgressBar(Udon.IwaSync3.APP_NAME, $"動画情報を取得中", 0f);
            Debug.LogError($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] 動画情報の取得を開始します。");
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    await (_task = ResolveTracks(self, _cancellationTokenSource.Token));
                }
                catch (OperationCanceledException)
                {

                }
            }
            Debug.Log($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] 動画情報の取得が終了しました。");
            EditorUtility.ClearProgressBar();
        }

        static async Task ResolveTracks(Playlist self,
            CancellationToken cancellationToken = default)
        {
            var serializedObject = new SerializedObject(self);
            var playlistPrefixProperty = serializedObject.FindProperty("playlistPrefix");
            var playlistExcludeShortVideoProperty = serializedObject.FindProperty("playlistExcludeShortVideo");
            var tracksProperty = serializedObject.FindProperty("tracks");

            var playlistPrefix = playlistPrefixProperty.stringValue;
            var playlistExcludeShortVideo = playlistExcludeShortVideoProperty.boolValue;

            for (var i = 0; i < tracksProperty.arraySize; i++)
            {
                var progress = (float)i / tracksProperty.arraySize;
                EditorUtility.DisplayProgressBar(Udon.IwaSync3.APP_NAME, $"動画情報を取得中 ({i + 1}/{tracksProperty.arraySize})", progress);

                var trackProperty = tracksProperty.GetArrayElementAtIndex(i);
                var url = trackProperty.FindPropertyRelative("url").stringValue;
                if (url.StartsWith(playlistPrefix))
                    url = url.Substring(playlistPrefix.Length);
                if (!IsValidVideoURL(url))
                    continue;

                var track = await ResolveTrackWithRetry(url, 3, cancellationToken);
                if (track == null)
                {
                    Debug.LogError($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] 動画情報の取得に失敗しました。");
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog(Udon.IwaSync3.APP_NAME, "動画情報の取得に失敗しました", "OK");
                    throw new OperationCanceledException();
                }

                if (playlistExcludeShortVideo
                    && track.title.EndsWith("#shorts", StringComparison.OrdinalIgnoreCase))
                {
                    trackProperty.FindPropertyRelative("title").stringValue = string.Empty;
                    trackProperty.FindPropertyRelative("url").stringValue = string.Empty;
                    continue;
                }

                //trackProperty.FindPropertyRelative("mode").intValue = (int)track.mode;
                trackProperty.FindPropertyRelative("title").stringValue = track.title;
                trackProperty.FindPropertyRelative("url").stringValue = $"{playlistPrefix}{track.url}";
            }

            if (playlistExcludeShortVideo)
            {
                for (var i = tracksProperty.arraySize - 1; 0 < i; i--)
                {
                    var trackProperty = tracksProperty.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(trackProperty.FindPropertyRelative("title").stringValue)
                        && string.IsNullOrEmpty(trackProperty.FindPropertyRelative("url").stringValue))
                        tracksProperty.DeleteArrayElementAtIndex(i);
                }
            }

            if (self)
            {
                serializedObject.ApplyModifiedProperties();
                var editor = (PlaylistEditor)Editor.CreateEditor(self);
                editor.ApplyModifiedProperties();
                GameObject.DestroyImmediate(editor);
            }
        }

        static async Task<Track> ResolveTrackWithRetry(string url, int retry,
            CancellationToken cancellationToken = default)
        {
            for(var i = 0; i < retry; i++)
            {
                var track = await ResolveTrack(url, cancellationToken);
                if (track != null)
                    return track;
            }

            return null;
        }

        static async Task<Track> ResolveTrack(string url,
            CancellationToken cancellationToken = default)
        {
            var videoId = ResolveVideoID(url);
            if (videoId == null)
                return null;

            var track = (Track)null;

            await YoutubeDLHelper.Execute(url, new[]
                {
                    $"--skip-download",
                    $"--get-title"
                }, e =>
                {
                    if (track == null)
                    {
                        track = new Track
                        {
                            url = url
                        };
                    }

                    var match = Regex.Match(e.Data, $@"^(.+)$");
                    if (match.Success)
                    {
                        var title = match.Groups[1].Value;
                        track.title = title;
                    }
                }, e =>
                {
                }, cancellationToken
            );

            return track;
        }

        internal static async Task ResolvePlaylist(Playlist self)
        {
            EditorUtility.DisplayProgressBar(Udon.IwaSync3.APP_NAME, "プレイリストを取得中", 0f);
            Debug.Log($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] プレイリストの取得を開始します。");
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    await (_task = ResolvePlaylist(self, _cancellationTokenSource.Token));
                }
                catch (OperationCanceledException)
                {

                }
            }
            Debug.Log($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] プレイリストの取得が終了しました。");
            EditorUtility.ClearProgressBar();
        }

        static async Task ResolvePlaylist(Playlist self,
            CancellationToken cancellationToken = default)
        {
            var serializedObject = new SerializedObject(self);
            var playlistUrlProperty = serializedObject.FindProperty("playlistUrl");
            var playlistPrefixProperty = serializedObject.FindProperty("playlistPrefix");
            var playlistLimitCountProperty = serializedObject.FindProperty("playlistLimitCount");
            var tracksProperty = serializedObject.FindProperty("tracks");

            var playlistUrl = playlistUrlProperty.stringValue;
            var playlistPrefix = playlistPrefixProperty.stringValue;
            var playlistLimitCount = playlistLimitCountProperty.intValue;

            var tracks = new List<Track>();

            var error = string.Empty;
            var context = SynchronizationContext.Current;
            await YoutubeDLHelper.Execute(playlistUrl, new[]
                {
                    $"--skip-download",
                    $"--max-downloads {playlistLimitCount}"
                }, e =>
                {
                    var match1 = Regex.Match(e.Data, @"^\[download] Downloading video (\d+) of (\d+)$");
                    if (match1.Success)
                    {
                        var left = int.Parse(match1.Groups[1].Value);
                        var right = int.Parse(match1.Groups[2].Value);
                        if (0 < playlistLimitCount && playlistLimitCount < right)
                            right = playlistLimitCount;
                        var progress = (float)left / right;
                        context.Post(_ => EditorUtility.DisplayProgressBar(Udon.IwaSync3.APP_NAME, $"プレイリストを取得中 ({left}/{right})", progress), null);
                    }
                    var match2 = Regex.Match(e.Data, @"^\[youtube\] ([^&\s]+): Downloading webpage$");
                    if (match2.Success)
                    {
                        var videoId = match2.Groups[1].Value;
                        var track = new Track
                        {
                            url = $"{playlistPrefix}{ResolveURL(videoId)}"
                        };
                        tracks.Add(track);
                    }
                }, e =>
                {
                    if (e.Data.StartsWith("WARNING: "))
                        return;
                    if (e.Data.StartsWith("         "))
                        return;
                    if (e.Data.StartsWith("yt_dlp"))
                        return;

                    var match1 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): This live stream recording is not available\.");
                    if (match1.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match1.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` は配信のアーカイブが視聴できないためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match2 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Private video\. Sign in if you've been granted access to this video");
                    if (match2.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match2.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` は非公開かログインが必要な動画のためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match3 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Sign in to confirm your age\. This video may be inappropriate for some users\.");
                    if (match3.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match3.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` は年齢制限が設定されているためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match4 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Video unavailable\. The uploader has not made this video available in your country");
                    if (match4.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match4.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` は非公開に設定されているためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match5 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Premieres in \d+ minutes");
                    if (match5.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match5.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` はプレミア公開待ち動画のためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match6 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Video unavailable\. This video is no longer available because the YouTube account associated with this video has been terminate.");
                    if (match6.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match6.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` は投稿者のアカウントが削除されているためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match7 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Video unavailable\. This video is no longer available due to a copyright claim by .+");
                    if (match7.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match7.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` は著作権主張のため利用できないためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match8 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Premieres in \d+ hours");
                    if (match8.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match8.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` はプレミア公開待ち動画のためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }
                    var match9 = Regex.Match(e.Data, @"^ERROR: \[youtube\] ([^&\s]+): Video unavailable\. This video contains content from .+");
                    if (match9.Success)
                    {
                        var idx = tracks.FindIndex(x => Regex.IsMatch(x.url, $@"v={match9.Groups[1].Value}"));
                        if (idx < 0)
                            return;
                        var track = tracks[idx];
                        Debug.LogWarning($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] `{track.url}` は著作権主張のため利用できないためリストへの取り込みから除外します。");
                        tracks.RemoveAt(idx);
                        return;
                    }

                    Debug.LogWarning(e.Data);
                    error += e.Data;
                }, cancellationToken
            );

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[<color=#47F1FF>{Udon.IwaSync3.APP_NAME}</color>] プレイリストの取得に失敗しました。");
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(Udon.IwaSync3.APP_NAME, "プレイリストの取得に失敗しました\nプレイリストが非公開になっていないか確認して下さい", "OK");
                throw new OperationCanceledException();
            }

            tracksProperty.arraySize = tracks.Count;
            for (var i = 0; i < tracksProperty.arraySize; i++)
            {
                var trackProperty = tracksProperty.GetArrayElementAtIndex(i);
                var track = tracks[i];
                //trackProperty.FindPropertyRelative("mode").intValue = (int)track.mode;
                //trackProperty.FindPropertyRelative("title").stringValue = track.title;
                trackProperty.FindPropertyRelative("url").stringValue = track.url;
            }

            if (self)
            {
                serializedObject.ApplyModifiedProperties();
                await ResolveTracks(self, cancellationToken);
            }
        }
    }
}
