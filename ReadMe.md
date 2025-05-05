---
languages:
- csharp
products:
- dotnet
page_type: sample
name: "AspNetPlugin Demo"
urlFragment: "aspnetplugin-demo"
description: "A sample that demonstrates creating an asp.net app with plugins"
---

# AspNetPlugin Demo

This sample demonstrates how to create an asp.net app with a plugin architecture, using the `AssemblyLoadContext` to help load plugins.


## Build and Run

1. Install .NET 9.0 or newer.
2. Use the .NET SDK to build the project via `dotnet build`.
   - The Server project does not contain any references to the plugin projects, so you need to build the solution.
3. Go to the Server directory and use `dotnet run` to run the app, OR, in the root directory of this solution, run command: dotnet watch --project Server
    - You should see the app output the "Now listening on: http://localhost:5002"
	- Input this URL in the address bar of your browser
