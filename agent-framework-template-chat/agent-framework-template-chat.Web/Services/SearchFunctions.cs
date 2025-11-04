using System.ComponentModel;

namespace agent_framework_template_chat.Web.Services;

public class SearchFunctions(SemanticSearch semanticSearch)
{
    private readonly SemanticSearch _semanticSearch = semanticSearch;

    [Description("Searches for information using a phrase or keyword")]
    public async Task<IEnumerable<string>> SearchAsync(
       [Description("The phrase to search for.")] string searchPhrase,
       [Description("If possible, specify the filename to search that file only. If not provided or empty, the search includes all files.")] string? filenameFilter = null)
    {
        // Perform semantic search over ingested chunks
        var results = await _semanticSearch.SearchAsync(searchPhrase, filenameFilter, maxResults: 5);

        // Format results as XML for the agent
        return results.Select(result =>
            $"<result filename=\"{result.DocumentId}\" page_number=\"{result.PageNumber}\">{result.Text}</result>");
    }
}

