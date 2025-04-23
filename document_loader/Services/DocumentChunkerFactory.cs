using document_loader.Services.Interfaces;
using document_loader.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace document_loader.Services;

public class DocumentChunkerFactory(IServiceProvider serviceProvider) : IDocumentChunkerFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IDocumentChunker Create(DocumentType documentType) =>
        documentType switch
        {
            DocumentType.Pdf => _serviceProvider.GetRequiredService<PdfDocumentChunker>(),
            DocumentType.Docx => _serviceProvider.GetRequiredService<DocxDocumentChunker>(),
            DocumentType.WebPage => _serviceProvider.GetRequiredService<WebsiteChunker>(),
            _ => throw new ArgumentException("Invalid document type", nameof(documentType))
        };
}
