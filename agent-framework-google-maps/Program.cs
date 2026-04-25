using Microsoft.Extensions.Configuration;

const string ApiKeyName = "GEMINI_API_KEY";

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetValue<string>(ApiKeyName) ?? throw new InvalidOperationException("GEMINI_API_KEY is not set.");

Console.WriteLine("Hello, Google Maps Agent Framework!");

