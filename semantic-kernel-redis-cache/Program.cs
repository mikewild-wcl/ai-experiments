using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenAI;
using semantic_kernel_redis_cache.Services;
using semantic_kernel_redis_cache.Services.Interfaces;
using System.ClientModel;

const string ApiKeyName = "GITHUB_MODELS_TOKEN";
const string ModelName = "OpenAiSettings:Model";
const string EndpointName = "OpenAiSettings:Endpoint";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetValue<string>(ApiKeyName);
var endpoint = configuration.GetValue<string>(EndpointName);
var model = configuration.GetValue<string>(ModelName);

var builder = Host.CreateApplicationBuilder(args);

var openAIOptions = new OpenAIClientOptions
{
    Endpoint = new Uri(endpoint!)
};
var client = new OpenAIClient(new ApiKeyCredential(apiKey!), openAIOptions);

builder.Services
    .AddTransient<IChatService, ChatService>()
    .AddTransient((serviceProvider) =>
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder
            .AddOpenAIChatCompletion(model!, client);

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

    // TODO: Check cache for response. See p350 on Gen AI book
    // References:
    // https://redis.io/blog/what-is-semantic-caching/
    // https://devblogs.microsoft.com/dotnet/redis-makes-intelligent-apps-smarter-and-consistent/
    //  -> https://github.com/CawaMS/chatappredis/tree/main
    // https://github.com/Azure-Samples/azure-redis-dalle-semantic-caching/tree/main/OutputCacheDallESample
    // https://www.nuget.org/packages/Redis.OM/
    // https://www.nuget.org/packages/NRedisStack/
    // docker run -p 6379:6379 -p 8001:8001 redis/redis-stack
    // docker run -p 6379:6379 --name redis-stack redis/redis-stack:latest


    Console.WriteLine("Response > ");
    await foreach (var responseToken in chatService.GetResponseAsync(userInput))
    {
        Console.Write(responseToken);
    }

    Console.WriteLine();

} while (userInput != "/q");
