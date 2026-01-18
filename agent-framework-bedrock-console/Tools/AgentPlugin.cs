using Microsoft.Extensions.AI;

namespace agent_framework_bedrock_console.Tools;

internal class AgentPlugin(LightsProvider lightsProvider)
{
    public List<LightModel> GetLights()
    {
        return lightsProvider.GetLights();
    }

    public LightModel? ChangeState(int id, bool isOn)
    {
        return lightsProvider.ChangeState(id, isOn);
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(this.GetLights);
        yield return AIFunctionFactory.Create(this.ChangeState);
    }
}
