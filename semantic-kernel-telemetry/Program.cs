using Azure.AI.OpenAI;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using semantic_kernel_telemetry.Extensions;
using System.ClientModel;
using System.Web;

const int MaxHistoryMessages = 5;
var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .Build();

/*
 * https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/docs/metrics/getting-started-console/README.md
 * https://opentelemetry.io/docs/languages/dotnet/traces/getting-started-console/
 */
var telemetryResourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("AI_Experiments.SemanticKernel.Telemetry");

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(telemetryResourceBuilder)
    .AddSource("Microsoft.SemanticKernel*")
    .AddSource("semantic-kernel-telemetry")
    .AddConsoleExporter()
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(telemetryResourceBuilder)
    .AddMeter("Microsoft.SemanticKernel*")
    .AddConsoleExporter()
    .Build();

// https://bartwullems.blogspot.com/2024/06/semantic-kernelopentelemetry.html
// https://github.com/microsoft/semantic-kernel/blob/main/dotnet/docs/TELEMETRY.md
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        var addConsole = configuration.GetValue<bool?>("AddConsoleMonitoring") ?? false;
        if (addConsole)
        {
            options.AddConsoleExporter();
        }

        var azureMonitorConnectionString = configuration.GetConnectionString("AzureMonitoringConnectionString");
        if (!string.IsNullOrWhiteSpace(azureMonitorConnectionString))
        {
            options.AddAzureMonitorLogExporter(opt => opt.ConnectionString = azureMonitorConnectionString);
        }

        options.IncludeFormattedMessage = true;
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    })
    .SetMinimumLevel(LogLevel.Debug)
    .AddFilter("Microsoft", LogLevel.Warning)
    .AddFilter("Microsoft.SemanticKernel", LogLevel.Information);
});

var deployment = configuration.GetValue<string>("AzureOpenAiSettings:DeploymentName");
var endpoint = configuration.GetValue<string>("AzureOpenAiSettings:Endpoint");
var apiKey = configuration.GetValue<string>("AzureOpenAiSettings:ApiKey");
var service = configuration.GetValue<string>("AzureOpenAiSettings:Service");

var client = new AzureOpenAIClient(new Uri(endpoint!), new ApiKeyCredential(apiKey!));

var builder = Kernel
    .CreateBuilder()
    .AddAzureOpenAIChatCompletion(deployment!, client, serviceId: service);

builder.Services.AddSingleton(loggerFactory);

var kernel = builder.Build();

var history = new ChatHistory();

//var serviceProvider = builder.Services.BuildServiceProvider();
// {Method = {Microsoft.SemanticKernel.Connectors.AzureOpenAI.AzureOpenAIChatCompletionService <AddAzureOpenAIChatCompletion>b__0(System.IServiceProvider, System.Object)}}
var chatCompletionService = kernel.Services.GetKeyedService<IChatCompletionService>(service);

var reducer = new ChatHistorySummarizationReducer(chatCompletionService, MaxHistoryMessages);

string? userInput;
do
{
    Console.Write("Give me an idea for a joke: ");
    userInput = Console.ReadLine();

    if (userInput is null or { Length: 0 })
    {
        continue;
    }

    if (history.Count > MaxHistoryMessages)
    {
        Console.WriteLine("Summarizing chat history...");
    }

    history = await history.ReduceAsync(reducer, default);
    history.AddUserMessage(userInput);

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

    //TODO: You can use a chat history with "Kernel.InvokePrompt" if you use the ChatPrompt meta language as shown in this sample
    // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/PromptTemplates/HandlebarsPrompts.cs
    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/chat-history?pivots=programming-language-csharp

    var result = await prompt.InvokeAsync(kernel, new KernelArguments
    {
        ["input"] = HttpUtility.HtmlEncode(userInput),
        //["history"] { Name = "history", AllowDangerouslySetContent = true },
        ["history"] = history.Select(h => new
        {
            role = h.Role,
            content = HttpUtility.HtmlEncode(h.Content)
        }),
        //["history"] = new []
        //{
        //    new { role = "user", content = HttpUtility.HtmlEncode("pepperoni"),
        //    HttpUtility.HtmlEncode("mozzarella"),
        //    HttpUtility.HtmlEncode("spinach"),
        //},
        //{
        //    "history",
        //    new[]
        //        {
        //            new { role = "user", content = "What is my current membership level?" },
        //        }
        //},
    });

    Console.WriteLine($"Assistant > {result}");

    history.AddMessage(AuthorRole.Assistant, result?.ToString() ?? string.Empty);

} while (userInput is { Length: > 0 });