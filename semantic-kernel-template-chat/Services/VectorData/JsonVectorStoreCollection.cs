using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System.Linq.Expressions;
using System.Numerics.Tensors;
using System.Reflection;
using System.Text.Json;

namespace Services.VectorData;

internal class JsonVectorStoreCollection<TKey, TRecord> : VectorStoreCollection<TKey, TRecord>
    where TKey : notnull
    where TRecord : class
{
    private static readonly Func<TRecord, TKey> _getKey = CreateKeyReader();
    private static readonly Func<TRecord, ReadOnlyMemory<float>> _getVector = CreateVectorReader();

    private readonly string _name;
    private readonly string _filePath;
    private readonly VectorStoreCollectionMetadata _collectionMetadata;
    private Dictionary<TKey, TRecord>? _records;

    public JsonVectorStoreCollection(string name, string filePath, VectorStoreCollectionDefinition? vectorStoreCollectionDefinition)
    {
        _name = name;
        _filePath = filePath;

        if (File.Exists(filePath))
        {
            _records = JsonSerializer.Deserialize<Dictionary<TKey, TRecord>>(File.ReadAllText(filePath));
        }

        _collectionMetadata = new()
        {
            VectorStoreSystemName = JsonStoreConstants.VectorStoreSystemName,
            CollectionName = name
        };
    }

    public override string Name => _name;

    public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_records is not null);

    public async Task CreateCollectionAsync(CancellationToken cancellationToken = default)
    {
        _records = [];
        await WriteToDiskAsync(cancellationToken);
    }

    public override async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        if (_records is null)
        {
            await CreateCollectionAsync(cancellationToken);
        }
    }

    public override Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        _records!.Remove(key);
        return WriteToDiskAsync(cancellationToken);
    }

    //VectorStoreCollection<TKey, TRecord>. EnsureCollectionDeletedAsync(CancellationToken)

    public Task DeleteBatchAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default)
    {
        foreach (var key in keys)
        {
            _records!.Remove(key);
        }

        return WriteToDiskAsync(cancellationToken);
    }

    public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default)
    {
        _records?.Clear();

        return WriteToDiskAsync(cancellationToken);
    }

    public Task DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        _records = null;
        File.Delete(_filePath);
        return Task.CompletedTask;
    }

    public override Task<TRecord?> GetAsync(TKey key, RecordRetrievalOptions? options = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_records!.GetValueOrDefault(key));
    public override IAsyncEnumerable<TRecord> GetAsync(Expression<Func<TRecord, bool>> filter, int top,
        FilteredRecordRetrievalOptions<TRecord>? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        //Verify.NotLessThan(top, 1);

        options ??= new();

        var records = _records
            .Values
            //.Cast<TRecord>()
            //.Select(x => x)
            .AsQueryable()
            .Where(filter);

        var orderBy = options.OrderBy?.Invoke(new()).Values;
        if (orderBy is { Count: > 0 })
        {
            var first = orderBy[0];
            var sorted = first.Ascending
                    ? records.OrderBy(first.PropertySelector)
                    : records.OrderByDescending(first.PropertySelector);

            for (int i = 1; i < orderBy.Count; i++)
            {
                var next = orderBy[i];
                sorted = next.Ascending
                    ? sorted.ThenBy(next.PropertySelector)
                    : sorted.ThenByDescending(next.PropertySelector);
            }

            records = sorted;
        }

        return records
            .Skip(options.Skip)
            .Take(top)
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<TRecord> GetBatchAsync(IEnumerable<TKey> keys, RecordRetrievalOptions? options = null, CancellationToken cancellationToken = default)
        => keys.Select(key => _records!.GetValueOrDefault(key)!).Where(r => r is not null).ToAsyncEnumerable();

    //public override async Task<TKey> UpsertAsync(TRecord record, CancellationToken cancellationToken = default)
    //{
    //    var key = _getKey(record);
    //    _records![key] = record;
    //    await WriteToDiskAsync(cancellationToken);
    //    return key;
    //}
    public override Task UpsertAsync(TRecord record, CancellationToken cancellationToken = default)
        => UpsertAsync([record], cancellationToken);

    //public override Task UpsertAsync(IEnumerable<TRecord> records, CancellationToken cancellationToken = default);
    public override async Task UpsertAsync(IEnumerable<TRecord> records, CancellationToken cancellationToken = default)
    {
        var results = new List<TKey>();
        foreach (var record in records)
        {
            var key = _getKey(record);
            _records![key] = record;
            results.Add(key);
        }

        await WriteToDiskAsync(cancellationToken);

        //foreach (var key in results)
        //{
        //    yield return key;
        //}
    }

    public override IAsyncEnumerable<VectorSearchResult<TRecord>> SearchAsync<TVector>(TVector vector, int top, VectorSearchOptions<TRecord>? options = null, CancellationToken cancellationToken = default)
    {
        if (vector is not ReadOnlyMemory<float> floatVector)
        {
            throw new NotSupportedException($"The provided vector type {vector!.GetType().FullName} is not supported.");
        }

        IEnumerable<TRecord> filteredRecords = _records!.Values.Cast<TRecord>();

        if (options?.Filter is not null)
        {
            filteredRecords = filteredRecords.AsQueryable().Where(options.Filter);
        }

        var ranked = from record in filteredRecords
                     let candidateVector = _getVector(record)
                     let similarity = TensorPrimitives.CosineSimilarity(candidateVector.Span, floatVector.Span)
                     orderby similarity descending
                     select (Record: record, Similarity: similarity);

        var results = ranked.Skip(options?.Skip ?? 0).Take(top);
        return results.Select(r => new VectorSearchResult<TRecord>(r.Record, r.Similarity)).ToAsyncEnumerable();
    }

    private static Func<TRecord, TKey> CreateKeyReader()
    {
        var propertyInfo = typeof(TRecord).GetProperties()
            .Where(p => p.GetCustomAttribute<VectorStoreKeyAttribute>() is not null
                && p.PropertyType == typeof(TKey))
            .Single();
        return record => (TKey)propertyInfo.GetValue(record)!;
    }

    private static Func<TRecord, ReadOnlyMemory<float>> CreateVectorReader()
    {
        var propertyInfo = typeof(TRecord).GetProperties()
            .Where(p => p.GetCustomAttribute<VectorStoreVectorAttribute>() is not null
                && p.PropertyType == typeof(ReadOnlyMemory<float>))
            .Single();
        return record => (ReadOnlyMemory<float>)propertyInfo.GetValue(record)!;
    }

    private async Task WriteToDiskAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_records);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    public override object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));

        return
            serviceKey is not null ? null :
            serviceType == typeof(VectorStoreCollectionMetadata) ? _collectionMetadata :
            serviceType == typeof(Dictionary<TKey, TRecord>) ? this._records :
            serviceType.IsInstanceOfType(this) ? this :
            null;
    }
}
