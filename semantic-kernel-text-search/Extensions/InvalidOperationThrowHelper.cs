namespace semantic_kernel_console_gemini;

public class InvalidOperationThrowHelper : InvalidOperationException
{
    public static void ThrowIfNullOrEmpty(string? val, string message = "Value must not be null or empty.")
    {
        if (string.IsNullOrEmpty(val))
        {
            throw new InvalidOperationException(message);
        }
    }
}
