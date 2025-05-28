using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Polly;
using Polly.Retry;
using semantic_kernel_azure_sql_vectors.Models;
using semantic_kernel_azure_sql_vectors.Services;
using semantic_kernel_azure_sql_vectors.Services.Interfaces;
using System.ClientModel;
using System.Text;

const string FilePrefix = "file:";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var builder = Host.CreateApplicationBuilder(args);

//builder.Services.Configure<AzureOpenAiSettings>(configuration.GetSection(nameof(AzureOpenAiSettings)));
var azureOpenAiSettings = configuration
    .GetSection(nameof(AzureOpenAiSettings))
    .Get<AzureOpenAiSettings>();

builder.Services.AddLogging(services =>
    services
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace));

var client = new AzureOpenAIClient(
    new Uri(azureOpenAiSettings!.Endpoint),
    new ApiKeyCredential(azureOpenAiSettings.ApiKey));

var connStr = configuration.GetConnectionString("AzureSqlVectorStoreDbConnection");
builder.Services.AddSqlServerVectorStore(_ => configuration.GetConnectionString("AzureSqlVectorStoreDbConnection"));
//builder.Services.AddSqlServerVectorStore(sp =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();
//    options.ConnectionString = config.GetConnectionString("AzureSqlVectorStoreDbConnection");
//    return config.GetConnectionString("AzureSqlVectorStoreDbConnection");
//});

builder.Services.AddResiliencePipeline("retryPipeline", pipelineBuilder =>
{
    pipelineBuilder.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        Delay = TimeSpan.FromSeconds(2),
        MaxRetryAttempts = 5,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = args =>
        {
            Console.WriteLine($"Attempt: {args.AttemptNumber}");
            return ValueTask.CompletedTask;
        }
    });
});

builder.Services.AddTransient<IChatService, ChatService>();
builder.Services.AddTransient<IDocumentIngester, DocumentIngester>();
//builder.Services.AddTransient<IDocumentLoaderFactory, DocumentLoaderFactory>();
builder.Services.AddTransient<IDocumentLoader, DocxDocumentLoader>();

builder.Services.AddTransient((serviceProvider) =>
{
    var kernelBuilder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    kernelBuilder
        .AddAzureOpenAIChatCompletion(
            azureOpenAiSettings.DeploymentName,
            client)
        .AddAzureOpenAIEmbeddingGenerator(
            azureOpenAiSettings.EmbeddingDeploymentName,
            client,
            dimensions: 1536);
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    return kernelBuilder.Build();
});

var host = builder.Build();

var chatService = host.Services.GetRequiredService<IChatService>();
var documentIngester = host.Services.GetRequiredService<IDocumentIngester>();

foreach (var arg in args)
{
    if (arg.StartsWith(FilePrefix))
    {
        var filePath = arg.Substring(FilePrefix.Length)
            ?.Replace("\"", ""); // Normalize - remove quotes

        await documentIngester.Ingest(filePath);
    }
}

string userId = Guid.NewGuid().ToString(); // Unique user ID for the session
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    //userInput = await documentIngester.IngestDocumentsFromPrompt(userInput);
    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }

    Console.WriteLine("Response > ");
    var responses = new StringBuilder();

    await foreach (var responseToken in chatService.GetResponseAsync(userInput, userId))
    {
        Console.Write(responseToken);
        responses.Append(responseToken);
    }

    Console.WriteLine(responses.ToString());

} while (userInput != "/q");