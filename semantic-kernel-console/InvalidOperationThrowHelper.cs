namespace semantic_kernel_console;

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
