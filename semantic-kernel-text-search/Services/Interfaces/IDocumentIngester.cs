namespace semantic_kernel_text_search.Services.Interfaces;

public interface IDocumentIngester
{
    Task IngestDocumentsFromParameterList(string[] parameters);

    Task<string?> IngestDocumentsFromPrompt(string? prompt);

    Task IngestDocumentFromFilePath(string? path);

    Task IngestDocumentFromUri(Uri uri);
}
