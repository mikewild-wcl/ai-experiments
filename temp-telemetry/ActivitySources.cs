using System.Diagnostics;

namespace temp_telemetry;

internal static class ActivitySources
{
    public static readonly ActivitySource MyActivitySource = new(
                "MyCompany.MyProduct.MyLibrary");
}
