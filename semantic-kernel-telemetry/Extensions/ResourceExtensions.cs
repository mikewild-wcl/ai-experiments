using System.Reflection;

namespace semantic_kernel_telemetry.Extensions;

public static class ResourceExtensions
{
    public static string ReadManifestResourceStreamAsString(this Type type, string resourcePath)
    {
        //var qualifiedPath = string.IsNullOrEmpty(type.Namespace) 
        //    ? resourcePath
        //    : $"{type.Namespace}.{resourcePath}";
        //return type.Assembly.ReadManifestResourceStreamAsString(qualifiedPath);
        return type.Assembly.ReadManifestResourceStreamAsString(resourcePath);
    }

    public static string ReadManifestResourceStreamAsString(this Assembly assembly, string resourcePath)
    {
        var qualifiedPath = $"{assembly.GetName().Name.Replace('-', '_')}.{resourcePath}";
        //semantic-kernel-telemetry.prompt.yaml
        //semantic_kernel_telemetry.prompt.yaml
        var resourceNames = assembly.GetManifestResourceNames();
        foreach (var name in resourceNames)
        {
            Console.WriteLine(name);
        }

        //using var stream = assembly.GetManifestResourceStream(qualifiedPath);
        using var stream = assembly.GetManifestResourceStream(qualifiedPath);

        if (stream == null)
        {
            throw new InvalidOperationException($"Stream for '{qualifiedPath}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
