using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace VideoCompressor;

public sealed partial class FfmpegWrapper(string ffmpegPath = "ffmpeg")
{
    public event Action<string>? OnOutput;
    public event Action<string>? OnError;
    public event Action<double>? OnProgress; // seconds elapsed

    static readonly Regex ProgressRegex = FfmpegProgressRegex();
    readonly string _ffmpegPath = ffmpegPath;

    public async Task<(int ExitCode, string Stdout, string Stderr)> RunAsync(IEnumerable<string> args)
    {
        // Build command with actual ffmpeg path
        var ffmpegCmd = $"{_ffmpegPath} {string.Join(' ', args)}";

        string shell;
        string shellArgs;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            shell = "cmd.exe";
            shellArgs = $"/c {ffmpegCmd}";
        }
        else
        {
            shell = "/bin/bash";
            shellArgs = $"-c \"{ffmpegCmd}\"";
        }

        var psi = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = shellArgs,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        using var process = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            stdout.AppendLine(e.Data);
            OnOutput?.Invoke(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            stderr.AppendLine(e.Data);
            OnError?.Invoke(e.Data);
            TryParseProgress(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return (process.ExitCode, stdout.ToString(), stderr.ToString());
    }

    void TryParseProgress(string line)
    {
        var match = ProgressRegex.Match(line);
        if (!match.Success)
        {
            return;
        }

        if (!int.TryParse(match.Groups[1].Value, out var hours) ||
            !int.TryParse(match.Groups[2].Value, out var minutes) ||
            !double.TryParse(match.Groups[3].Value, out var seconds))
        {
            return;
        }

        var totalSeconds = hours * 3600 + minutes * 60 + seconds;
        OnProgress?.Invoke(totalSeconds);
    }

    [GeneratedRegex(@"time=(\d+):(\d+):(\d+\.\d+)", RegexOptions.Compiled)]
    private static partial Regex FfmpegProgressRegex();

    public override string ToString()
    {
        return _ffmpegPath;
    }
}
