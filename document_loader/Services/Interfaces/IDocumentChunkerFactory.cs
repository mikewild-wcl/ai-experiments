using document_loader.Models.Enums;

namespace document_loader.Services.Interfaces;

public interface IDocumentChunkerFactory
{
    IDocumentChunker Create(DocumentType documentType);
}
