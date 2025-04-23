using HtmlAgilityPack;

namespace document_loader.Services.Interfaces;

public interface IHtmlWebProvider
{
    Task<HtmlDocument> LoadFromWebAsync(string downloadPath);
}