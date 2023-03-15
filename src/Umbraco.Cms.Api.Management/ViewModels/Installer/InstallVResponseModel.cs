using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class InstallVResponseModel
{
    [Required]
    public UserInstallResponseModel User { get; set; } = null!;

    [Required]
    public DatabaseInstallResponseModel Database { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel TelemetryLevel { get; set; } = TelemetryLevel.Basic;
}
