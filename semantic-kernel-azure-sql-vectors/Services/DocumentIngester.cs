using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Polly.Registry;
using semantic_kernel_azure_sql_vectors.Models;
using semantic_kernel_azure_sql_vectors.Services.Interfaces;

namespace semantic_kernel_azure_sql_vectors.Services;

public class DocumentIngester(
    IDocumentLoader documentLoader,
    Kernel kernel,
    VectorStore vectorStore,
    ResiliencePipelineProvider<string> resiliencePipelineProvider,
    ILogger<DocumentIngester> logger) : IDocumentIngester
{
    public readonly IDocumentLoader _documentLoader = documentLoader;
    private readonly Kernel _kernel = kernel;
    private readonly VectorStore _vectorStore = vectorStore;
    private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider = resiliencePipelineProvider;
    private readonly ILogger<DocumentIngester> _logger = logger;

    public async Task Ingest(string? path)
    {
        //using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var stream = OpenFileStream(path);
        if (stream is null)
        {
            _logger.LogWarning("Could not open file stream for path: {Path}", path);
            return;
        }

        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline("retryPipeline");
        var collection = await resiliencePipeline.ExecuteAsync(
            async ct =>
            {
                var collection = _vectorStore.GetCollection<string, TextParagraph>(Constants.CollectionName);
                await collection.EnsureCollectionExistsAsync();
                return collection;
            },
           default(CancellationToken));

        if (collection is null)
        {
            _logger.LogError("Could not get collection: {CollectionName}", Constants.CollectionName);
            return;
        }

        var embeddingGenerator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        await foreach (var paragraph in _documentLoader.StreamParagraphs(stream, path))
        {
            // Generate the text embedding.
            //_logger.LogInformation("Generating embedding for paragraph: {ParagraphId}", paragraph.ParagraphId);
            //paragraph.DocumentUri = path;

            paragraph.TextEmbedding = await embeddingGenerator.GenerateVectorAsync(paragraph.Text);

            await collection.UpsertAsync(paragraph);
        }
    }

    private Stream? OpenFileStream(string? path)
    {
        if (!File.Exists(path))
        {
            _logger.LogWarning("File was not found: {Path}", path);
            return null;
        }

        var fileExtension = Path.GetExtension(path)?.ToLowerInvariant();
        if (fileExtension != ".docx")
        {
            _logger.LogWarning("Unsupported file type: {FileExtension}", fileExtension);
            return null;
        }

        return new FileStream(path, FileMode.Open, FileAccess.Read);
    }
}
