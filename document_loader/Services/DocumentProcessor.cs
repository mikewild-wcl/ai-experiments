
using document_loader.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace document_loader.Services;

public class DocumentProcessor(ILogger<DocumentProcessor> logger) : IDocumentProcessor
{
    private readonly ILogger<DocumentProcessor> _logger = logger;

    public async IAsyncEnumerable<string> ProcessDocument(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            yield break;
        }

        var documentExtension = Path.GetExtension(path);
        /*
        documentExtension switch
        {
            ".txt" => _logger.LogInformation("Processing text file"),
            ".pdf" => _logger.LogInformation("Processing PDF file"),
            ".docx" => _logger.LogInformation("Processing Word file"),
            _ => _logger.LogWarning("Unknown file type")
        };
        */

        //Use a regex to extract url of there isn't a file extension and it starts http
        //Then can use HtmlAgilityPack to get data

        //TODO: Use a factory to get the doc reader based on file extension
        //      Then return a stream
        // Docs - https://github.com/MicrosoftDocs/semantic-kernel-docs/blob/main/semantic-kernel/concepts/vector-store-connectors/how-to/vector-store-data-ingestion.md
        await foreach (var item in GenerateData())
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
