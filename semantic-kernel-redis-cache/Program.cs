using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using semantic_kernel_redis_cache.Services;
using semantic_kernel_redis_cache.Services.Interfaces;

const string ApiKeyName = "GITHUB_MODELS_TOKEN";
const string DeploymentName = "AzureOpenAiSettings:DeploymentName";
const string EndpointName = "AzureOpenAiSettings:Endpoint";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetValue<string>(ApiKeyName);
var endpoint = configuration.GetValue<string>(EndpointName);
var deployment = configuration.GetValue<string>(DeploymentName);

var builder = Host.CreateApplicationBuilder(args);

// TODO: Investigate why this gets 401 not authorized error
//       See https://github.com/orgs/community/discussions/158638
var client = new AzureOpenAIClient(
    new Uri(endpoint), 
    new AzureKeyCredential(apiKey), new AzureOpenAIClientOptions());

builder.Services
    .AddTransient<IChatService, ChatService>()
    .AddTransient((serviceProvider) =>
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder
            .AddAzureOpenAIChatCompletion(deployment!, client);

        kernelBuilder.Services
            .AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        return kernelBuilder.Build();
    });

var host = builder.Build();

var chatService = host.Services.GetRequiredService<IChatService>();

string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();
    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    Console.WriteLine("Response > ");
    await foreach (var responseToken in chatService.GetResponseAsync(userInput))
    {
        Console.Write(responseToken);
    }

    Console.WriteLine();

} while (userInput != "/q");
