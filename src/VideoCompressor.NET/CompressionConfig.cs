namespace VideoCompressor;

public record CompressionConfig(
    string Codec = "libx265",
    int Crf = 28,
    string Preset = "slow",
    int? Fps = 30,
    int? Scale = null,
    string Audio = "aac",
    string AudioBitrate = "128k",
    string Ext = ".mp4"
);