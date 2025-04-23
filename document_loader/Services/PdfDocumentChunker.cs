using document_loader.Models;
using document_loader.Services.Interfaces;
using Microsoft.SemanticKernel.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace document_loader.Services;

public class PdfDocumentChunker : IDocumentChunker
{
    public async IAsyncEnumerable<string> StreamChunks(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            yield break;
        }

        if(!File.Exists(filePath))
        {
            yield break;
        }

        using var pdf = PdfDocument.Open(filePath);
        var pages = pdf.GetPages();
        var paragraphs = pages.SelectMany(GetPageParagraphs);

        foreach (var paragraph in paragraphs)
        {
            yield return $"{paragraph.PageNumber} - {paragraph.IndexOnPage} {paragraph.Text}";
        }
    }

    private static IEnumerable<(int PageNumber, int IndexOnPage, string Text)> GetPageParagraphs(Page pdfPage)
    {
        var letters = pdfPage.Letters;
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);
        var textBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        var pageText = string.Join(Environment.NewLine + Environment.NewLine,
            textBlocks.Select(t => t.Text.ReplaceLineEndings(" ")));

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only
        return TextChunker.SplitPlainTextParagraphs([pageText], Constants.MaxTokensPerParagraph)
            .Select((text, index) => (pdfPage.Number, index, text));
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only
    }
}
