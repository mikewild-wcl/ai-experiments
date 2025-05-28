namespace semantic_kernel_text_search.Data.Entities;

public class DocumentContent
{
    public int Id { get; set; }

    public required string Content { get; set; }

    public int DocumentId { get; set; }

    public int Index { get; set; }

    public required float[] Embedding { get; set; }

    public virtual Document Document { get; set; } = null!;
}
