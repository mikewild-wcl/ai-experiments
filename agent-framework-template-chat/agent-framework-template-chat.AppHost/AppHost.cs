var builder = DistributedApplication.CreateBuilder(args);

// See https://learn.microsoft.com/dotnet/aspire/azure/local-provisioning#configuration
// for instructions providing configuration values
var openai = builder.AddAzureOpenAI("openai");

openai.AddDeployment(
    name: "gpt-4o-mini",
    modelName: "gpt-4o-mini",
    modelVersion: "2024-07-18");

openai.AddDeployment(
    name: "text-embedding-3-small",
    modelName: "text-embedding-3-small",
    modelVersion: "1");

var webApp = builder.AddProject<Projects.agent_framework_template_chat_Web>("aichatweb-app");
webApp
    .WithReference(openai)
    .WaitFor(openai);

builder.Build().Run();
