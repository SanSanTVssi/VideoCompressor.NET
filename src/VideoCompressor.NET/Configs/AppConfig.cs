using System.Reflection;

namespace VideoCompressor.Configs;


public record AppConfig
{
    public string WorkingDir { get; init; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    public string InputDirName { get; init; } = "input";
    public string OutputDirName { get; init; } = "output";

    public string FfmpegPath { get; init; } = "ffmpeg"; // can be a custom path
    public int DefaultCrf { get; init; } = 28;
    public string DefaultCodec { get; init; } = "libx265";
    public string DefaultPreset { get; init; } = "slow";
    public int DefaultFps { get; init; } = 30;
    public int? DefaultScale { get; init; } = null;
    public string DefaultAudioCodec { get; init; } = "aac";
    public string DefaultAudioBitrate { get; init; } = "128k";
    public string DefaultExt { get; init; } = ".mp4";

    public Dictionary<string, string> CustomPresets { get; init; } = new();

    public string InputDirPath => Path.Combine(WorkingDir, InputDirName);
    public string OutputDirPath => Path.Combine(WorkingDir, OutputDirName);
}