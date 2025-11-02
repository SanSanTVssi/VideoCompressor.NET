using VideoCompressor;
using VideoCompressor.Configs;
using VideoCompressor.Providers;

var configProvider = new AppConfigProvider<AppConfig>();
var appConfig = configProvider.Provide();

var inputDir = appConfig.InputDirPath;
var outputDir = appConfig.OutputDirPath;

if (!Directory.Exists(inputDir))
{
    Console.WriteLine($"Input directory not found: {inputDir}");
    return;
}

Directory.CreateDirectory(outputDir);

var inputFiles = Directory
    .EnumerateFiles(inputDir)
    .Where(f => f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
             || f.EndsWith(".mov", StringComparison.OrdinalIgnoreCase)
             || f.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)
             || f.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
    .ToList();

if (inputFiles.Count == 0)
{
    Console.WriteLine("No video files found in input directory.");
    return;
}

var compressor = VideoCompressor.VideoCompressor.FromAppConfig(appConfig);

compressor.OnLog += line =>
{
    if (!string.IsNullOrWhiteSpace(line))
    {
        Console.WriteLine(line);
    }
};

compressor.OnProgress += time =>
{
    Console.WriteLine($"Progress: {time:F1}s");
};

Console.WriteLine($"Processing {inputFiles.Count} file(s)...\n");

var totalStart = DateTime.Now;
long totalInputSize = 0;
long totalOutputSize = 0;

foreach (var inputPath in inputFiles)
{
    var inputFile = new FileInfo(inputPath);
    var name = Path.GetFileNameWithoutExtension(inputFile.Name);
    var ext = Path.GetExtension(inputFile.Name);
    var outputName = $"{name}_compressed{ext}";
    var outputPath = Path.Combine(outputDir, outputName);

    try
    {
        Console.WriteLine($"▶ Compressing: {inputFile.Name}");

        var start = DateTime.Now;
        var result = await compressor.CompressAsync(inputPath, outputPath);
        var duration = DateTime.Now - start;

        totalInputSize += result.InputSize;
        totalOutputSize += result.OutputSize;

        Console.WriteLine(result);
        Console.WriteLine($"⏱️ Duration: {duration:mm\\:ss}");
        Console.WriteLine(new string('-', 80));
    }
    catch (CompressionErrorException ex)
    {
        Console.WriteLine($"❌ Failed: {inputFile.Name}");
        Console.WriteLine(ex.Error);
        Console.WriteLine(new string('-', 80));
    }
}

var totalTime = DateTime.Now - totalStart;
var reductionPercent = totalInputSize > 0
    ? (double)(totalInputSize - totalOutputSize) / totalInputSize * 100
    : 0.0;

Console.WriteLine();
Console.WriteLine("========= FINAL SUMMARY =========");
Console.WriteLine($"🕒 Total time: {totalTime:mm\\:ss}");
Console.WriteLine($"📦 Total input size:  {HumanReadableSize(totalInputSize)}");
Console.WriteLine($"📦 Total output size: {HumanReadableSize(totalOutputSize)}");
Console.WriteLine($"📉 Overall reduction: {reductionPercent:F2}%");
Console.WriteLine("=================================\n");
Console.WriteLine("✅ All done!");


static string HumanReadableSize(long bytes)
{
    if (bytes == 0)
    {
        return "0 B";
    }

    var units = new[] { "B", "KiB", "MiB", "GiB", "TiB" };
    double size = bytes;
    var i = 0;

    while (size >= 1024 && i < units.Length - 1)
    {
        size /= 1024.0;
        i++;
    }

    return $"{size:F2} {units[i]}";
}
