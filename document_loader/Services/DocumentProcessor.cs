using document_loader.Extensions;
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

        var documentType = filePath.GetDocumentType();

        if (documentType == DocumentType.Unknown)
        {
            _logger.LogWarning("Unknown document type for path: {Path}", filePath);
            yield break;
        }

        if (documentType != DocumentType.WebPage && !File.Exists(filePath))
        {
            _logger.LogWarning("File was not found: {Path}", filePath);
            yield break;
        }

        var documentChunker = _documentChunkerFactory.Create(documentType);

        await foreach (var item in documentChunker.StreamChunks(filePath))
        {
            yield return item;
        }
    }    
}
