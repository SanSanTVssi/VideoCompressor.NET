namespace VideoCompressor;

public record CompressionError(
    FileInfo InputFile,
    FileInfo OutputFile,
    string Command,
    string ErrorMessage
) : CompressionResult
{
    public override string ToString()
    {
        return
            $"❌ ERROR\n" +
            $"Input: {InputFile.FullName}\n" +
            $"Output: {OutputFile.FullName}\n" +
            $"Command: {Command}\n" +
            $"Message: {ErrorMessage}";
    }
}