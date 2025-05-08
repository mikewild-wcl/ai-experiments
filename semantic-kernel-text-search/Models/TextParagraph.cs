using Microsoft.Extensions.VectorData;

namespace semantic_kernel_text_search.Models;

public class TextParagraph
{
    /// <summary>A unique key for the text paragraph.</summary>
    [VectorStoreRecordKey]
    public required string Key { get; init; }

    [VectorStoreRecordData]
    public required string DocumentUri { get; init; }

    [VectorStoreRecordData]
    public required string ParagraphId { get; init; }

    [VectorStoreRecordData]
    public required string Text { get; init; }

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}