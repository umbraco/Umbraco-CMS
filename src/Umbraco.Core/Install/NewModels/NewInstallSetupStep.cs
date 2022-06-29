using Umbraco.Cms.Core.Install.Models;

namespace Umbraco.Cms.Core.Install.NewModels;

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

    public abstract Task<bool> RequiresExecution(InstallData model);
}
