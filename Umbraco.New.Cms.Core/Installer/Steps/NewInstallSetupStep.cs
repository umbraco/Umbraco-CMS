using Umbraco.Cms.Core.Install.Models;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer.Steps;

public abstract class NewInstallSetupStep
{
    public NewInstallSetupStep(string name, int order, InstallationType target)
    {
        Name = name;
        Order = order;
        InstallationTypeTarget = target;
    }

    public string Name { get; }

    public int Order { get; }

    public InstallationType InstallationTypeTarget { get; }

    public abstract Task ExecuteAsync(InstallData model);

    public abstract Task<bool> RequiresExecutionAsync(InstallData model);
}
