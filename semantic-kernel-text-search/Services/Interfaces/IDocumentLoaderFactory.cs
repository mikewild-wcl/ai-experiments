using semantic_kernel_text_search.Models.Enums;

namespace semantic_kernel_text_search.Services.Interfaces;

public interface IDocumentLoaderFactory
{
    IDocumentLoader? Create(DocumentType documentType);
}
