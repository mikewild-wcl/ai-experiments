namespace semantic_kernel_text_search.Services.Interfaces;

public interface IDocumentIngester
{
    Task<string?> IngestDocumentsFromPrompt(string? prompt);
}
