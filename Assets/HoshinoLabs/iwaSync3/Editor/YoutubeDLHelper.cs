using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    internal static class YoutubeDLHelper
    {
        internal static string ToolPath
        {
            get
            {
                var dir = Directory.GetCurrentDirectory();
                return Path.Combine(dir, "yt-dlp.exe");
            }
        }

        internal static bool Exists => File.Exists(ToolPath);

        internal delegate void DataReceivedEventHandler(DataReceivedEventArgs e);

        internal static Task Execute(string url, string[] args, DataReceivedEventHandler output, DataReceivedEventHandler error,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<int>();

            var info = new ProcessStartInfo
            {
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.GetEncoding("shift_jis"),
                StandardErrorEncoding = Encoding.GetEncoding("shift_jis"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = ToolPath,
                CreateNoWindow = true,
                Arguments = string.Join(" ",
                    $"--no-cache-dir",
                    $"--rm-cache-dir",
                    $"--no-check-certificate",
                    args == null ? string.Empty : string.Join(" ", args),
                    $"\"{url}\""
                )
            };

            var process = new Process();
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => tcs.SetResult(process.ExitCode);
            process.ErrorDataReceived += (sender, e) => error(e);
            process.OutputDataReceived += (sender, e) => output(e);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return Task.Run(() =>
            {
                while (!tcs.Task.IsCompleted)
                {
                    if (cancellationToken.IsCancellationRequested)
                        process.Kill();
                    cancellationToken.ThrowIfCancellationRequested();
                    Thread.Sleep(1);
                }
            }, cancellationToken);
        }
    }
}
