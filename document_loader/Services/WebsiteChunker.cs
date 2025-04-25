using document_loader.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace document_loader.Services;

public class WebsiteChunker(
    IHtmlWebProvider htmlWebProvider,
    ILogger<WebsiteChunker> logger) : IDocumentChunker
{
    public readonly IHtmlWebProvider _htmlWeb = htmlWebProvider;
    public readonly ILogger<WebsiteChunker> _logger = logger;

    public async IAsyncEnumerable<string> StreamChunks(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            yield break;
        }

        var textNodes = await GetTextNodes(filePath);


        if (textNodes is null)
        {
            yield break;
        }

        foreach (var item in textNodes.Where(t => !string.IsNullOrWhiteSpace(t.InnerText)))
        {
            yield return $"website line {item.InnerText}";
        }
    }

    private async Task<HtmlNodeCollection?> GetTextNodes(string uri)
    {

        try
        {
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(uri);
            var textNodes = htmlDocument.DocumentNode.SelectNodes("//text()");
            return textNodes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to access download page at {Uri}", uri);
        }

        return default;
    }
}
