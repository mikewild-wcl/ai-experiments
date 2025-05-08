namespace unit_test_playground.Extensions;

public static class FileRegexExtensions
{
    public static IEnumerable<string> ExtractFilePath(this string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Enumerable.Empty<string>();
        }

        return Enumerable.Empty<string>();
    }
}
