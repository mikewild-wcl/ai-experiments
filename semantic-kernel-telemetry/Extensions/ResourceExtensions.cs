using System.Reflection;

namespace semantic_kernel_telemetry.Extensions;

public static class ResourceExtensions
{
    public static string ReadManifestResourceStreamAsString(this Type type, string resourcePath)
    {
        return type.Assembly.ReadManifestResourceStreamAsString(resourcePath);
    }

    public static string ReadManifestResourceStreamAsString(this Assembly assembly, string resourcePath)
    {
        var qualifiedPath = $"{assembly.GetName().Name.Replace('-', '_')}.{resourcePath}";
        var resourceNames = assembly.GetManifestResourceNames();

        using var stream = assembly.GetManifestResourceStream(qualifiedPath);

        if (stream == null)
        {
            throw new InvalidOperationException($"Stream for '{qualifiedPath}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
