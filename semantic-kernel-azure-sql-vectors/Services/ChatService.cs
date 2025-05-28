using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace semantic_kernel_azure_sql_vectors.Services.Interfaces;

public class ChatService(
    Kernel kernel,
    ILogger<ChatService> logger)
    :  IChatService
{
    private readonly Kernel _kernel = kernel;
    private readonly ILogger<ChatService> _logger = logger;
    private ChatHistory _chatHistory = [];

    public async IAsyncEnumerable<string> GetResponseAsync(string userMessage, string userId, CancellationToken cancellationToken = default)
    {
        if (!_chatHistory.Any())
        {
            _chatHistory.AddSystemMessage(
            """
            You are a helpful assistant that answers questions in rhyme. 
            """);
        }

        _chatHistory.AddUserMessage(userMessage);

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var responses = new StringBuilder();

        await foreach (var item in chatCompletionService.GetStreamingChatMessageContentsAsync(_chatHistory))
        {
            if (item.Metadata?.Any() == true)
            {
                foreach (var property in item.Metadata)
                {
                    _logger.LogInformation("AI response has additional property {Key} = {Value}", property.Key, property.Value);
                }
            }

            _logger.LogInformation("AI response '{Content}'", item.Content);
            responses.Append(item.Content);
            yield return item.Content;
        }

        _chatHistory.AddAssistantMessage(responses.ToString());

    }
}
