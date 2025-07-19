using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenAI;
using semantic_kernel_redis_cache.Services;
using semantic_kernel_redis_cache.Services.Interfaces;
using System.ClientModel;

const string ApiKeyName = "GITHUB_TOKEN";
const string ModelIdName = "OpenAiSettings:ModelId";
const string EndpointName = "OpenAiSettings:Endpoint";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetValue<string>(ApiKeyName);
var endpoint = configuration.GetValue<string>(EndpointName);
var modelId = configuration.GetValue<string>(ModelIdName);

var builder = Host.CreateApplicationBuilder(args);

var client = new OpenAIClient(
    new ApiKeyCredential(apiKey!),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(endpoint!)
    });

builder.Services.AddTransient<IChatService, ChatService>();

builder.Services.AddTransient((serviceProvider) =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder
        .AddOpenAIChatCompletion(modelId!, client);

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
    if (string.IsNullOrWhiteSpace(userInput))
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
