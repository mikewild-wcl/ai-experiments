using semantic_kernel_text_search.Models;

namespace semantic_kernel_text_search.Services.Interfaces;

public interface IDocumentLoader
{
    IAsyncEnumerable<TextParagraph> StreamChunks(Stream stream, string documentUri);
}