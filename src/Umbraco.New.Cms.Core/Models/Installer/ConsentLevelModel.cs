using Umbraco.Cms.Core.Models;

namespace Umbraco.New.Cms.Core.Models.Installer;

public class ConsentLevelModel
{
    public TelemetryLevel Level { get; set; }

    public string Description { get; set; } = string.Empty;
}
