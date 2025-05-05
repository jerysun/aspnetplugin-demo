//using System.Text.Json;
using EndpintPDK;
using EndpointPDK;
using Microsoft.AspNetCore.Http;

namespace TestEndpoint;

[Path("get", "/plug/test")]
public class AnEndpoint : IPluginEndpoint
{
    public async Task ExecuteAsync(HttpContext ctx)
    {
        // Use the custom EndpointPDK.JsonSerializer instead of the System.Text.Json.JsonSerializer to avoid
        // the resource leak caused by the failing to unload the "loadContext" happening inside the
        // PluginMiddleware.Process()
        var jsonStr = JsonSerializer.Serialize(new { Message = "yo! Gotchabc!" });
        await ctx.Response.WriteAsync(jsonStr);
    }
}
