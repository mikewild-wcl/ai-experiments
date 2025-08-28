using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using semantic_kernel_redis_cache.Services.Interfaces;
using StackExchange.Redis;
using static NRedisStack.Search.Schema;

namespace semantic_kernel_redis_cache.Services;

public class SemanticCacheService(
    IConnectionMultiplexer connectionMultiplexer,
    Kernel kernel,
    ILogger<SemanticCacheService> logger)
    : ISemanticCacheService
{
    private readonly Kernel _kernel = kernel;
    private readonly ILogger<SemanticCacheService> _logger = logger;

    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    private readonly Lock _lockObject = new();
    private bool _indexCreated = false;

    const string VectorIndexName = "vector_idx";
    const string VectorJsonIndexName = "vector_json_idx";
    const int VectorDimensions = 1536;
    public async Task<(bool Found, string? CachedResponse)> Query(string prompt)
    {
        await CreateIndexIfNotExists();

        var embedding = await GenerateEmbeddingVector(prompt);
        var bytes = await ConvertToByteArray(embedding);

        var found = false;
        string? cachedQuery = prompt;

        return (found, cachedQuery);
    }

    public async Task Save(string prompt, string chatResponse)
    {
        await CreateIndexIfNotExists();

        var embedding = await GenerateEmbeddingVector(prompt);
        var bytes = await ConvertToByteArray(embedding);

        // Save to Redis with embedding as vector
    }

    private async Task CreateIndexIfNotExists()
    {
        if (_indexCreated)
        {
            return;
        }

        // This class should be injected as a singleton - create the index on first use. Consider rewriting as a Lazy class
        // lock - https://www.infoworld.com/article/3632180/how-to-use-the-new-lock-object-in-c-sharp-13.html

#pragma warning disable S6966 // Awaitable method should be used - turned off because await cannot be used inside lock scope
        using (_lockObject.EnterScope())
        {
            if (_indexCreated)
            {
                return;
            }

            var schema = new Schema()
            .AddTextField(new FieldName("content", "content"))
            .AddTagField(new FieldName("genre", "genre"))
            .AddVectorField("embedding", VectorField.VectorAlgo.HNSW,
                new Dictionary<string, object>()
                {
                    ["TYPE"] = "FLOAT32",
                    ["DIM"] = $"{VectorDimensions}",
                    ["DISTANCE_METRIC"] = "L2"
                }
            );

            try
            {
                _database.FT().DropIndex(VectorIndexName);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Unable to drop index {Index} because it did not exist. {Exception}", VectorIndexName, ex);
            }

            /*
              {
                "prompt": "user prompt",
                "responseText": "response text",
                "embedding": [0.0001, 0.0002, ..., 0.1536]
              }
             */
            var jsonSchema = new Schema()
                .AddTextField(new FieldName("$.prompt", "prompt"))
                .AddTagField(new FieldName("$.responseText", "responseText"))
                .AddVectorField(
                    new FieldName("$.embedding", "embedding"),
                    VectorField.VectorAlgo.HNSW,
                    new Dictionary<string, object>()
                    {
                        ["TYPE"] = "FLOAT32",
                        ["DIM"] = $"{VectorDimensions}",
                        ["DISTANCE_METRIC"] = "L2"
                    }
                );

            _database.FT().Create(
                VectorIndexName,
                new FTCreateParams()
                    .On(IndexDataType.HASH)
                    .Prefix("doc:"),
                schema);

            try
            {
                _database.FT().DropIndex(VectorJsonIndexName);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Unable to drop index {Index} because it did not exist. {Exception}", VectorJsonIndexName, ex);
            }

            _indexCreated = _database.FT().Create(
                VectorJsonIndexName,
                new FTCreateParams()
                    .On(IndexDataType.JSON)
                    .Prefix("jdoc:"),
                schema);

            _logger.LogInformation("Created index {Index}.", VectorJsonIndexName);
            _logger.LogInformation("Created index {Index}.", VectorIndexName);

            _indexCreated = true;
        }
#pragma warning restore S6966 // Awaitable method should be used
    }

    private async Task CreateJsonIndexIfNotExists()
    {
        if (_indexCreated)
        {
            return;
        }

        // This class should be injected as a singleton - create the index on first use. Consider rewriting as a Lazy class
        // lock - https://www.infoworld.com/article/3632180/how-to-use-the-new-lock-object-in-c-sharp-13.html
        using (_lockObject.EnterScope())
        {
            if (_indexCreated)
            {
                return;
            }



        }
    }

    private async Task<ReadOnlyMemory<float>> GenerateEmbeddingVector(string prompt)
    {
        var embeddingGenerator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        var embedding = await embeddingGenerator.GenerateVectorAsync(prompt);

        var floatArray = embedding.ToArray();
        byte[] byteArray = new byte[floatArray.Length * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);

        return embedding;
    }

    private async Task<byte[]> ConvertToByteArray(ReadOnlyMemory<float> embedding)
    {
        var floatArray = embedding.ToArray();
        byte[] byteArray = new byte[floatArray.Length * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);

        return byteArray;
    }

    /*
     * Use RedisMemoryStore? https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.connectors.redis.redismemorystore?view=semantic-kernel-dotnet
     * 
     * https://devblogs.microsoft.com/semantic-kernel/making-ai-powered-net-apps-more-consistent-and-intelligent-with-redis/

    //Create and use Redis semantic memory store
    RedisMemoryStore memoryStore = new RedisMemoryStore(database, vectorSize: 1536);
    var memory = new SemanticTextMemory(
        memoryStore,
        new AzureOpenAITextEmbeddingGenerationService(aoaiEmbeddingModel, aoaiEndpoint, aoaiApiKey)
        );

    //Code for saving text strings into Redis Semantic Store
    await memory.SaveInformationAsync(collectionName, $"{your_text_blob}", $"{an_arbitrary_key}");     
    

    ////
    RedisValue[] userMsgList = await _redisConnection.BasicRetryAsync(
    async(db) =>(await db.HashValuesAsync(_userName + ":" + userMessageSet)));

if (userMsgList.Any()) {
  foreach (var userMsg in userMsgList) {
    chat.AddUserMessage(userMsg.ToString());
  }
}

    /////

     
     
     chat.AddUserMessage(question);

await _redisConnection.BasicRetryAsync(
    async(_db) => _db.HashSetAsync($"{_userName}:{userMessageSet}", [
      new HashEntry(new RedisValue(Utility.GetTimestamp()), question)
    ]));
     */
}
