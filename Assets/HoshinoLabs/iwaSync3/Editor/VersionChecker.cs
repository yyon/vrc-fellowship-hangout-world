using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Version = System.Version;

namespace HoshinoLabs.IwaSync3
{
    internal static class VersionChecker
    {
        internal static bool VersionChecked { get; private set; }
        static Version _currentVersion;
        internal static Version CurrentVersion
        {
            get
            {
                if (_currentVersion == null)
                {
                    var match = Regex.Match(Udon.IwaSync3.APP_VERSION, $@"^v(\d+).(\d+).(\d+)", RegexOptions.IgnoreCase);
                    var major = int.Parse(match.Groups[1].Value);
                    var minor = int.Parse(match.Groups[2].Value);
                    var build = int.Parse(match.Groups[3].Value);
                    _currentVersion = new Version(major, minor, build, 0);
                }
                return _currentVersion;
            }
        }
        internal static Version LatestVersion { get; private set; }

        [Serializable]
        internal struct GitReleasesResponse
        {
            [SerializeField]
            internal string tag_name;
        }

        [MenuItem("HoshinoLabs/iwaSync3/Check Version")]
        static void ForceCheckVersion()
        {
            VersionChecked = false;
            CheckVersion();
        }

        [InitializeOnLoadMethod]
        static async void CheckVersion()
        {
            try
            {
                if (VersionChecked)
                    return;

                LatestVersion = null;

                var url = "https://raw.githubusercontent.com/hoshinolabs-vrchat/iwaSync3-Public/main/releases.txt";
                var tcs = new TaskCompletionSource<UnityWebRequest>();
                var request = UnityWebRequest.Get(url);
                var async = request.SendWebRequest();
                async.completed += _ => tcs.SetResult(async.webRequest);
                var response = await tcs.Task;

                VersionChecked = true;

                if (response.isHttpError || response.isNetworkError)
                    return;

                var releases = JsonUtility.FromJson<GitReleasesResponse>(response.downloadHandler.text);

                var match = Regex.Match(releases.tag_name, $@"^v(\d+).(\d+).(\d+)", RegexOptions.IgnoreCase);
                if (!match.Success)
                    return;

                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);
                var build = int.Parse(match.Groups[3].Value);
                LatestVersion = new Version(major, minor, build, 0);
            }
            catch (Exception)
            {

            }
        }
    }
}
