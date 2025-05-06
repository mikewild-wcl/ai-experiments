using semantic_kernel_text_search.Services.Interfaces;

namespace semantic_kernel_text_search.Services;

public class DocumentIngester : IDocumentIngester
{
    public async Task<string?> IngestDocumentsFromPrompt(string? prompt)
    {
        return prompt;
    }
}
