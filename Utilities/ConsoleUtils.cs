/*
 * Adapted from https://github.com/rwjdk/MicrosoftAgentFrameworkSamples
 */
using System.Text.Json;

namespace Utilities;

public static class ConsoleUtils
{
    private static readonly JsonSerializerOptions DefaultSerializwerOptions = new()
    {
        WriteIndented = true
    };

    public static void WriteLineRed(Exception e)
    {
        WriteLineRed(e.ToString());
    }

    public static void WriteLineRed(string text)
    {
        WriteLine(text, ConsoleColor.Red);
    }

    public static void WriteLineYellow(string text)
    {
        WriteLine(text, ConsoleColor.Yellow);
    }

    public static void WriteLineDarkGray(string text)
    {
        WriteLine(text, ConsoleColor.DarkGray);
    }

    public static void WriteLineGreen(string text)
    {
        WriteLine(text, ConsoleColor.Green);
    }

    public static void WriteLine(string text, ConsoleColor color)
    {
        ConsoleColor orgColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        finally
        {
            Console.ForegroundColor = orgColor;
        }
    }

    public static string Prettify(string input)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
        return JsonSerializer.Serialize(jsonElement, DefaultSerializwerOptions);
    }

    public static void Separator()
    {
        Console.WriteLine();
        WriteLine("".PadLeft(Console.WindowWidth, '-'), ConsoleColor.Gray);
        Console.WriteLine();
    }
}