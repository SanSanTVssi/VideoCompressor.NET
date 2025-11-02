using VideoCompressor.Configs;

namespace VideoCompressor.Providers;

public class AppConfigProvider<TConfig> : IProvider<TConfig> where TConfig : AppConfig
{
    private TConfig? _appConfig;
    
    public TConfig Provide()
    {
        if (_appConfig != null) return _appConfig;

        var pathToConfig = Environment.GetEnvironmentVariable("CONFIG_PATH") 
                           ?? throw new InvalidOperationException("CONFIG_PATH is not defined in environment variables.");
        
        var configStr = File.ReadAllText(Path.Combine(pathToConfig, "config.json"));
        _appConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<TConfig>(configStr)
                     ?? throw new InvalidOperationException("Failed to deserialize config.");
        
        return _appConfig;
    }
}