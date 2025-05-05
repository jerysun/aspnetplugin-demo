using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;

namespace EndpointPDK;

public abstract class PluginMiddleware : IMiddleware
{
    protected abstract PluginParams GetPluginParams();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var alcRef = await Process(context, next);

        if (!context.Response.HasStarted)
        {
            Console.WriteLine("Before: await next(context);");
            await next(context);
            Console.WriteLine("After: await next(context);");
        }

        for (var i = 0; i < 10 && alcRef.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine($"Unloading Attempts: ${i}");
        }
        Console.WriteLine($"Unloading Successful: {!alcRef.IsAlive}");

    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<WeakReference> Process(HttpContext context, RequestDelegate next)
    {
        var pluginParams = GetPluginParams();
        var loadContext = new AssemblyLoadContext(pluginParams.Path, isCollectible: true);

        try
        {
            var assembly = loadContext.LoadFromAssemblyPath(pluginParams.Path);

            var endpointType = assembly.GetType(pluginParams.PluginTypeName);
            var pathInfo = endpointType?.GetCustomAttribute<PathAttribute>();


            if (pathInfo != null
                && pathInfo.Method.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase)
                && pathInfo.Path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase))
            {
                var endpoint = Activator.CreateInstance(endpointType) as IPluginEndpoint;
                await endpoint.ExecuteAsync(context);
            }

        }
        finally
        {
            loadContext.Unload();
            await next.Invoke(context);
        }

        return new WeakReference(loadContext);
    }

    protected virtual string GetAssemblyFullPath(string relativePath)
    {
        // Navigate up to the solution root
        var root = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(typeof(PluginMiddleware).Assembly.Location)))))));

        return Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
    }
}