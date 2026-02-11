using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
//using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Web;
using Utilities;

Console.Clear();

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

//var kernel = Kernel.CreateBuilder()
//    .AddAzureOpenAIChatCompletion(
//        configuration["LanguageModel:DeploymentName"]!,
//        endpoint: configuration["LanguageModel:Endpoint"]!,
//        apiKey: configuration["LanguageModel:ApiKey"]!
//    )
//    .Build();

using var httpClient = new HttpClient(new ConsoleWriterHttpClientHandler());

var agent = new AzureOpenAIClient(
    new Uri(configuration["AzureOpenAiSettings:Endpoint"]!),
    new ApiKeyCredential(configuration["AzureOpenAiSettings:ApiKey"]!),
    new AzureOpenAIClientOptions
    {
        Transport = new HttpClientPipelineTransport(httpClient)
    })
    .GetChatClient(configuration["AzureOpenAiSettings:DeploymentName"])
    .AsIChatClient()
    .AsAIAgent(
        instructions: "You are a helpful assistant that loves talking about cooking.",
        name: "Assistant"
        );

var thread = await agent.CreateSessionAsync();

// TODO: Lookup at debugging with handlers - https://youtu.be/Gr3S1Q9eZrc?si=5BEsQf_2uv5Zht8r

// YamlDotNet templates not currently supported - see this issue:
//https://github.com/microsoft/agent-framework/issues/121
//https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-templates?pivots=programming-language-csharp

//var promptTemplate = await File.ReadAllTextAsync(Path.Join(Directory.GetCurrentDirectory(), "prompt.yaml"));
//var prompt = kernel.CreateFunctionFromPromptYaml(
//    promptTemplate,
//    new HandlebarsPromptTemplateFactory
//    {
//        AllowDangerouslySetContent = true
//    });

//var templateConfig = Microsoft.SemanticKernel.KernelFunctionYaml.ToPromptTemplateConfig(
//    promptTemplate);

//var result = await kernel.InvokeAsync(prompt,
//    arguments: new KernelArguments
//    {
//        ["dish"] = "pizza",
//        ["ingredients"] = new List<string> {
//            HttpUtility.HtmlEncode("pepperoni"),
//            HttpUtility.HtmlEncode("mozarella"),
//            HttpUtility.HtmlEncode("spinach")
//        }
//    });

var dish = "pizza";
var ingredients = new List<string> {
    HttpUtility.HtmlEncode("pepperoni"),
    HttpUtility.HtmlEncode("mozarella"),
    HttpUtility.HtmlEncode("spinach")
};

var prompt =
    $"""
    Help me cook something nice, give me a recipe for {dish}. Use the ingredients I have in the fridge: 
    
    {string.Join("\n", ingredients)}
    """;

var result = await agent.RunAsync(prompt, thread, new AgentRunOptions());

Console.WriteLine(result);
