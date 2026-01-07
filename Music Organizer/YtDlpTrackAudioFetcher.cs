using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Music_Organizer
{
    public interface ITrackAudioFetcher
    {
        Task<string> DownloadAudioAsync(string artist, string title, CancellationToken token);
    }

    public sealed class YtDlpTrackAudioFetcher : ITrackAudioFetcher
    {
        public async Task<string> DownloadAudioAsync(string artist, string title, CancellationToken token)
        {
            var query = artist + " - " + title + " audio";
            var safeBase = MakeSafeFileName(artist + " - " + title);
            var folder = AppPaths.TempMusic;


            var outputTemplate = Path.Combine(folder, safeBase + ".%(ext)s");

            var args =
                "-f bestaudio/best " +
                "--no-playlist " +
                "--no-part " +
                "--print after_move:filepath " +
                "-o " + Quote(outputTemplate) + " " +
                Quote("ytsearch1:" + query);

            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p == null)
                throw new InvalidOperationException("Failed to start yt-dlp.");

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            p.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    stdout.AppendLine(e.Data);
            };

            p.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    stderr.AppendLine(e.Data);
            };

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            await WaitForExitAsync(p, token);

            if (token.IsCancellationRequested)
                throw new OperationCanceledException(token);

            if (p.ExitCode != 0)
                throw new InvalidOperationException("yt-dlp failed: " + stderr.ToString());

            var lines = stdout
                .ToString()
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var finalPath = lines.LastOrDefault();
            if (string.IsNullOrWhiteSpace(finalPath))
                throw new InvalidOperationException("yt-dlp did not print a filepath.");

            return finalPath.Trim();
        }

        private static async Task WaitForExitAsync(Process p, CancellationToken token)
        {
            while (!p.HasExited)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(50, token);
            }
        }

        private static string Quote(string s) => "\"" + s.Replace("\"", "\\\"") + "\"";

        private static string MakeSafeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name.Trim();
        }
    }
}
