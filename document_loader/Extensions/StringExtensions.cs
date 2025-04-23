using document_loader.Models.Enums;

namespace document_loader.Extensions;

public static class StringExtensions
{
    public static DocumentType GetDocumentType(this string filePath)
    {
        return Path.GetExtension(filePath)?.ToLowerInvariant() switch
        {
            ".docx" => DocumentType.Docx,
            ".pdf" => DocumentType.Pdf,
            _ => (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
                ? DocumentType.WebPage
                : DocumentType.Unknown
        };
    }
}
