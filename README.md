# Experiments and tutorials

## Environment variables

Tokens have been saved to environment variables:
- Gemini `GEMINI_API_KEY`
- GitHub Models `GITHUB_TOKEN`

To set the environment variables, use the following commands
```
setx GEMINI_API_KEY <key>
setx GITHUB_TOKEN <key>
```

The endpoint for GitHub Models is https://models.inference.ai.azure.com

Note that some projects need `DOTNET_ENVIRONMENT` defined as `Development` in their DEBUG properties.


## Projects

### document_loader

A console app for loading documents and splitting into chunks. 
This project will take a document path and try to split them, handling
pdf, Word docx, and web pages.

### mcp-simple-server

A simple server that implements the MCP protocol, using modelcontextprotocol/csharp-sdk.

Based on https://laurentkempe.com/2025/03/22/model-context-protocol-made-easy-building-an-mcp-server-in-csharp/


### semantic-kernel-azure-sql-vectors

A console app that uses the Semantic Kernel to load documents 
and save the embeddings to Azure SQL using the Azure SQL Connector.

Uses Azure OpenAI - 
	- create an OpenAI resource in the Azure portal
	- to create models, you need to assign the Cognitive Services Usages Reader role to your user account or service principal.
	  See https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/quota?tabs=rest#prerequisites
		- under Subscriptions > Access control (IAM) > Add role assignment > search for Cognitive Services Usages Reader and assign your user

### semantic-kernel-console

A basic example of a semantic kernel app, based on https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide?pivots=programming-language-csharp#writing-your-first-console-app

The sample uses GitHub Models.


### semantic-kernel-console-gemini

A basic example of a semantic kernel app, based on https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide?pivots=programming-language-csharp#writing-your-first-console-app

The sample has been modified to use Gemini models.


### semantic-kernel-azure-openai-managed-id-auth

A console app that authenticates using Azure managed identity.


### semantic-kernel-template-chat

A sample generated from the Microsoft.Extensions.AI.Templates. 
It uses the defaults of GitHub Models and an on-disk memory vector store .

The token has been copied into user secrets, as described in the README.md.


### semantic-kernel-text-search

A console that loads documents and performs searches using SK text search.


### test-data

A simple web site project where test files and pages can be served. 
Add files or save html pages into the wwwroot/Data folder - they will not be checked in to source control.


### unit-test-playground

A place to write simple unit tests for things like extensions, without needing to reference another project.

	 
## Visual Studio templates

Install the AI templates for Visual Studio 2022:
```
dotnet new install Microsoft.Extensions.AI.Templates
```


## Links

- [Breaking change in IAsyncEnumerable](https://eur06.safelinks.protection.outlook.com/?url=https%3A%2F%2Flearn.microsoft.com%2Fen-us%2Fdotnet%2Fcore%2Fcompatibility%2Fcore-libraries%2F10.0%2Fasyncenumerable&data=05%7C02%7Cmichael.wild.external%40eviden.com%7C758fe62bc81f4ab84fe808dd6efdec6d%7C7d1c77852d8a437db8421ed5d8fbe00a%7C0%7C0%7C638788759054321521%7CUnknown%7CTWFpbGZsb3d8eyJFbXB0eU1hcGkiOnRydWUsIlYiOiIwLjAuMDAwMCIsIlAiOiJXaW4zMiIsIkFOIjoiTWFpbCIsIldUIjoyfQ%3D%3D%7C0%7C%7C%7C&sdata=tFOuNNp0qSNRcyecb%2F3RiaC1WOrFoZgFAMYH6Z5knXw%3D&reserved=0)
- [Semantic Kernel Quick start](https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide?pivots=programming-language-csharp#understanding-the-code)
- [AI Samples](https://github.com/dotnet/ai-samples)


### MCP

- [Simplifying MCP Server Development with Aspire](https://nikiforovall.github.io/dotnet/2025/04/04/mcp-template-and-aspire.html)
- [SSE-Powered MCP Server with C#](https://laurentkempe.com/2025/04/05/sse-powered-mcp-server-with-csharp-and-dotnet-in-157mb-executable/)
- [Layered System - MCPs with auth](https://www.layered.dev/openai-embraces-mcp-the-protocol-era-of-ai-has-arrived)


## Breaking changes in .NET 10

Breaking changes wwere seen when upgrading projects to .NET 10.

 - **error** `The call is ambiguous between the following methods or properties: 'System.Linq.AsyncEnumerable.ToAsyncEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)' and 'System.Linq.AsyncEnumerable.ToAsyncEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)'
 - fix was to remove this package:
	```
	<PackageReference Include="System.Linq.Async" Version="6.0.1" />
	```

## Breaking changes in Semantic Kernel

A number of changes were made in Semantic Kernel and needed to be fixed See:
 - [Vector Store changes - April 2025](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/vectorstore-april-2025?pivots=programming-language-csharp)
 - [Vector Store changes - May 2025](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/vectorstore-may-2025?pivots=programming-language-csharp)
 