using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.ViewModels.Analytics;

public class AnalyticsLevelViewModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel AnalyticsLevel { get; set; }
}
