namespace EndpointPDK;

public class PluginParams(string pluginPath, string pluginsPluginType)
{
    public string Path { get; set; } = pluginPath;
    public string PluginTypeName { get; set; } = pluginsPluginType;
}