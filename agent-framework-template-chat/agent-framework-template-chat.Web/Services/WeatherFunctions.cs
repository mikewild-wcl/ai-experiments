using System.ComponentModel;

namespace agent_framework_template_chat.Web.Services;

public class WeatherFunctions
{
    [Description("Gets the current weather for a location")]
    public async Task<string> GetWeatherAsync(
           [Description("The city and state/country")] string location)
    {
        // Call weather API
        return $"Weather for {location}: Sunny, 72°F";
    }
}

