using document_loader.Services.Interfaces;
using document_loader.Models.Enums;

namespace document_loader.Services;

public class DocumentChunkerFactory : IDocumentChunkerFactory
{
    public IDocumentChunker Create(DocumentType documentType) =>
        documentType switch
        {
            DocumentType.Pdf => new PdfDocumentChunker(),
            DocumentType.Docx => new DocxDocumentChunker(),
            DocumentType.Html => new WebsiteChunker(),
            _ => throw new ArgumentException("Invalid document type", nameof(documentType))
        };
}
