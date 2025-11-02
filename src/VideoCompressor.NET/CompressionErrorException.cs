namespace VideoCompressor;

public class CompressionErrorException(CompressionError error) : Exception(error.ErrorMessage)
{
    public CompressionError Error { get; } = error;
}