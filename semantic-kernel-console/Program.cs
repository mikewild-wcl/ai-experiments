using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using semantic_kernel_console;
using System.ClientModel;

const string ApiKeyName = "GITHUB_MODELS_TOKEN";

var apiKey = Environment.GetEnvironmentVariable(ApiKeyName);
var model = "openai/gpt-4.1";
var openAIOptions = new OpenAIClientOptions
{
    Endpoint = new Uri("https://models.github.ai/inference")
};

var client = new OpenAIClient(new ApiKeyCredential(apiKey), openAIOptions);

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(model, client);

// Add enterprise components
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add a plugin (the LightsPlugin class is defined below)
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var history = new ChatHistory();

string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    history.AddUserMessage(userInput);

    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    Console.WriteLine("Assistant > " + result);

    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is { Length: > 0 });