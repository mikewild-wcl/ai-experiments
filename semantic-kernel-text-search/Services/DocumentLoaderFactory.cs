using semantic_kernel_text_search.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using semantic_kernel_text_search.Models.Enums;

namespace semantic_kernel_text_search.Services;

public class DocumentLoaderFactory(IServiceProvider serviceProvider) : IDocumentLoaderFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IDocumentLoader? Create(DocumentType documentType) =>
        documentType switch
        {
            //DocumentType.Pdf => _serviceProvider.GetRequiredService<PdfDocumentLoader>(),
            DocumentType.Docx => _serviceProvider.GetRequiredService<DocxDocumentLoader>(),
            //DocumentType.WebPage => _serviceProvider.GetRequiredService<WebsiteLoader>(),
            //_ => throw new ArgumentException("Invalid document type", nameof(documentType))
            _ => null
        };
}
