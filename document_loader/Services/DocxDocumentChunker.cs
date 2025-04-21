using document_loader.Services.Interfaces;
using System.IO;

namespace document_loader.Services;

public class DocxDocumentChunker : IDocumentChunker
{
    public async IAsyncEnumerable<string> StreamChunks(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            yield break;
        }

        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(100);
            yield return $"docx line {i}";
        }
    }
}
