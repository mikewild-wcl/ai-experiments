using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

//const string ApiKeyName = "GEMINI_API_KEY";
const string ApiKeyName = "GEMINI_PAID_API_KEY";
const string ModelKeyName = "GEMINI_MODEL";

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

//var apiKey = configuration.GetValue<string>(ApiKeyName) ?? throw new InvalidOperationException("GEMINI_API_KEY is not set.");
var apiKey = configuration[ApiKeyName]
      ?? throw new InvalidOperationException($"{ApiKeyName} is not set. Add it via user secrets: dotnet user-secrets set \"{ApiKeyName}\" \"your_key_here\"");
var model = configuration[ModelKeyName] 
    ?? "gemini-3-flash-preview";
//var model = "gemini-2.5-flash-lite-preview-09-2025";

//if (string.IsNullOrEmpty(apiKey))
//{
//    throw new InvalidOperationException($"{apiKeyName} is not set. Add it via user secrets: dotnet user-secrets set \"{apiKeyName}\" \"your_key_here\"");
//}

Client client = new(apiKey: apiKey);
var chatClient = client.AsIChatClient(model);

var question = "What is the opening times of Hard Rock cafe in New York. And tell me if they have wheelchair access";

ChatClientAgent agent = new(chatClient,
    new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = _ => new GenerateContentConfig
            {
                Tools =
                [
                    new Tool
                    {
                        GoogleMaps = new GoogleMaps
                        {
                            EnableWidget = true
                        }
                    }
                ]
            }
        }
    });

var m = new ChatMessage(ChatRole.User, question);
//var m = agent.RunAsync(new ChatMessage(ChatRole.User, question), session, options, cancellationToken);

var response = await agent.RunAsync(question);

Console.WriteLine(response);
//response.Usage.OutputAsInformation();
Console.WriteLine($"- Input Tokens: {response?.Usage?.InputTokenCount}");
Console.WriteLine($"- Output Tokens: {response?.Usage?.OutputTokenCount} " + $"({response?.Usage?.ReasoningTokenCount ?? 0} was used for reasoning)");

/* Maps sample:
Utils.Init("Google Gemini (Google Maps)");
   Secrets secrets = SecretsManager.GetSecrets();
   Client client = new(apiKey: secrets.GoogleGeminiApiKey);
   IChatClient iChatClient = client.AsIChatClient("gemini-3-flash-preview");
   
   string question = "What is the opening times of Hard Rock cafe in New York. And tell me if they have wheelchair access";
   
   ChatClientAgent agent = new(iChatClient,
       new ChatClientAgentOptions
       {
           ChatOptions = new ChatOptions
           {
               RawRepresentationFactory = _ => new GenerateContentConfig
               {
                   Tools =
                   [
                       new Tool
                       {
                           GoogleMaps = new GoogleMaps
                           {
                               EnableWidget = true
                           }
                       }
                   ]
               }
           }
       });
   
   AgentResponse response = await agent.RunAsync(question);
   
   Console.WriteLine(response);
   response.Usage.OutputAsInformation();
   
   //More detailed data
   if (response.RawRepresentation is ChatResponse { RawRepresentation: GenerateContentResponse generateContentResponse })
   {
       foreach (Candidate candidate in generateContentResponse.Candidates ?? [])
       {
           GroundingMetadata? groundingMetadata = candidate.GroundingMetadata;
           if (groundingMetadata != null)
           {
               //Widget can be displayed using Google Maps Javascript API
               //https://developers.google.com/maps/documentation/javascript/load-maps-js-api
               string? widget = groundingMetadata.GoogleMapsWidgetContextToken;
   
               //Grounding data
               foreach (GroundingChunk chunk in groundingMetadata.GroundingChunks ?? [])
               {
                   if (chunk.Maps != null)
                   {
                       Utils.Yellow("- URL: " + chunk.Maps.Uri);
                       Utils.Yellow("- Title: " + chunk.Maps.Title);
                       Utils.Yellow("- Text: " + Environment.NewLine + chunk.Maps.Text);
                   }
               }
   
           }
       }
   }
   
   //Maps Cost: 5000 prompts per month (free), then $14 / 1000 queries 
*/

/* Search sample:
using Google.GenAI;
   using Google.GenAI.Types;
   using Microsoft.Agents.AI;
   using Microsoft.Extensions.AI;
   using Shared;
   using Shared.Extensions;
   
   Utils.Init("Google Gemini (WebSearch)");
   Secrets secrets = SecretsManager.GetSecrets();
   Client client = new(apiKey: secrets.GoogleGeminiApiKey);
   IChatClient iChatClient = client.AsIChatClient("gemini-3-flash-preview");
   
   string question = "What is today's Space news? (Show today's date + Answer in max 20 words + a link)";
   
   Utils.Green("No Web Search Tool");
   ChatClientAgent normalAgent = new(iChatClient);
   AgentResponse response1 = await normalAgent.RunAsync(question);
   Console.WriteLine(response1);
   response1.Usage.OutputAsInformation();
   
   Utils.Separator();
   
   Utils.Green("Web Search Tool (Easy)");
   ChatClientAgent webSearchAgent = new(iChatClient, tools: [new HostedWebSearchTool()]);
   AgentResponse response2 = await webSearchAgent.RunAsync(question);
   Console.WriteLine(response2);
   response2.Usage.OutputAsInformation();
   
   Utils.Separator();
   
   Utils.Green("Web Search Tool (Advanced)");
   ChatClientAgent webSearchAdvancedAgent = new(
       iChatClient,
       new ChatClientAgentOptions
       {
           ChatOptions = new ChatOptions
           {
               RawRepresentationFactory = _ => new GenerateContentConfig
               {
                   Tools =
                   [
                       new Tool
                       {
                           GoogleSearch = new GoogleSearch
                           {
                               SearchTypes = new SearchTypes
                               {
                                   WebSearch = new WebSearch()
                               }
                           }
                       }
                   ]
               }
           }
       });
   
   AgentResponse response3 = await webSearchAdvancedAgent.RunAsync(question);
   Console.WriteLine(response3);
   response3.Usage.OutputAsInformation();
   
   //Web Search Cost: 5000 prompts per month (free), then $14 / 1000 search queries
   
 */
