using System.ComponentModel;
using System.Text.Json.Serialization;

namespace agent_framework_bedrock_console.Tools;

internal class LightsProvider
{
    // Mock data for the lights
    private readonly List<LightModel> lights =
    [
        new LightModel { Id = 1, Name = "Table Lamp", IsOn = false },
        new LightModel { Id = 2, Name = "Porch light", IsOn = false },
        new LightModel { Id = 3, Name = "Chandelier", IsOn = true }
    ];

    [Description("Gets a list of lights and their current state")]
    public List<LightModel> GetLights()
    {
        return lights;
    }

    [Description("Changes the state of the light")]
    public LightModel? ChangeState(int id, bool isOn)
    {
        var light = lights.FirstOrDefault(light => light.Id == id);

        if (light == null)
        {
            return null;
        }

        // Update the light with the new state
        light.IsOn = isOn;

        return light;
    }
}

public record LightModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }
}