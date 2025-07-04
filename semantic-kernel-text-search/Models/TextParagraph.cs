using Microsoft.Extensions.VectorData;

namespace semantic_kernel_text_search.Models;

public class TextParagraph
{
    /// <summary>A unique key for the text paragraph.</summary>
    [VectorStoreKey]
    public required string Key { get; init; }

    [VectorStoreData]
    public required string DocumentUri { get; init; }

    [VectorStoreData]
    public required string ParagraphId { get; init; }

    [VectorStoreData]
    public required string Text { get; init; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}