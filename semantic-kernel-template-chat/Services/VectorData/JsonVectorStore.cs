using Microsoft.Extensions.VectorData;

namespace Services.VectorData;

/// <summary>
/// This VectorStore implementation is for prototyping only. Do not use this in production.
/// In production, you must replace this with a real vector store. There are many VectorStore
/// implementations available, including ones for standalone vector databases like Qdrant or Milvus,
/// or for integrating with relational databases such as SQL Server or PostgreSQL.
/// 
/// This implementation stores the vector records in large JSON files on disk. It is very inefficient
/// and is provided only for convenience when prototyping.
/// </summary>
public class JsonVectorStore(string basePath) : VectorStore
{
    public override Task<bool> CollectionExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task EnsureCollectionDeletedAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string name, VectorStoreCollectionDefinition? vectorStoreCollectionDefinition = null)
        => new JsonVectorStoreCollection<TKey, TRecord>(name, Path.Combine(basePath, name + ".json"), vectorStoreCollectionDefinition);

    public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(string name, VectorStoreCollectionDefinition definition)
    {
        throw new NotImplementedException();
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        throw new NotImplementedException();
    }

    public override IAsyncEnumerable<string> ListCollectionNamesAsync(CancellationToken cancellationToken = default)
        => Directory.EnumerateFiles(basePath, "*.json").ToAsyncEnumerable();
}
