using agent_framework_console;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

const string ApiKeyName = "GITHUB_MODELS_TOKEN";
const string ModelsEndpoint = "https://models.github.ai/inference";

var apiKey = Environment.GetEnvironmentVariable(ApiKeyName);
var model = "openai/gpt-4.1";

//using var httpClient = new HttpClient(new ConsoleWriterHttpClientHandler());
using var httpClient = new HttpClient();

var openAIOptions = new OpenAIClientOptions
{
    Endpoint = new Uri(ModelsEndpoint),
    Transport = new HttpClientPipelineTransport(httpClient)
};

// Add logging
//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Create a service collection to hold the agent plugin and its dependencies.
ServiceCollection services = new();
services.AddSingleton<LightsProvider>();
services.AddSingleton<AgentPlugin>();
var serviceProvider = services.BuildServiceProvider();

var client = new OpenAIClient(new ApiKeyCredential(apiKey), openAIOptions);
var agent = client
    .GetChatClient(model)
    .AsIChatClient()
    .CreateAIAgent(
        instructions: "You are a helpful assistant that manages lights.",
        name: "Assistant",
        tools: [.. serviceProvider.GetRequiredService<AgentPlugin>().AsAITools()],
        services: serviceProvider);

var thread = agent.GetNewThread();

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
    await foreach (var update in agent.RunStreamingAsync(userInput, thread))
    {
        Console.Write(update);
    }
    Console.WriteLine();
} while (userInput is { Length: > 0 });