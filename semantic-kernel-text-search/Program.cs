// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using OpenAI;
using semantic_kernel_text_search.Services;
using semantic_kernel_text_search.Services.Interfaces;
using semantic_kernel_console_gemini;
using System.ClientModel;
using semantic_kernel_text_search.Data;

/*
  https://github.com/microsoft/semantic-kernel/discussions/7090
  https://github.com/microsoft/semantic-kernel/discussions/7125

Vector loading - https://github.com/MicrosoftDocs/semantic-kernel-docs/blob/main/semantic-kernel/concepts/vector-store-connectors/how-to/vector-store-data-ingestion.md

Text search - https://github.com/MicrosoftDocs/semantic-kernel-docs/blob/main/semantic-kernel/concepts/text-search/index.md

Azure SQL vector DB - https://github.com/marcominerva/SqlDatabaseVectorSearch


 * */

const string ApiKeyName = "GITHUB_TOKEN";
const string ModelIdName = "OpenAiSettings:ModelId";
const string EmbeddingModelIdName = "OpenAiSettings:EmbeddingModelId";
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
var embeddingModelId = configuration.GetValue<string>(EmbeddingModelIdName);

InvalidOperationThrowHelper.ThrowIfNullOrEmpty(apiKey, "ApiKey must be provided.");
InvalidOperationThrowHelper.ThrowIfNullOrEmpty(modelId, "ModelId must be provided.");
InvalidOperationThrowHelper.ThrowIfNullOrEmpty(embeddingModelId, "EmbeddingModelId must be provided.");
InvalidOperationThrowHelper.ThrowIfNullOrEmpty(endpoint, "Endpoint must be provided.");

//TODO: 
//configuration.GetSection("OpenApiSettings").Bind(openApiSettings);

Console.WriteLine($"Using model {modelId} and embedding model {embeddingModelId}");

var builder = Host.CreateApplicationBuilder(args);

//Dependency injection
//https://devblogs.microsoft.com/semantic-kernel/using-semantic-kernel-with-dependency-injection/

///https://devblogs.microsoft.com/dotnet/github-ai-models-dotnet-semantic-kernel/
var openAiClient = new OpenAIClient(
    new ApiKeyCredential(apiKey!),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(endpoint!)
    });

#pragma warning disable SKEXP0010  // Type is for evaluation purposes only
builder.Services.AddOpenAIEmbeddingGenerator(
    modelId: embeddingModelId!,
    openAiClient, // ? openAIClient = null, /endpoint!,
                  //apiKey: apiKey!,
                  //serviceId: "YOUR_SERVICE_ID",
    dimensions: 1536 // Optional number of dimensions to generate embeddings with.
);
#pragma warning restore SKEXP0010

///https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/inmemory-connector?pivots=programming-language-csharp
builder.Services.AddInMemoryVectorStore();
//builder.Services.AddInMemoryVectorStoreCollection<string, TextParagraph>("documentData");

builder.Services.AddAzureSql<DocumentDbContext>(builder.Configuration.GetConnectionString("SqlConnection"), options =>
{
    options.UseVectorSearch();
}, options =>
{
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddTransient<IUserPromptReader, UserPromptReader>();
builder.Services.AddTransient<IDocumentIngester, DocumentIngester>();
builder.Services.AddTransient<IDocumentLoaderFactory, DocumentLoaderFactory>();
builder.Services.AddTransient<DocxDocumentLoader>();

builder.Services.AddTransient((serviceProvider) =>
{
    return new Kernel(serviceProvider);
});

var host = builder.Build();

var documentIngester = host.Services.GetRequiredService<IDocumentIngester>();
var promptReader = host.Services.GetRequiredService<IUserPromptReader>();

await documentIngester.IngestDocumentsFromParameterList(args);

string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput?.Trim() == "/h" || userInput?.Trim() == "/help" || userInput?.Trim() == "--help" || userInput?.Trim() == "--h")
    {
        WriteHelpTextToConsole();
        continue;
    }

    userInput = await documentIngester.IngestDocumentsFromPrompt(userInput);
    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    Console.WriteLine("Response > ");
    await foreach (var responseToken in promptReader.ProcessDocument(userInput))
    {
        Console.Write(responseToken);
    }

    Console.WriteLine();

} while (userInput != "/q");

static void WriteHelpTextToConsole()
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Commands:");
    Console.WriteLine("  /file:\t\tUpload a file from a path on your local disk into the search store");
    Console.WriteLine("  /url:\t\tDownload a file or a web page from a website and load into the search store");
    Console.WriteLine("  /h:\t\tShow this help text. /help, --help and --h are also accepted:");
    Console.WriteLine("");
    Console.ForegroundColor = color;
}
