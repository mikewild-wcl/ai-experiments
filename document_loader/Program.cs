using document_loader.Services;
using document_loader.Services.Interfaces;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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
    filePath = Path.GetFullPath(filePath);

    Console.WriteLine("Results > ");

    await foreach(var line in processor.ProcessDocument(filePath))
    {
        Console.WriteLine($"line: {line}");
    }
} while (!string.IsNullOrWhiteSpace(userInput));

