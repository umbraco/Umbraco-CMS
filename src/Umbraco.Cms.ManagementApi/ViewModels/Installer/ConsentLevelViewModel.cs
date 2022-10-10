using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class ConsentLevelViewModel
{
    public TelemetryLevel Level { get; set; }

    public string Description { get; set; } = string.Empty;
}
