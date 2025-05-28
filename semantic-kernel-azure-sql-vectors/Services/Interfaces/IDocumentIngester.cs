namespace semantic_kernel_azure_sql_vectors.Services.Interfaces;

public interface IDocumentIngester
{
    Task Ingest(string? path);
}
