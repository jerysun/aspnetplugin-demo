using EndpointPDK;

namespace TestEndpoint;

public class PluginAnEndpoint : PluginMiddleware
{
    private const string PluginAssemblyRelativePath = @"TestEndpoint\bin\Debug\net9.0\TestEndpoint.dll";
    
    protected override PluginParams GetPluginParams()
    {
        var fullPath = GetAssemblyFullPath(PluginAssemblyRelativePath);
        return new PluginParams(
            fullPath,
            "TestEndpoint.AnEndpoint" 
        );
    }
}