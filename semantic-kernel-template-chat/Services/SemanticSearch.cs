using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace semantic_kernel_template_chat.Services;

public class SemanticSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore)
{
    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchAsync(string text, string? filenameFilter, int maxResults)
    {
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(text);
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-semantic-kernel-template-chat-ingested");

        /*
        var oldFilter = filenameFilter is { Length: > 0 }
            ? new VectorSearchFilter().EqualTo(nameof(SemanticSearchRecord.FileName), filenameFilter)
            : null;
        */
        var nearest = vectorCollection.SearchAsync(queryEmbedding, maxResults, new VectorSearchOptions<SemanticSearchRecord>
        {
            Filter = x => string.IsNullOrEmpty(filenameFilter) || string.Equals(x.FileName, filenameFilter)
        });

        var results = new List<SemanticSearchRecord>();
        await foreach (var item in nearest)
        {
            results.Add(item.Record);
        }

        return results;
    }
}
