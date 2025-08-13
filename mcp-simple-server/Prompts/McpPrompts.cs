using ModelContextProtocol.Server;
using System.ComponentModel;

namespace mcp_simple_server.Prompts;

[McpServerPromptType] 
internal class McpPrompts
{
    [McpServerPrompt(Name ="HistoryPrompt", Title = "History prompt")] 
    [Description("Get a prompt about the project history")]
    public string GetHistory() =>
        $"Tell me all there is to know about the history of the project.";
}
