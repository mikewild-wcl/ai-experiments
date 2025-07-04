﻿using Microsoft.Extensions.VectorData;

namespace document_loader.Models;

public class TextParagraph
{
    /// <summary>A unique key for the text paragraph.</summary>
    [VectorStoreKey]
    public required string Key { get; init; }

    /// <summary>A uri that points at the original location of the document containing the text.</summary>
    [VectorStoreData]
    public required string DocumentUri { get; init; }

    /// <summary>The id of the paragraph from the document containing the text.</summary>
    [VectorStoreData]
    public required string ParagraphId { get; init; }

    /// <summary>The text of the paragraph.</summary>
    [VectorStoreData]
    public required string Text { get; init; }

    /// <summary>The embedding generated from the Text.</summary>
    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}