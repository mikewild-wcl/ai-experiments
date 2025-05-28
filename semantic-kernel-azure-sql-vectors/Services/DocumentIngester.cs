using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Polly.Registry;
using semantic_kernel_azure_sql_vectors.Models;
using semantic_kernel_azure_sql_vectors.Services.Interfaces;
using System.Threading;

namespace semantic_kernel_azure_sql_vectors.Services;

public class DocumentIngester(
    IDocumentLoader documentLoader,
    Kernel kernel,
    //IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    ResiliencePipelineProvider<string> resiliencePipelineProvider,
    ILogger<DocumentIngester> logger) : IDocumentIngester
{
    public readonly IDocumentLoader _documentLoader = documentLoader;
    //public readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
    private readonly Kernel _kernel = kernel;
    public readonly VectorStore _vectorStore = vectorStore;
    public readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider = resiliencePipelineProvider;
    public readonly ILogger<DocumentIngester> _logger = logger;

    private const string CollectionName = "text-paragraphs";

    public async Task Ingest(string? path)
    {
        //using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var stream = OpenFileStream(path);
        if(stream is null)
        {
            _logger.LogWarning("Could not open file stream for path: {Path}", path);
            return;
        }

        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline("retryPipeline");
        //VectorStoreCollection<string, TextParagraph> collection = _vectorStore.GetCollection<string, TextParagraph>(CollectionName);
        //await collection.EnsureCollectionExistsAsync();

        var collection =await resiliencePipeline.ExecuteAsync(
            async ct => {
                var collection = _vectorStore.GetCollection<string, TextParagraph>(CollectionName);
                await collection.EnsureCollectionExistsAsync();
                return collection;
            },
           default(CancellationToken));

        if (collection is null)
        {
            _logger.LogError("Could not get collection: {CollectionName}", CollectionName);
            return;
        }

        var embeddingGenerator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        await foreach (var paragraph in _documentLoader.StreamParagraphs(stream, path))
        {
            // Generate the text embedding.
            _logger.LogInformation("Generating embedding for paragraph: {ParagraphId}", paragraph.ParagraphId);
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
