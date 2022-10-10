using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class InstallViewModel
{
    [Required]
    public UserInstallViewModel User { get; set; } = null!;

    [Required]
    public DatabaseInstallViewModel Database { get; set; } = null!;

    public TelemetryLevel TelemetryLevel { get; set; } = TelemetryLevel.Basic;
}
