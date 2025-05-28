namespace semantic_kernel_azure_sql_vectors.Services.Interfaces;

public interface IChatService
{
    IAsyncEnumerable<string> GetResponseAsync(string userMessage, string userId, CancellationToken cancellationToken = default);
}
