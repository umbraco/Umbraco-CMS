using Umbraco.Cms.Core.Install.Models;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer;

public interface IInstallStep
{
    InstallationType InstallationTypeTarget { get; }

    Task ExecuteAsync(InstallData model);

    Task<bool> RequiresExecutionAsync(InstallData model);
}
