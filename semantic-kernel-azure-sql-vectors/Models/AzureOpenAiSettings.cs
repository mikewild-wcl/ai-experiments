namespace semantic_kernel_azure_sql_vectors.Models;

public record AzureOpenAiSettings(
    string ApiKey,
    string Endpoint,
    string DeploymentName,
    string EmbeddingDeploymentName)
{
    public string? ModelId { get; set; }    
    public string? EmbeddingModelId { get; set; }
}
