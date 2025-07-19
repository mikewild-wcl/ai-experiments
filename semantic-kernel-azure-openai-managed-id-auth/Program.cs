using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Numerics;
using static System.Net.WebRequestMethods;

const string DeploymentConfigKey = "DeploymentName";
const string EndpointConfigKey = "Endpoint";
const string ModelIdConfigKey = "ModelId";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var deploment = configuration.GetValue<string>(DeploymentConfigKey);
var endpoint = configuration.GetValue<string>(EndpointConfigKey);
var modelId = configuration.GetValue<string>(ModelIdConfigKey);

/*
// https://learn.microsoft.com/en-us/dotnet/ai/azure-ai-services-authentication
// https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/local-development-dev-accounts?toc=%2Fdotnet%2Fai%2Ftoc.json&bc=%2Fdotnet%2Fai%2Ftoc.json&tabs=azure-portal%2Csign-in-visual-studio%2Ccommand-line#implement-the-code
var credential = new DefaultAzureCredential
var client = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(configuration.GetValue<string>("AzureOpenAI:ApiKey")));

var builder = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(modelId, client);


builder.Services
    .AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));


var kernel = builder.Build();

AzureOpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    Temperature = 0.8f,
};

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var history = new ChatHistory();

history.AddSystemMessage(
    """
    You are a wild and crazy dude who loves to tell jokes and make people laugh.
    """);

string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: promptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
*/