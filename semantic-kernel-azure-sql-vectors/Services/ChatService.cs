using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Polly.Registry;
using semantic_kernel_azure_sql_vectors.Models;
using System.Text;

namespace semantic_kernel_azure_sql_vectors.Services.Interfaces;

public class ChatService(
    Kernel kernel,
    VectorStore vectorStore,
    ResiliencePipelineProvider<string> resiliencePipelineProvider,
    ILogger<ChatService> logger)
    : IChatService
{
    private readonly Kernel _kernel = kernel;
    public readonly VectorStore _vectorStore = vectorStore;
    public readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider = resiliencePipelineProvider;
    private readonly ILogger<ChatService> _logger = logger;
    private ChatHistory _chatHistory = [];

    public async IAsyncEnumerable<string> GetResponseAsync(string userMessage, string userId, CancellationToken cancellationToken = default)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var embeddingGenerator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        if (!_chatHistory.Any())
        {
            _chatHistory.AddSystemMessage(
            """
            You are a helpful assistant that answers questions in rhyme. 
            Answer the user's question in a poetic form.
            ---
            """);
        }

        var cancellationSource = new CancellationTokenSource();
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(userMessage, cancellationToken: cancellationSource.Token);

        var resiliencePipeline = _resiliencePipelineProvider.GetPipeline("retryPipeline");
        var collection = await resiliencePipeline.ExecuteAsync(
            async ct =>
            {
                var collection = _vectorStore.GetCollection<string, TextParagraph>(Constants.CollectionName);
                await collection.EnsureCollectionExistsAsync();
                return collection;
            },
           cancellationSource.Token);

        if (collection is null)
        {
            _logger.LogError("Could not get collection: {CollectionName}", Constants.CollectionName);
            yield return "Could not access the database. Please try again.";
            yield break;
        }

        var queryPromptBuilder = new StringBuilder(
        $"""
            You are a helpful assistant that answers questions in rhyme. 
            Answer the following question in a poetic form:
            ---
            {userMessage}
            ---
            You may use the following information:
        """);

        var searchResults = collection.SearchAsync(queryEmbedding, top: 5);
        await foreach (var searchResult in searchResults)
        {
            queryPromptBuilder.Append($"---{Environment.NewLine}{searchResult.Record.Text}");
        }

        _chatHistory.AddUserMessage(queryPromptBuilder.ToString());

        var promptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            MaxTokens = 1000,
            Temperature = 0.9f,
            TopP = 0.9f
        };

        var responses = new StringBuilder();
        await foreach (var item in chatCompletionService.GetStreamingChatMessageContentsAsync(_chatHistory, promptExecutionSettings, cancellationToken: cancellationSource.Token))
        {
            //if (item.Metadata?.Any() == true)
            //{
            //    foreach (var property in item.Metadata)
            //    {
            //        _logger.LogInformation("AI response has additional property {Key} = {Value}", property.Key, property.Value);
            //    }
            //}

            //_logger.LogInformation("AI response '{Content}'", item.Content);
            responses.Append(item.Content);
            yield return item.Content;
        }

        _chatHistory.AddAssistantMessage(responses.ToString());

    }
}
