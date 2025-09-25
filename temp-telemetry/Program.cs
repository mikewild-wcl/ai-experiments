// See https://aka.ms/new-console-template for more information
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using temp_telemetry;

Console.WriteLine("Hello, World!");

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("getting-started"))
    .AddSource("MyCompany.MyProduct.MyLibrary")
    .AddConsoleExporter()
    .Build();

using (var activity = ActivitySources.MyActivitySource.StartActivity("SayHello"))
{
    // Set some attributes on the activity
    activity?.SetTag("foo", 1);
    activity?.SetTag("bar", "Hello, World!");
    activity?.SetTag("baz", new int[] { 1, 2, 3 });

    // Set the status of the activity
    activity?.SetStatus(ActivityStatusCode.Ok);

    // Do some work...
    Console.WriteLine("Hello World!");
}

Console.WriteLine("Trace has been exported. Press any key to exit.");
Console.ReadKey();
