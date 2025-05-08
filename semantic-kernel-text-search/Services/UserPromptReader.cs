using semantic_kernel_text_search.Services.Interfaces;

namespace semantic_kernel_text_search.Services;

public class UserPromptReader : IUserPromptReader
{
    public UserPromptReader()
    {
    }

    public async IAsyncEnumerable<string> ProcessDocument(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            yield break;
        }

        var lines = prompt.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            await Task.Delay(50); // Simulate async work
            yield return line;
        }
    }
}
