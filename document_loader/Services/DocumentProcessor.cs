using document_loader.Models.Enums;
using document_loader.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace document_loader.Services;

public class DocumentProcessor(
    IDocumentChunkerFactory documentChunkerFactory,
    ILogger<DocumentProcessor> logger) : IDocumentProcessor
{
    private readonly IDocumentChunkerFactory _documentChunkerFactory = documentChunkerFactory;
    private readonly ILogger<DocumentProcessor> _logger = logger;

    public async IAsyncEnumerable<string> ProcessDocument(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            yield break;
        }

        var documentExtension = Path.GetExtension(filePath);

        var documentType = documentExtension switch
        {
            ".docx" => DocumentType.Docx,
            ".pdf" => DocumentType.Pdf,
            _ => (filePath.StartsWith("http://") || filePath.StartsWith("https://")) 
                ? DocumentType.Html 
                : DocumentType.Unknown
        };

        if (documentType == DocumentType.Unknown)
        {
            _logger.LogWarning("Unknown document type for path: {Path}", filePath);
            yield break;
        }

        var documentChunker = _documentChunkerFactory.Create(documentType);


        //Use a regex to extract url of there isn't a file extension and it starts http
        //Then can use HtmlAgilityPack to get data

        //TODO: Use a factory to get the doc reader based on file extension
        //      Then return a stream
        // Docs - https://github.com/MicrosoftDocs/semantic-kernel-docs/blob/main/semantic-kernel/concepts/vector-store-connectors/how-to/vector-store-data-ingestion.md
        //await foreach (var item in GenerateData())
        //{
        //    yield return item;
        //}

        await foreach (var item in documentChunker.StreamChunks(filePath))
        {
            yield return item;
        }
    }

    public async Task<string> Test(string? path)
{
    return path;
}

private async IAsyncEnumerable<string> GenerateData()
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(100); // Simulate async work
        yield return $"line {i}";
    }
}

    //examples of how to use IAsyncEnumerable
    /*
     * 
     public async IAsyncEnumerable<int> GetNumbersAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(100); // Simulate async work
            yield return i;
        }
    }
     * 
     * 
     public IAsyncEnumerable<int> GetNumbersAsync()
        {
            var numbers = Enumerable.Range(0, 10);
            return numbers.ToAsyncEnumerable()
                          .SelectAwait(async i =>
                          {
                              await Task.Delay(100); // Simulate async work
                              return i;
                          });
        }
     */
}
