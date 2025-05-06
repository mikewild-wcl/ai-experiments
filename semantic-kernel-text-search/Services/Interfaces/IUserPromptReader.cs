namespace semantic_kernel_text_search.Services.Interfaces;

public interface IUserPromptReader
{
    IAsyncEnumerable<string> ProcessDocument(string? prompt);
}
