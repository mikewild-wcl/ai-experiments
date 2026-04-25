using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

//var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
//var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5.4-nano";
//var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

var endpoint = configuration["AzureOpenAiSettings:Endpoint"] ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = configuration["AzureOpenAiSettings:DeploymentName"] ?? "gpt-5.4-nano";
var apiKey = configuration["AzureOpenAiSettings:ApiKey"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

#pragma warning disable MAAI001
var skillsProvider = new AgentSkillsProvider(
#pragma warning restore MAAI001
    Path.Combine(AppContext.BaseDirectory, "skills"));

AIAgent agent = new AzureOpenAIClient(
        new Uri(endpoint),
        new ApiKeyCredential(apiKey))
    .GetChatClient(deploymentName)
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "SkillsAgent",
        ChatOptions = new()
        {
            Instructions = "You are a world-class chef.",
        },
        AIContextProviders = [skillsProvider],
    });

await foreach (var update in agent.RunStreamingAsync("What can I have for dinner? I have tomatoes, chicken, and rice."))
{
    Console.Write(update);
}

Console.WriteLine();