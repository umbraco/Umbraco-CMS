using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class InstallViewModel
{
    [Required]
    public UserInstallViewModel User { get; set; } = null!;

    [Required]
    public DatabaseInstallViewModel Database { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel TelemetryLevel { get; set; } = TelemetryLevel.Basic;
}
