using semantic_kernel_azure_sql_vectors.Models;

namespace semantic_kernel_azure_sql_vectors.Services.Interfaces;

public interface IDocumentLoader
{
    IAsyncEnumerable<TextParagraph> StreamParagraphs(Stream stream, string documentUri);
}
