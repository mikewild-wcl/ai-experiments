namespace semantic_kernel_console_gemini;

public static class ThrowHelper
{
    public static void ThrowInvalidOperationException(string message)
    {
        throw new InvalidOperationException(message);
    }
}
