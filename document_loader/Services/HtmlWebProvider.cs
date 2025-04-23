using System.Diagnostics.CodeAnalysis;
using document_loader.Services.Interfaces;
using HtmlAgilityPack;

namespace document_loader.Services;

public class HtmlWebProvider : IHtmlWebProvider
{
    private readonly HtmlWeb _htmlWeb;

    public HtmlWebProvider()
    {
        _htmlWeb = new HtmlWeb();
    }

    public async Task<HtmlDocument> LoadFromWebAsync(string downloadPath)
    {
        return await _htmlWeb.LoadFromWebAsync(downloadPath);
    }
}
