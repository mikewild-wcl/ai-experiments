using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenAI;
using semantic_kernel_redis_cache.Services;
using semantic_kernel_redis_cache.Services.Interfaces;
using StackExchange.Redis;
using System.ClientModel;

const string ApiKeyName = "GITHUB_MODELS_TOKEN";
const string ModelName = "OpenAiSettings:Model";
const string EmbeddingModelName = "OpenAiSettings:EmbeddingModel";
const string EndpointName = "OpenAiSettings:Endpoint";
const string RedisConnectionStringName = "Redis";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetValue<string>(ApiKeyName);
var endpoint = configuration.GetValue<string>(EndpointName);
var model = configuration.GetValue<string>(ModelName);
var embeddingModel = configuration.GetValue<string>(EmbeddingModelName);

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

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        kernelBuilder
            .AddOpenAIChatCompletion(model!, client)
            .AddOpenAIEmbeddingGenerator(embeddingModel!, client)
            .Services
                .AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        return kernelBuilder.Build();
    });

var redisConnectionString = configuration.GetConnectionString(RedisConnectionStringName);

builder.Services
    .AddSingleton<ISemanticCacheService, SemanticCacheService>()
    .AddSingleton<IConnectionMultiplexer>(
        await ConnectionMultiplexer.ConnectAsync(redisConnectionString!));

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
    // https://github.com/CawaMS/chatappredis/blob/main/ChatApp/chatapp.csproj
    // https://www.nuget.org/packages/Redis.OM/
    // https://www.nuget.org/packages/NRedisStack/
    // https://learn.microsoft.com/en-us/azure/redis/tutorial-semantic-cache
    // https://medium.com/@yashpaddalwar/implementing-semantic-caching-in-rag-using-redis-for-faster-responses-b901bcc8324b
    // https://redis.io/docs/latest/develop/clients/dotnet/
    // 
    // 
    // 
    // 
    // docker run -p 6379:6379 -p 8001:8001 redis/redis-stack
    // docker run -p 6379:6379 --name redis-stack redis/redis-stack:latest


    Console.WriteLine("Response > ");
    await foreach (var responseToken in chatService.GetResponseAsync(userInput))
    {
        Console.Write(responseToken);
    }

    Console.WriteLine();

} while (userInput != "/q");
