using Azure;
using Azure.AI.Inference;


var endpoint = new Uri("https://models.github.ai/inference");
var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
var model = "openai/gpt-4.1";

var client = new ChatCompletionsClient(
    endpoint,
    credential,
    new AzureAIInferenceClientOptions());

var requestOptions = new ChatCompletionsOptions()
{
    Messages =
    {
        new ChatRequestSystemMessage(""),
        new ChatRequestUserMessage("What is the capital of France?"),
    },
    Model = model
};

Response<ChatCompletions> response = client.Complete(requestOptions);
Console.WriteLine(response.Value.Content);