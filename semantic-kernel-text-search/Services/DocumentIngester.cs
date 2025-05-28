using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using semantic_kernel_text_search.Models.Enums;
using semantic_kernel_text_search.Services.Interfaces;

namespace semantic_kernel_text_search.Services;

public class DocumentIngester(
    IDocumentLoaderFactory documentLoaderFactory,
#pragma warning disable SKEXP0001  // Type is for evaluation purposes only
    //IEmbeddingGenerationService<string, float> embeddingGenerator,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
#pragma warning restore SKEXP0001
    IVectorStore vectorStore,
    ILogger<DocumentIngester> logger
    ) : IDocumentIngester
{
    private const string FilePrefix = "file:";
    private const string UrlPrefix = "url:";

    private readonly IDocumentLoaderFactory _documentLoaderFactory = documentLoaderFactory;
#pragma warning disable SKEXP0001  // Type is for evaluation purposes only
    //private readonly IEmbeddingGenerationService<string, float> _embeddingGenerator = embeddingGenerator;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
#pragma warning restore SKEXP0001
    private readonly IVectorStore _vectorStore = vectorStore;
    private readonly ILogger<DocumentIngester> _logger = logger;

    public async Task IngestDocumentsFromParameterList(string[] parameters)
    {
        foreach (var arg in parameters)
        {
            if (arg.StartsWith(FilePrefix))
            {
                var filePath = arg.Substring(FilePrefix.Length)
                    ?.Replace("\"", ""); // Normalize - remove quotes

                await IngestDocumentFromFilePath(filePath);
            }
            else if (arg.StartsWith(UrlPrefix))
            {
                var url = arg.Substring(UrlPrefix.Length);
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    await IngestDocumentFromUri(uri);
                }
                else
                {
                    _logger.LogWarning("Could not create uri. Invalid URL format: {Url}", url);
                }
            }
        }
    }

    public async Task<string?> IngestDocumentsFromPrompt(string? prompt)
    {
        return prompt;
    }

    public async Task IngestDocumentFromFilePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _logger.LogWarning("File not found: {File}", path);
            return;
        }

        var fileExtension = Path.GetExtension(path)?.ToLowerInvariant();

        var documentType = fileExtension switch
        {
            ".docx" => DocumentType.Docx,
            ".pdf" => DocumentType.Pdf,
            _ => DocumentType.Unknown
        };

        var documentLoader = _documentLoaderFactory.Create(documentType);
        if(documentLoader is null)
        {
            _logger.LogError("Document loader not found for file type {FileType} with extension {Extension}", documentType, fileExtension);
            return;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

        var chunkedData = documentLoader.StreamChunks(stream, path);
        await foreach (var chunk in chunkedData)
        {
            _logger.LogInformation("Ingesting chunk {Key} - {ParagraphId}: {Chunk}", chunk.Key, chunk.ParagraphId, chunk.Text);
        }

        //var embedding = await _embeddingGenerator
        //    .GenerateAsync(await chunkedData.Select(c => c.Text).FirstOrDefaultAsync());
            //.GenerateEmbeddingAsync(await chunkedData.Select(c => c.Text).FirstAsync());

        var embeddings = await _embeddingGenerator
            .GenerateAsync(await chunkedData.Select(c => c.Text).ToListAsync());
    }

    public async Task IngestDocumentFromUri(Uri uri)
    {
    }
}    
