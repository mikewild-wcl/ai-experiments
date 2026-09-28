using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;
using System.ClientModel;
using System.ComponentModel;
using OpenAI.Responses;

if (args.Length > 0 && args[0] == "--server")
{
    // Run as server mode
    await RunMcpServerAsync();
    return;
}

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var openAiEndpoint = configuration["AZURE_OPENAI_ENDPOINT"]
                     ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                     ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

var deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
                     ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
                     ?? "gpt-5.4-nano";

var apiKey = configuration["AZURE_OPENAI_API_KEY"]
             ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("API Key not set. Will default to using an Azure managed or CLI credential");
    apiKey = null;
}

// --- MCP client + skill discovery ---
// Launch this same assembly as a stdio MCP server in a child process.
var thisAssemblyPath = typeof(Program).Assembly.Location;
Console.WriteLine("Discovering MCP-based skills");

await using McpClient client = await McpClient.CreateAsync(
    new StdioClientTransport(new()
    {
        Name = "skills-server",
        Command = "dotnet",
        Arguments = [thisAssemblyPath, "--server"],
    }));

var skillsProvider = new AgentSkillsProviderBuilder()
    .UseMcpSkills(client)
    .Build();

//AuthenticationTokenProvider credential = new DefaultAzureCredential();
//AuthenticationTokenProvider credential2 = new ApiKeyCredential(apiKey);

// --- Agent ---
AIAgent agent;

if (apiKey is not null)
{
    // WARNING: DefaultAzureCredential is convenient for development but requires careful consideration in production.
    // In production, consider using a specific credential (e.g., ManagedIdentityCredential) to avoid
    // latency issues, unintended credential probing, and potential security risks from fallback mechanisms.
    //var client = CreateOpenAiClient(openAiEndpoint, apiKey);
    var agentClient = CreateAgentClient(openAiEndpoint, deploymentName, skillsProvider, apiKey);

    agent = agentClient
        .AsBuilder()
        .UseToolApproval(new ToolApprovalAgentOptions
        {
            // NOTE: Auto-approving all skill tools is done here for simplicity in
            // this demonstration. In production, you should prompt the user before
            // allowing skill tools to execute. See Agent_Step07_SkillsAutoApproval
            // for a walkthrough of the full approval flow.
            AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
        })
        .Build();
}
else
{
    agent = new AIProjectClient(new Uri(openAiEndpoint), new DefaultAzureCredential())
        .AsAIAgent(new ChatClientAgentOptions
        {
            Name = "SkillsAgent",
            ChatOptions = new()
            {
                ModelId = deploymentName,
                Instructions = "You are a helpful assistant. Use available skills to answer the user.",
            },
            AIContextProviders = [skillsProvider],
        })
        .AsBuilder()
        .UseToolApproval(new ToolApprovalAgentOptions
        {
            // NOTE: Auto-approving all skill tools is done here for simplicity in
            // this demonstration. In production, you should prompt the user before
            // allowing skill tools to execute. See Agent_Step07_SkillsAutoApproval
            // for a walkthrough of the full approval flow.
            AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
        })
        .Build();
}

// --- Run ---
Console.WriteLine(new string('-', 60));

try
{
    AgentResponse response = await agent.RunAsync(
        "How many kilometers is a marathon (26.2 miles)? And how many pounds is 75 kilograms?");

    Console.WriteLine($"Agent: {response.Text}");
}
catch (ClientResultException ex)
{
    Console.Error.WriteLine("=== ClientResultException ===");
    Console.Error.WriteLine($"Type: {ex.GetType().FullName}");
    Console.Error.WriteLine($"Status: {ex.Status}");
    Console.Error.WriteLine($"Message: {ex.Message}");
    Console.Error.WriteLine($"Inner exception: {ex.InnerException?.ToString() ?? "<none>"}");
    Console.Error.WriteLine($"Stack trace:{Environment.NewLine}{ex.StackTrace}");

    var rawResponse = ex.GetRawResponse();

    if (rawResponse is not null)
    {
        Console.Error.WriteLine($"Raw response status: {rawResponse.Status}");
        Console.Error.WriteLine($"Raw response reason: {rawResponse.ReasonPhrase}");

        Console.Error.WriteLine("Response headers:");
        foreach (var header in rawResponse.Headers)
        {
            Console.Error.WriteLine($"{header.Key}: {header.Value}");
        }

        try
        {
            rawResponse.BufferContent();

            if (rawResponse.ContentStream is not null)
            {
                rawResponse.ContentStream.Position = 0;

                using var reader = new StreamReader(
                    rawResponse.ContentStream,
                    leaveOpen: true);

                Console.Error.WriteLine("Response body:");
                Console.Error.WriteLine(reader.ReadToEnd());
            }
            else
            {
                Console.Error.WriteLine("Response body: <empty>");
            }
        }
        catch (Exception responseReadException)
        {
            Console.Error.WriteLine(
                $"Could not read raw response body: {responseReadException}");
        }
    }
    else
    {
        Console.Error.WriteLine("No raw response was attached.");
        Console.Error.WriteLine($"Runtime type: {ex.GetType().AssemblyQualifiedName}");
    }

    throw;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.ToString());
    throw;
}


static AIAgent CreateAgentClient(string openAiEndpoint, string deploymentName, AgentSkillsProvider skillsProvider, string? apiKey = null)
{
    const string agentName = "SkillsAgent";
    const string agentInstructions = "You are a helpful assistant. Use available skills to answer the user.";

    if (apiKey is not null)
    {
        return new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(apiKey))
            .GetResponsesClient()
            .AsAIAgent(new ChatClientAgentOptions
            {
                Name = agentName,
                ChatOptions = new()
                {
                    ModelId = deploymentName,
                    Instructions = agentInstructions,
                },
                AIContextProviders = [skillsProvider],
            });
    }
    else
    {
        return new AIProjectClient(
                new Uri(openAiEndpoint),
                //new DefaultAzureCredential()
                new AzureCliCredential())
            .AsAIAgent(new ChatClientAgentOptions
            {
                Name = agentName,
                ChatOptions = new()
                {
                    ModelId = deploymentName,
                    Instructions = agentInstructions,
                },
                AIContextProviders = [skillsProvider],
            });
    }
}

static async Task RunMcpServerAsync()
{
    var builder = Host.CreateApplicationBuilder();

    // Critical for stdio transport: any provider that writes to stdout will corrupt the
    // JSON-RPC channel. Clear all providers; the MCP SDK routes its own diagnostics
    // appropriately.
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

    builder.Services.AddMcpServer(o => o.ServerInfo = new() { Name = "SkillsServer", Version = "1.0.0" })
    .WithStdioServerTransport()
    .WithResources<SkillResources>();

    await builder.Build().RunAsync();
}

#pragma warning disable CA1812 // Discovered by MCP SDK via [McpServerResourceType] attribute
[McpServerResourceType]
internal sealed class SkillResources
#pragma warning restore CA1812
{
    private const string IndexJson = """
        {
            "$schema": "https://schemas.agentskills.io/discovery/0.2.0/schema.json",
            "skills": [
                {
                    "name": "unit-converter",
                    "type": "skill-md",
                    "description": "Convert between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms.",
                    "url": "skill://unit-converter/SKILL.md"
                }
            ]
        }
        """;

    private const string SkillMd = """
        ---
        name: unit-converter
        description: Convert between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms.
        ---

        ## Usage

        When the user requests a unit conversion, use these factors:

        | From        | To          | Factor   |
        |-------------|-------------|----------|
        | miles       | kilometers  | 1.60934  |
        | kilometers  | miles       | 0.621371 |
        | pounds      | kilograms   | 0.453592 |
        | kilograms   | pounds      | 2.20462  |

        Formula: result = value × factor
        """;

    [McpServerResource(UriTemplate = "skill://index.json", Name = "Skill Index", MimeType = "application/json")]
    [Description("SEP-2640 skill discovery index")]
    public static string GetIndex() => IndexJson;

    [McpServerResource(UriTemplate = "skill://unit-converter/SKILL.md", Name = "Unit Converter Skill", MimeType = "text/markdown")]
    [Description("Unit converter skill instructions")]
    public static string GetSkillMd() => SkillMd;
}