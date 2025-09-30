using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace semantic_kernel_telemetry.Extensions;

internal static class TelemetryExtensions
{
    public const string AddConsoleMonitoringConfigKey = "AddConsoleMonitoring";
    public const string AzureMonitoringConnectionStringConfigKey = "AzureMonitoringConnectionString";

    public static string? GetAzureMonitoringConnectionString(this IConfiguration configuration)
        => configuration.GetConnectionString(AzureMonitoringConnectionStringConfigKey);

    public static bool IsConsoleMonitoringEnabled(this IConfiguration configuration) =>
         configuration?.GetValue<bool?>(AddConsoleMonitoringConfigKey) ?? false;

    public static MeterProviderBuilder AddMetricExporters(this MeterProviderBuilder builder, IConfiguration configuration)
    {
        if (configuration.IsConsoleMonitoringEnabled())
        {
            builder.AddConsoleExporter();
        }

        var azureMonitorConnectionString = configuration.GetAzureMonitoringConnectionString();
        if (!string.IsNullOrWhiteSpace(azureMonitorConnectionString))
        {
            builder.AddAzureMonitorMetricExporter(opt => opt.ConnectionString = azureMonitorConnectionString);
        }

        return builder;
    }

    public static TracerProviderBuilder AddTraceExporters(this TracerProviderBuilder builder, IConfiguration configuration)
    {
        if (configuration.IsConsoleMonitoringEnabled())
        {
            builder.AddConsoleExporter();
        }

        var azureMonitorConnectionString = configuration.GetAzureMonitoringConnectionString();
        if (!string.IsNullOrWhiteSpace(azureMonitorConnectionString))
        {
            builder.AddAzureMonitorTraceExporter(opt => opt.ConnectionString = azureMonitorConnectionString);
        }

        return builder;
    }

    public static OpenTelemetryLoggerOptions AddLogExporters(this OpenTelemetryLoggerOptions loggerOptions, IConfiguration configuration)
    {
        if (configuration.IsConsoleMonitoringEnabled())
        {
            return loggerOptions.AddConsoleExporter(null);
        }

        var azureMonitorConnectionString = configuration.GetAzureMonitoringConnectionString();
        if (!string.IsNullOrWhiteSpace(azureMonitorConnectionString))
        {
            loggerOptions.AddAzureMonitorLogExporter(opt => opt.ConnectionString = azureMonitorConnectionString);
        }

        return loggerOptions;
    }
}
