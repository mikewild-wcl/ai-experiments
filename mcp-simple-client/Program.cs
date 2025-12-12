using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.ClientModel;

const string ApiKeyConfigKey = "AzureOpenAiSettings:ApiKey";
const string DeploymentConfigKey = "AzureOpenAiSettings:DeploymentName";
const string EndpointConfigKey = "AzureOpenAiSettings:Endpoint";

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetValue<string>(ApiKeyConfigKey);
var deployment = configuration.GetValue<string>(DeploymentConfigKey);
var endpoint = configuration.GetValue<string>(EndpointConfigKey);

var client = new AzureOpenAIClient(
    new Uri(endpoint!),
    new ApiKeyCredential(apiKey!));

var builder = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(deployment!, client);

builder.Services
    .AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

var kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var mcpClient = await McpClient.CreateAsync(
    new StdioClientTransport(new()
    {
        //Command = @"..\..\..\..\mcp-simple-server\bin\Debug\net9.0\mcp-simple-server.exe",
        Command = "dotnet run",
        Arguments = ["--project", "..\\..\\..\\..\\mcp-simple-server\\mcp-simple-server.csproj"],
        Name = "Minimal MCP Server",
    }));

Console.WriteLine("Available tools:");
var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"{tool}");
}
Console.WriteLine();

Console.WriteLine("Available resources:");
var resources = await mcpClient.ListResourcesAsync();
foreach (var resource in resources)
{
    Console.WriteLine($"{resource.Name} {resource.MimeType}");
    var resourceValue = await resource.ReadAsync();
    if (resourceValue?.Contents is not null)
    {
        foreach (var contentItem in resourceValue.Contents)
        {
            var textContents = contentItem as TextResourceContents;
            if(textContents is not null)
            {
                Console.WriteLine($"> {textContents.Uri} - {textContents.Text}");
            }
            else
            {
                Console.WriteLine($"> {contentItem.Uri} - type is {contentItem.GetType().Name}");
            }
        }
    }
}
Console.WriteLine();

Console.WriteLine("Available prompts:");
var prompts = await mcpClient.ListPromptsAsync();
foreach (var prompt in prompts)
{
    Console.WriteLine($"{prompt.Name}: {prompt.Description}");
    var promptValue = await prompt.GetAsync();
    if(promptValue is not null)
    {
        foreach (var promptMessage in promptValue.Messages)
        {
            var contentBlock = promptMessage.Content as TextContentBlock;
            if (contentBlock is not null)
            {
                Console.WriteLine($"> {contentBlock.Text}");
            }
            else
            {
                Console.WriteLine($"> content block type is {promptMessage.Content?.GetType().Name}");
            }
        }
    }
}
Console.WriteLine();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernel.Plugins.AddFromFunctions("simple_mcp_functions", tools.Select(f => f.AsKernelFunction()));

OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    Temperature = 0.8f,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
};
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var history = new ChatHistory();

Console.WriteLine("Hello, official MCP csharp-sdk and MCP Server!");
string? userInput= "What is the current (CET) time in Illzach, France?";
Console.Write("User > ");
Console.WriteLine(userInput);

do
{
    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    history.AddUserMessage(userInput);
      
    Console.Write("Assistant > ");

    List<StreamingChatMessageContent> updates = [];
    await foreach (var update in chatCompletionService
        .GetStreamingChatMessageContentsAsync(
            history,
            promptExecutionSettings,
            kernel))
    {
        Console.Write(update);
        updates.Add(update);
    }
    Console.WriteLine();

    history.AddRange(
        updates
        .Where(x => x.Content is not null)
        .Select(x => new ChatMessageContent(
            x.Role ?? AuthorRole.Assistant,
            x.Content)));

    Console.Write("User > ");
    userInput = Console.ReadLine();
} while (userInput is { Length: > 0 });
