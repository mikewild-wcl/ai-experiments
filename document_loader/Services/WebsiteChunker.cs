using document_loader.Services.Interfaces;

namespace document_loader.Services;

public class WebsiteChunker(
    IHtmlWebProvider htmlWebProvider) : IDocumentChunker
{
    public readonly IHtmlWebProvider _htmlWebProvider = htmlWebProvider;

    public async IAsyncEnumerable<string> StreamChunks(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            yield break;
        }

        //Use a regex to extract url of there isn't a file extension and it starts http
        //Then can use HtmlAgilityPack to get data

        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(100);
            yield return $"website line {i}";
        }
    }
}
