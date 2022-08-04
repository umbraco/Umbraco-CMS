using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer;

public interface IInstallStep
{
    Task ExecuteAsync(InstallData model);

    Task<bool> RequiresExecutionAsync(InstallData model);
}
