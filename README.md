# Experiments and tutorials

## Environment variables

Tokens have been saved to environment variables:
- Gemini `GEMINI_API_KEY`
- GitHub Models `GITHUB_TOKEN`

The endpoint for GitHub Models is https://models.inference.ai.azure.com


## Projects

### document_loader

A console app for loading documents and splitting into chunks. 
This project will take a document path and try to split them, handling
pdf, Word docx, and web pages.


### semantic-kernel-console

A basic example of a semantic kernel app, based on https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide?pivots=programming-language-csharp#writing-your-first-console-app

The sample uses GitHub Models


### semantic-kernel-console-gemini

A basic example of a semantic kernel app, based on https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide?pivots=programming-language-csharp#writing-your-first-console-app

The sample has been modified to use Gemini models.


### semantic-kernel-template-chat

A sample generated from the Microsoft.Extensions.AI.Templates. 
It uses the defaults of GitHub Models and an on-disk memory vector store .

The token has been copied into user secrets, as described in the README.md.

The project was upgraded to .NET 10.0 (preview 2). There was one breaking change
 - *error `The call is ambiguous between the following methods or properties: 'System.Linq.AsyncEnumerable.ToAsyncEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)' and 'System.Linq.AsyncEnumerable.ToAsyncEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)'
 - fix was to remove this package:
	```
	<PackageReference Include="System.Linq.Async" Version="6.0.1" />
	```

	- 
## Visual Studio templates

Install the AI templates for Visual Studio 2022:
```
dotnet new install Microsoft.Extensions.AI.Templates
```


## Links

- [Breaking change in IAsysncEmumerable](https://eur06.safelinks.protection.outlook.com/?url=https%3A%2F%2Flearn.microsoft.com%2Fen-us%2Fdotnet%2Fcore%2Fcompatibility%2Fcore-libraries%2F10.0%2Fasyncenumerable&data=05%7C02%7Cmichael.wild.external%40eviden.com%7C758fe62bc81f4ab84fe808dd6efdec6d%7C7d1c77852d8a437db8421ed5d8fbe00a%7C0%7C0%7C638788759054321521%7CUnknown%7CTWFpbGZsb3d8eyJFbXB0eU1hcGkiOnRydWUsIlYiOiIwLjAuMDAwMCIsIlAiOiJXaW4zMiIsIkFOIjoiTWFpbCIsIldUIjoyfQ%3D%3D%7C0%7C%7C%7C&sdata=tFOuNNp0qSNRcyecb%2F3RiaC1WOrFoZgFAMYH6Z5knXw%3D&reserved=0)
- [Semantic Kernel Quick start](https://eur06.safelinks.protection.outlook.com/?url=https%3A%2F%2Flearn.microsoft.com%2Fen-us%2Fsemantic-kernel%2Fget-started%2Fquick-start-guide%3Fpivots%3Dprogramming-language-csharp%23understanding-the-code&data=05%7C02%7Cmichael.wild.external%40eviden.com%7C5b8b6bbd559a4f05110708dd6eff96e9%7C7d1c77852d8a437db8421ed5d8fbe00a%7C0%7C0%7C638788766230012636%7CUnknown%7CTWFpbGZsb3d8eyJFbXB0eU1hcGkiOnRydWUsIlYiOiIwLjAuMDAwMCIsIlAiOiJXaW4zMiIsIkFOIjoiTWFpbCIsIldUIjoyfQ%3D%3D%7C0%7C%7C%7C&sdata=oynJGWZ8PVV4v0uoJEvG5bmCZA9Cra%2BYjGcbqjiT53o%3D&reserved=0)
- [AI Samples](https://github.com/dotnet/ai-samples)


### MCP

- [Simplifying MCP Server Development with Aspire](https://nikiforovall.github.io/dotnet/2025/04/04/mcp-template-and-aspire.html)
- [SSE-Powered MCP Server with C#](https://laurentkempe.com/2025/04/05/sse-powered-mcp-server-with-csharp-and-dotnet-in-157mb-executable/)
- [Layered System - MCPs with auth](https://www.layered.dev/openai-embraces-mcp-the-protocol-era-of-ai-has-arrived)

