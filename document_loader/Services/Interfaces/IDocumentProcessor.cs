namespace document_loader.Services.Interfaces;

public interface IDocumentProcessor
{
    IAsyncEnumerable<string> ProcessDocument(string? path);
}
