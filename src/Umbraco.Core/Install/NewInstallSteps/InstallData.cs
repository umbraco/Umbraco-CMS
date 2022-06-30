using Umbraco.Cms.Core.Install.NewModels;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Install.NewInstallSteps;

public class InstallData
{
    public UserInstallData User { get; set; } = null!;

    public DatabaseInstallData Database { get; set; } = null!;

    public TelemetryLevel TelemetryLevel { get; set; } = TelemetryLevel.Basic;
}
