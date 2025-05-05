using EndpointPDK;
using TestEndpoint;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Register the PluginMiddleware as a service
builder.Services.AddTransient<PluginAnEndpoint>();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<PluginAnEndpoint>();

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.Run();
