// See https://aka.ms/new-console-template for more information
using DocumentFormat.OpenXml.Vml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using semantic_kernel_text_search.Services;
using semantic_kernel_text_search.Services.Interfaces;
using System.Diagnostics;

/*
  https://github.com/microsoft/semantic-kernel/discussions/7090
  https://github.com/microsoft/semantic-kernel/discussions/7125

Vector loading - https://github.com/MicrosoftDocs/semantic-kernel-docs/blob/main/semantic-kernel/concepts/vector-store-connectors/how-to/vector-store-data-ingestion.md

Text search - https://github.com/MicrosoftDocs/semantic-kernel-docs/blob/main/semantic-kernel/concepts/text-search/index.md

 * */

const string ApiKeyName = "GITHUB_TOKEN";

var envVar = Environment.GetEnvironmentVariable(ApiKeyName);
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

// Populate values from your OpenAI deployment
var modelId = "gpt-4.1";
var endpoint = "https://models.inference.ai.azure.com";
var apiKey = configuration.GetValue<string>(ApiKeyName);

var builder = Host.CreateApplicationBuilder(args);

//Dependency injection
//https://devblogs.microsoft.com/semantic-kernel/using-semantic-kernel-with-dependency-injection/

builder.Services.AddScoped<IUserPromptReader, UserPromptReader>();
builder.Services.AddTransient<IDocumentIngester, DocumentIngester>();

var host = builder.Build();

var promptReader = host.Services.GetRequiredService<IUserPromptReader>();
var documentIngester = host.Services.GetRequiredService<IDocumentIngester>();

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
    if (string.IsNullOrWhiteSpace(userInput))
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

void WriteHelpTextToConsole()
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
