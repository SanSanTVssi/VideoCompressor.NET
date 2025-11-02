namespace VideoCompressor.Providers;

public interface IProvider<out T>
{
    T Provide();
}