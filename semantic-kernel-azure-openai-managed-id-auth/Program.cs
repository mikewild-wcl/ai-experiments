using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

const string DeploymentConfigKey = "AzureOpenAiSettings:DeploymentName";
const string EndpointConfigKey = "AzureOpenAiSettings:Endpoint";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var deployment = configuration.GetValue<string>(DeploymentConfigKey);
var endpoint = configuration.GetValue<string>(EndpointConfigKey);

/* 
  Using DefaultAzureCredential initially failed with error
     The principal `` lacks the required data action `Microsoft.CognitiveServices/accounts/OpenAI/deployments/chat/completions/action` to perform `POST /openai/deployments/{deployment-id}/chat/completions` operation

 The fix was to add role assignments to the user from the Access control (IAM) blade of AI Foundry | Azure OpenAI. Either add
    Azure AI Developer
 or (some or all)
    Azure AI User
    Cognitive Services User
    Cognitive Services OpenAI User

  This has instructions on adding custom permissions: https://github.com/azure-ai-foundry/foundry-samples/issues/155
*/

var credential = new DefaultAzureCredential(true);

var client = new AzureOpenAIClient(
    new Uri(endpoint), credential);

var builder = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(deployment, client);

builder.Services
    .AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

var kernel = builder.Build();

AzureOpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    Temperature = deployment.StartsWith('o') ? 1.0f : 0.8f,
};

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var history = new ChatHistory();
history.AddSystemMessage(
    """
    You are a wild and crazy dude living in San Francisco and working in tech.
    You love telling jokes and making people laugh.
    Respond to the user in a humorous way and in the voice of a tech bro.
    """);

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
        executionSettings: promptExecutionSettings,
        kernel: kernel);

    Console.WriteLine("Assistant > " + result);

    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is { Length: >0 });
