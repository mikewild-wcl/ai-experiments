namespace semantic_kernel_console;

public static class ThrowHelper
{
    public static void ThrowInvalidOperationException(string message)
    {
        throw new InvalidOperationException(message);
    }
}
