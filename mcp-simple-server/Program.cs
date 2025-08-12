using mcp_simple_server.Prompts;
using mcp_simple_server.Resources;
using mcp_simple_server.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<TimeTool>()
    .WithResources<McpResources>()
    .WithPrompts<McpPrompts>()
    .WithTools<RandomNumberTools>();

await builder.Build().RunAsync();
