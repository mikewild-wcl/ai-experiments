using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using semantic_kernel_telemetry.Extensions;
using System.ClientModel;
using System.Web;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .Build();

var deployment = configuration.GetValue<string>("AzureOpenAiSettings:DeploymentName");
var endpoint = configuration.GetValue<string>("AzureOpenAiSettings:Endpoint");
var apiKey = configuration.GetValue<string>("AzureOpenAiSettings:ApiKey");
var service = configuration.GetValue<string>("AzureOpenAiSettings:Service");

var client = new AzureOpenAIClient(new Uri(endpoint!), new ApiKeyCredential(apiKey!));

var builder = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(deployment!, client, serviceId: service);

var kernel = builder.Build();

var history = new ChatHistory();

string? userInput;
do
{
    Console.Write("Give me an idea for a joke: ");
    userInput = Console.ReadLine();

    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    //history.AddUserMessage(userInput);

    //var result = await chatCompletionService.GetChatMessageContentAsync(
    //    history,
    //    executionSettings: openAIPromptExecutionSettings,
    //    kernel: kernel);
    var template = typeof(Program).ReadManifestResourceStreamAsString("prompt.yaml");

    var prompt = kernel.CreateFunctionFromPromptYaml(
        template,
        new HandlebarsPromptTemplateFactory
        {
            AllowDangerouslySetContent = true
        });

    var result = await prompt.InvokeAsync(kernel, new KernelArguments
    {
        ["input"] = HttpUtility.HtmlEncode(userInput),
    });

    Console.WriteLine($"Assistant > {result}");

    //history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is { Length: > 0 });