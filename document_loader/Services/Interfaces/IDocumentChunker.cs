namespace document_loader.Services.Interfaces
{
    public interface IDocumentChunker
    {
        IAsyncEnumerable<string> StreamChunks(string filePath);
    }
}