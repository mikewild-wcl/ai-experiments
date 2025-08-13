using document_loader.Services;
using document_loader.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();

builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddScoped<IDocumentProcessor, DocumentProcessor>();
builder.Services.AddScoped<IDocumentChunkerFactory, DocumentChunkerFactory>();
builder.Services.AddTransient<DocxDocumentChunker>();
builder.Services.AddTransient<PdfDocumentChunker>();
builder.Services.AddTransient<WebsiteChunker>();

builder.Services.AddTransient<IHtmlWebProvider, HtmlWebProvider>();

var processor = builder
    .Services
    .BuildServiceProvider()
    .GetRequiredService<IDocumentProcessor>();

string? userInput;
do
{
    // Collect user input
    Console.Write("Document > ");
    userInput = Console.ReadLine();
    if(string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    var filePath = userInput.Replace("\"", ""); // Normalize - remove quotes

    Console.WriteLine("Results > ");

    await foreach(var line in processor.ProcessDocument(filePath))
    {
        Console.WriteLine($"line: {line}");
    }
} while (!string.IsNullOrWhiteSpace(userInput));

