using agent_framework_bedrock_console.Tools;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using System.ClientModel;

var builder = Host.CreateApplicationBuilder(args);

//The initialized WebApplicationBuilder (builder) provides default configuration and calls AddUserSecrets when the EnvironmentName is Development.
//"DOTNET_ENVIRONMENT": "Development"

var aiProvider = Enum.TryParse<AIProvider>(builder.Configuration["AIAPIProvider"], out var provider) ? provider : AIProvider.Bedrock;

var aiConfiguration = aiProvider switch
{
    AIProvider.Bedrock => new
    {
        ApiKey = builder.Configuration["Bedrock:ApiKey"],
        Endpoint = builder.Configuration["Bedrock:Endpoint"],
        Region = builder.Configuration["Bedrock:Region"],
        Model = builder.Configuration["Bedrock:Model"]
    },
    AIProvider.OpenAI => new
    {
        ApiKey = builder.Configuration["OpenAI:ApiKey"],
        Endpoint = builder.Configuration["OpenAI:Endpoint"],
        Region = default(string),
        Model = builder.Configuration["OpenAI:Model"]
    },
    _ => throw new NotSupportedException($"AI Provider '{aiProvider}' is not supported.")
};

if (aiProvider == AIProvider.OpenAI && string.IsNullOrEmpty(aiConfiguration.Endpoint) || string.IsNullOrEmpty(aiConfiguration.Model) || string.IsNullOrEmpty(aiConfiguration.ApiKey))
{
    Console.WriteLine("Error: Bedrock endpoint, model or API key missing. Please check the configuration or secrets.");
    return;
}

if (aiProvider == AIProvider.Bedrock && string.IsNullOrEmpty(aiConfiguration.Model) || string.IsNullOrEmpty(aiConfiguration.ApiKey))
{
    Console.WriteLine("Error: Bedrock endpoint, model or API key missing. Please check the configuration or secrets.");
    return;
}

Console.WriteLine("Hello, World!");
Console.WriteLine(
    $"""
    AI configuration: 
      Model '{aiConfiguration.Model}', 
      Endpoint '{aiConfiguration.Endpoint}',
      Region '{aiConfiguration.Region}',    
      ApiKey '{(!string.IsNullOrEmpty(aiConfiguration.ApiKey) ? "***" : "(missing)")}'
    """);

builder.Services
    .AddSingleton<LightsProvider>()
    .AddSingleton<AgentPlugin>();

RegionEndpoint? region = default; // Only for Bedrock client
if (aiProvider == AIProvider.Bedrock)
{
    Environment.SetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK", aiConfiguration.ApiKey);
    region = !string.IsNullOrEmpty(aiConfiguration.Region) 
        ? RegionEndpoint.GetBySystemName(aiConfiguration.Region)
        : RegionEndpoint.USEast1;
}

var chatClient = aiProvider switch
{
    AIProvider.Bedrock => new AmazonBedrockRuntimeClient(region)
        .AsIChatClient(aiConfiguration.Model),
    AIProvider.OpenAI => new OpenAIClient(
            new ApiKeyCredential(aiConfiguration.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(aiConfiguration.Endpoint)
            })
    .GetChatClient(aiConfiguration.Model)
    .AsIChatClient(),
    _ => throw new NotSupportedException($"AI Provider '{aiProvider}' is not supported.")
};

builder.Services.AddChatClient(chatClient);

builder.Services.AddAIAgent("assistant", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    return new ChatClientAgent(chatClient, "" +
        "You are a helpful assistant that manages lights. Answer questions concisely and accurately.",
        key,
        tools: [.. sp.GetRequiredService<AgentPlugin>().AsAITools()]);
});

var serviceProvider = builder.Services.BuildServiceProvider();

/* Create agent */
var agent = serviceProvider.GetRequiredKeyedService<AIAgent>("assistant");
var thread = await agent.CreateSessionAsync();

/* Chat loop */
string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    Console.Write("Assistant > ");
    await foreach (var update in agent.RunStreamingAsync(userInput, thread, options: new AgentRunOptions()))
    {
        Console.Write(update);
    }
    Console.WriteLine();
} while (userInput is { Length: > 0 });

enum AIProvider
{
    Bedrock,
    OpenAI,
}
