namespace semantic_kernel_redis_cache.Services.Interfaces;

public interface ISemanticCacheService
{
    Task<(bool Found, string? CachedResponse)> Query(string prompt);

    Task Save(string prompt, string chatResponse);
}
