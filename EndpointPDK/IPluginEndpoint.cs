using Microsoft.AspNetCore.Http;

namespace EndpointPDK;

public interface IPluginEndpoint
{
    Task ExecuteAsync(HttpContext ctx);
}