using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var apiKey = config["AzureOpenAISettings:ApiKey"]
    ?? throw new InvalidOperationException("AzureOpenAIKey not found in user secrets.");
var endpoint = config["AzureOpenAISettings:Endpoint"]
    ?? throw new InvalidOperationException("AzureOpenAIKey not found in user secrets.");
var embeddingDeploymentName = config["AzureOpenAISettings:EmbeddingDeploymentName"]
    ?? throw new InvalidOperationException("AzureOpenAIKey not found in user secrets.");

var endpointUri = new Uri(endpoint);
var credential = new AzureKeyCredential(apiKey);

var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = endpointUri
};

//var client = new EmbeddingClient(embeddingDeploymentName, credential, openAIOptions);
var client = new AzureOpenAIClient(endpointUri, credential)
    .GetEmbeddingClient(embeddingDeploymentName);

var response = client.GenerateEmbeddings(
    ["first phrase", "second phrase", "third phrase"]
).Value;

foreach (var embedding in response)
{
    ReadOnlyMemory<float> vector = embedding.ToFloats();
    int length = vector.Length;
    Console.Write($"data[{embedding.Index}]: length={length}, ");
    Console.Write($"[{vector.Span[0]}, {vector.Span[1]}, ..., ");
    Console.WriteLine($"{vector.Span[length - 2]}, {vector.Span[length - 1]}]");
}
