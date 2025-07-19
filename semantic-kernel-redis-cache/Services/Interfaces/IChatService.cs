namespace semantic_kernel_redis_cache.Services.Interfaces;

public interface IChatService
{
    IAsyncEnumerable<string> GetResponseAsync(string userMessage, CancellationToken cancellationToken = default);
}
