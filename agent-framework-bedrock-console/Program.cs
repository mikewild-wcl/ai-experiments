using agent_framework_bedrock_console.Tools;
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

var bedrockConfiguration = new
{
    ApiKey = builder.Configuration["Bedrock:ApiKey"],
    Endpoint = builder.Configuration["Bedrock:Endpoint"],
    Model = builder.Configuration["Bedrock:Model"]
};

if (string.IsNullOrEmpty(bedrockConfiguration.Endpoint) || string.IsNullOrEmpty(bedrockConfiguration.Model) || string.IsNullOrEmpty(bedrockConfiguration.ApiKey))
{
    Console.WriteLine("Error: Bedrock endpoint, model or API key missing. Please check the configuration or secrets.");
    return;
}

Console.WriteLine("Hello, World!");
Console.WriteLine(
    $"""
    Bedrock configuration: 
      Model '{bedrockConfiguration.Model}', 
      Endpoint '{bedrockConfiguration.Endpoint}', 
      ApiKey '{(!string.IsNullOrEmpty(bedrockConfiguration.ApiKey) ? "***" : "(missing)")}'
    """);

builder.Services
    .AddSingleton<LightsProvider>()
    .AddSingleton<AgentPlugin>();

var client = new OpenAIClient(
    new ApiKeyCredential(bedrockConfiguration.ApiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(bedrockConfiguration.Endpoint)
    });

var chatClient = client
    .GetChatClient(bedrockConfiguration.Model)
    .AsIChatClient();

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
var thread = agent.GetNewThread();

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
    await foreach (var update in agent.RunStreamingAsync(userInput, thread, options: new AgentRunOptions())
    {
        Console.Write(update);
    }
    Console.WriteLine();
} while (userInput is { Length: > 0 });