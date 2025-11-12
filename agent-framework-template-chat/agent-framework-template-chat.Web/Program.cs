using agent_framework_template_chat.Web.Components;
using agent_framework_template_chat.Web.Services;
using agent_framework_template_chat.Web.Services.Ingestion;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var openai = builder.AddAzureOpenAIClient("openai");
openai.AddChatClient("gpt-4o-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
openai.AddEmbeddingGenerator("text-embedding-3-small");

// Register the AI Agent using the Agent Framework
builder.AddAIAgent("ChatAgent", (sp, key) =>
{
    // Get required services
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Configuring AI Agent with key '{Key}' for model '{Model}'", key, "gpt-4o-mini");

    var searchFunctions = sp.GetRequiredService<SearchFunctions>();
    var weatherFunctions = sp.GetRequiredService<WeatherFunctions>();
    var chatClient = sp.GetRequiredService<IChatClient>();

    // Create and configure the AI agent
    var aiAgent = chatClient.CreateAIAgent(
        name: key,
        instructions: "You are a useful agent that helps users with short and funny answers.",
        description: "An AI agent that helps users with short and funny answers.",
        tools: [
            AIFunctionFactory.Create(searchFunctions.SearchAsync),
            AIFunctionFactory.Create(weatherFunctions.GetWeatherAsync)
            ]
        )
    .AsBuilder()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment())
    .Build();

    return aiAgent;
});

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteCollection<string, IngestedChunk>("data-agent-framework-template-chat-chunks", vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedDocument>("data-agent-framework-template-chat-documents", vectorStoreConnectionString);
builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddSingleton<WeatherFunctions>();

// Register SearchFunctions for DI injection into the agent
builder.Services.AddSingleton<SearchFunctions>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

await app.RunAsync();
