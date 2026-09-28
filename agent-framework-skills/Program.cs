using Azure;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

//var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
//    ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
//    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
//var deployment = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
//    ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
//    ?? "gpt-5.4-nano";
//var apiKey = configuration["AZURE_OPENAI_API_KEY"]
//    ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
//    ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

var endpoint = configuration["AzureOpenAiSettings:Endpoint"] ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = configuration["AzureOpenAiSettings:DeploymentName"] ?? "gpt-5.4-nano";
var apiKey = configuration["AzureOpenAiSettings:ApiKey"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

/* OLD:
#pragma warning disable MAAI001
var skillsProvider = new AgentSkillsProvider(
#pragma warning restore MAAI001
    Path.Combine(AppContext.BaseDirectory, "skills"));
*/

/*NEW:
 From https://devblogs.microsoft.com/agent-framework/agent-skills-for-net-is-now-released/
 */
var skillsProvider = new AgentSkillsProvider(
    Path.Combine(AppContext.BaseDirectory, "skills"),
    options: new AgentSkillsProviderOptions
    {
        DisableLoadSkillApproval = true
    }
    //SubprocessScriptRunner.RunAsync
);
//TODO: See https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/02-agents/AgentSkills/SubprocessScriptRunner.cs

/*
var recipeWriterTool = AIFunctionFactory.Create(
    (string ingredients, string? dish) =>
    {
        var requestedDish = string.IsNullOrWhiteSpace(dish) ? "a practical meal" : dish;
        return $"""
            Recipe idea for {requestedDish} using the available ingredients:

            Ingredients:
            {ingredients}

            Suggested structure:
            - Recipe name
            - Short ingredient list
            - 3-5 simple steps
            - One optional tip
            """;
    },
    "recipe_writer",
    "Create a recipe from the user's ingredients and optional dish request.");
*/

//NEW:
// https://github.com/Azure/azure-sdk-for-net/issues/60689
/* Old code before migrating from Azure OpenAI client to OpenAI client:
AIAgent agent = new AzureOpenAIClient(
        new Uri(endpoint),
        //new DefaultAzureCredential())
        new ApiKeyCredential(apiKey))
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "MyAgent",
        //ChatOptions = new()
        //{
        //    Instructions = "You are a world-class chef. Use the recipe_writer tool whenever the user asks for a recipe based on ingredients.",
        //    Tools = [recipeWriterTool],
        //},
        ChatOptions = new()
        {
            Instructions = "You are a world-class chef. Use the available skills to help you write recipes."
        },
        AIContextProviders = [skillsProvider],
    },
        model: deployment)
    .AsBuilder()
#pragma warning disable MAAI001
    .UseToolApproval(new ToolApprovalAgentOptions
#pragma warning restore MAAI001
    {
        // Auto-approve read-only skill tools (load_skill, read_skill_resource).
        // run_skill_script will still require explicit user approval.
        AutoApprovalRules = [AgentSkillsProvider.ReadOnlyToolsAutoApprovalRule],
    })
    .Build();

//AIAgent agent = new AzureOpenAIClient(
//        new Uri(endpoint),
//        new ApiKeyCredential(apiKey))
//    .GetChatClient(deployment)
//    .AsAIAgent(new ChatClientAgentOptions
//    {
//        Name = "SkillsAgent",
//        ChatOptions = new()
//        {
//            Instructions = "You are a world-class chef.",
//        },
//        AIContextProviders = [skillsProvider],
//    });
*/

// Moved warning suppression to project file for OPENAI001 - type is for evaluation purposes only.
OpenAIClient openAIClient = new(
    new AzureKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(endpoint)
    });

//ChatClient chatClient = new(
// model: deployment,
// credential: new ApiKeyCredential(apiKey),
// options: options);

//AIAgent agent = new AzureOpenAIClient(
//        new Uri(endpoint),
//        //new DefaultAzureCredential())
//        new ApiKeyCredential(apiKey))
AIAgent agent = openAIClient
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "MyAgent",
        ChatOptions = new()
        {
            Instructions = "You are a world-class chef. Use the available skills to help you write recipes."
        },
        AIContextProviders = [skillsProvider],
    },
    model: deployment
    )
    .AsBuilder()
#pragma warning disable MAAI001
    .UseToolApproval(new ToolApprovalAgentOptions
#pragma warning restore MAAI001
    {
        // Auto-approve read-only skill tools (load_skill, read_skill_resource).
        // run_skill_script will still require explicit user approval.
        AutoApprovalRules = [AgentSkillsProvider.ReadOnlyToolsAutoApprovalRule],
    })
    .Build(); ;

var p = Path.Combine(AppContext.BaseDirectory, "skills");
Console.WriteLine($"Skills directory: {p}");
var di = new System.IO.DirectoryInfo(p);
foreach (var d in di.GetDirectories())
{
    Console.WriteLine($"  - {d.Name}");
    foreach (var f in d.GetFiles())
    {
        Console.WriteLine($"    - {f.Name}");
    }
}

Console.WriteLine("Available skills:");
AgentResponse skillListResponse = await agent.RunAsync("List all available skills.");
Console.WriteLine($"   {skillListResponse.Text}");
Console.WriteLine(new string('-', 50));

var userInput =
    """
    What can I have for dinner? I have the following ingredients: tomatoes, chicken, and rice.

    If you can't create a recipe with these ingredients, please suggest a different recipe that I can make with them.
    
    If you don't see any ingredients in my request, tell me that I haven't told you what I have.
    
    Always respond, even if it is just to say you cannot help.
    """;
//AgentResponse response = await agent.RunAsync(userInput);
//Console.WriteLine(response.Text);

//Console.WriteLine(new string('-', 50));

await foreach (var update in agent.RunStreamingAsync(userInput))
{
    Console.Write(update);
}

Console.WriteLine();
