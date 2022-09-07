using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.ViewModels.Analytics;

public class AnalyticsViewModel
{
    [JsonPropertyName("telemetryLevel")]
    public TelemetryLevel TelemetryLevel { get; set; }
}
