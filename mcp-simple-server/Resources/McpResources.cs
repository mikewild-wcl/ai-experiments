using ModelContextProtocol.Server;
using System.ComponentModel;

namespace mcp_simple_server.Resources;

[McpServerResourceType] 
internal class McpResources
{
    [McpServerResource(Name ="History", MimeType = "text")] 
    [Description("Get the history of the project")]
    public string GetHistory() =>
        $"The history of the project is very short. That is all.";
}
