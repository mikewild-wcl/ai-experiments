using document_loader.Services;
using document_loader.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

var configuration = new ConfigurationBuilder()
    .Build();

var builder = Kernel.CreateBuilder();

builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddScoped<IDocumentProcessor, DocumentProcessor>();
builder.Services.AddScoped<IDocumentChunkerFactory, DocumentChunkerFactory>();
builder.Services.AddTransient<IDocumentChunker, DocxDocumentChunker>();
builder.Services.AddTransient<IDocumentChunker, PdfDocumentChunker>();
builder.Services.AddTransient<IDocumentChunker, WebsiteChunker>();
//builder.Services.AddTransient<IDocumentChunker>();

var processor = builder
    .Services
    .BuildServiceProvider()
    .GetService<IDocumentProcessor>();

string? userInput;
do
{
    // Collect user input
    Console.Write("Document > ");
    userInput = Console.ReadLine();

    var response = await processor!.Test(userInput);
    Console.WriteLine("Results > ");

    await foreach(var line in processor.ProcessDocument(userInput))
    {
        Console.WriteLine($"line: {line}");
    }
} while (userInput is not null);

