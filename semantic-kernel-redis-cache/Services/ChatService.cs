using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using semantic_kernel_redis_cache.Services.Interfaces;
using System.Runtime.CompilerServices;
using System.Text;

namespace semantic_kernel_redis_cache.Services;

public class ChatService(
    Kernel kernel,
    ISemanticCacheService semanticCacheService,
    ILogger<ChatService> logger)
    : IChatService
{
    private readonly Kernel _kernel = kernel;
    private readonly ISemanticCacheService _semanticCacheService = semanticCacheService;
    private readonly ILogger<ChatService> _logger = logger;
    private readonly ChatHistory _chatHistory = [];

    public async IAsyncEnumerable<string> GetResponseAsync(
        string userMessage, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        if (!_chatHistory.Any())
        {
            _chatHistory.AddSystemMessage(
            """
            You are a Silicon Valley CEO being interviewed for a podcast. 
            Answer the user's question in the style of a Silicon Valley tech bro.
            ---
            """);
        }

        using var cancellationSource = new CancellationTokenSource();

        _chatHistory.AddUserMessage(userMessage);

        var (found, cachedResponse) = await _semanticCacheService.Query(userMessage);
        if (found)
        {
            yield return cachedResponse;
            yield break;
        }

        var promptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            MaxTokens = 1000,
            Temperature = 0.9f,
            TopP = 0.9f
        };

        var responses = new StringBuilder();
        await foreach (var item in chatCompletionService.GetStreamingChatMessageContentsAsync(_chatHistory, promptExecutionSettings, cancellationToken: cancellationSource.Token))
        {
            var content = item?.Content ?? "";
            responses.Append(content);
            yield return content;
        }

        await _semanticCacheService.Save(userMessage, responses.ToString());

        _chatHistory.AddAssistantMessage(responses.ToString());
    }
}
