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

        foreach (var item in textNodes
            .Where(n => n.ParentNode?.Name != "script" &&
                         n.ParentNode?.Name != "style" &&
                         !string.IsNullOrWhiteSpace(n.InnerText)))
        {
            yield return $"website at {item.StreamPosition} - {item.InnerText}";
        }
    }

    private async Task<HtmlNodeCollection?> GetTextNodes(string uri)
    {
        try
        {
            // https://stackoverflow.com/questions/4182594/grab-all-text-from-html-with-html-agility-pack

            var htmlDocument = await _htmlWeb.LoadFromWebAsync(uri);

            var textNodes = htmlDocument
                .DocumentNode
                .SelectNodes("//text()");

            //IEnumerable < HtmlNode> nodes = doc.DocumentNode.Descendants().Where(n =>
            //    n.NodeType == HtmlNodeType.Text &&
            //    n.ParentNode.Name != "script" &&
            //    n.ParentNode.Name != "style");

            return textNodes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to access download page at {Uri}", uri);
        }

        return default;
    }
}
