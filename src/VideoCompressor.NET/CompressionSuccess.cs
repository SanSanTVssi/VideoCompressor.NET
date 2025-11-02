namespace VideoCompressor;

public record CompressionSuccess(
    FileInfo InputFile,
    FileInfo OutputFile,
    string Command,
    int ReturnCode,
    long InputSize,
    long OutputSize,
    double ReductionPercent
) : CompressionResult
{
    public override string ToString()
    {
        return
            $"✅ SUCCESS\n" +
            $"Input:  {InputFile.FullName} ({HumanReadableSize(InputSize)})\n" +
            $"Output: {OutputFile.FullName} ({HumanReadableSize(OutputSize)})\n" +
            $"Reduced: {ReductionPercent:F2}%\n" +
            $"Command: {Command}\n" +
            $"Exit Code: {ReturnCode}";
    }

    static string HumanReadableSize(long bytes)
    {
        if (bytes == 0)
        {
            return "0 B";
        }

        var units = new[] { "B", "KiB", "MiB", "GiB", "TiB" };
        var size = (double)bytes;
        var i = 0;

        while (size >= 1024 && i < units.Length - 1)
        {
            size /= 1024.0;
            i++;
        }

        return $"{size:F2} {units[i]}";
    }
}