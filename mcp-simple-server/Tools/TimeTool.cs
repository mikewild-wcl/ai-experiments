using ModelContextProtocol.Server;
using System.ComponentModel;

namespace mcp_simple_server.Tools;

internal class TimeTool
{
    [McpServerTool] 
    [Description("Get the current time for a city")]
    public string GetCurrentTime(string city) =>
        $"It is {DateTime.Now.Hour}:{DateTime.Now.Minute} in {city}.";
}
