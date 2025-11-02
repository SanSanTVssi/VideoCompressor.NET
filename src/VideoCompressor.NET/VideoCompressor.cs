using VideoCompressor.Configs;

namespace VideoCompressor;

public class VideoCompressor
{
    CompressionConfig _config = new();
    readonly FfmpegWrapper _ffmpeg;

    public event Action<double>? OnProgress;
    public event Action<string>? OnLog;

    public VideoCompressor(string ffmpegPath = "ffmpeg")
    {
        _ffmpeg = new FfmpegWrapper(ffmpegPath);
        _ffmpeg.OnProgress += p => OnProgress?.Invoke(p);
        _ffmpeg.OnOutput += line => OnLog?.Invoke(line);
        _ffmpeg.OnError += line => OnLog?.Invoke(line);
    }

    public static VideoCompressor FromAppConfig(AppConfig appConfig)
    {
        var config = new CompressionConfig(
            Codec: appConfig.DefaultCodec,
            Crf: appConfig.DefaultCrf,
            Preset: appConfig.DefaultPreset,
            Fps: appConfig.DefaultFps,
            Scale: appConfig.DefaultScale,
            Audio: appConfig.DefaultAudioCodec,
            AudioBitrate: appConfig.DefaultAudioBitrate,
            Ext: appConfig.DefaultExt
        );

        var compressor = new VideoCompressor(appConfig.FfmpegPath)
            .WithConfig(config);

        return compressor;
    }

    public VideoCompressor WithConfig(CompressionConfig config)
    {
        _config = config;
        return this;
    }

    public async Task<CompressionSuccess> CompressAsync(string inputPath, string? outputPath = null)
    {
        var input = new FileInfo(inputPath);
        var output = new FileInfo(outputPath ?? Path.ChangeExtension(input.FullName, _config.Ext));
        var args = BuildArgs(input, output);

        var (exitCode, stdout, stderr) = await _ffmpeg.RunAsync(args);

        if (exitCode == 0)
        {
            var inputSize = input.Exists ? input.Length : 0;
            var outputSize = output.Exists ? output.Length : 0;
            var reduction = inputSize > 0 ? ((double)(inputSize - outputSize) / inputSize) * 100 : 0;

            return new CompressionSuccess(
                input,
                output,
                $"{_ffmpeg} {string.Join(' ', args)}",
                exitCode,
                inputSize,
                outputSize,
                reduction
            );
        }

        var error = new CompressionError(
            input,
            output,
            $"{_ffmpeg} {string.Join(' ', args)}",
            stderr
        );

        throw new CompressionErrorException(error);
    }

    string[] BuildArgs(FileInfo input, FileInfo output)
    {
        static string Quote(string path) =>
            path.Contains(' ') && !path.StartsWith('"') ? $"\"{path}\"" : path;

        var vf = new[]
        {
            _config.Fps.HasValue ? $"fps={_config.Fps}" : null,
            _config.Scale.HasValue ? $"scale=-2:{_config.Scale}" : null
        }.Where(v => v is not null).ToArray();

        var vfArg = vf.Any() ? "-vf " + string.Join(',', vf) : string.Empty;

        var audioArgs = _config.Audio switch
        {
            "aac"  => new[] { "-c:a", "aac", "-b:a", _config.AudioBitrate },
            "opus" => new[] { "-c:a", "libopus", "-b:a", _config.AudioBitrate },
            "copy" => new[] { "-c:a", "copy" },
            "none" => new[] { "-an" },
            _      => new[] { "-c:a", "aac", "-b:a", "128k" }
        };

        return new[]
            {
                "-y",
                "-i", Quote(input.FullName),
                "-c:v", _config.Codec,
                "-crf", _config.Crf.ToString(),
                "-preset", _config.Preset,
                vfArg,
                "-pix_fmt", "yuv420p"
            }
            .Concat(audioArgs)
            .Append(Quote(output.FullName))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

}
