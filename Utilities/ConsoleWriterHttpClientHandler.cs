/*
 * Adapted from https://github.com/rwjdk/MicrosoftAgentFrameworkSamples
 */

namespace Utilities;

public class ConsoleWriterHttpClientHandler() : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
        ConsoleUtils.WriteLineGreen($"Raw Request ({request.RequestUri})");
        ConsoleUtils.WriteLineDarkGray(ConsoleUtils.Prettify(requestString));
        ConsoleUtils.Separator();
        var response = await base.SendAsync(request, cancellationToken);

        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        ConsoleUtils.WriteLineGreen("Raw Response");
        ConsoleUtils.WriteLineDarkGray(ConsoleUtils.Prettify(responseString));
        ConsoleUtils.Separator();
        return response;
    }
}
