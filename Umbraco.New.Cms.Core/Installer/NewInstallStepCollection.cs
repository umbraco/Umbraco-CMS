using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.New.Cms.Core.Installer.Steps;

namespace Umbraco.New.Cms.Core.Installer;

public class NewInstallStepCollection : BuilderCollectionBase<InstallSetupStep>
{
    public NewInstallStepCollection(Func<IEnumerable<InstallSetupStep>> items)
        : base(items)
    {
    }

    public IEnumerable<InstallSetupStep> GetInstallSteps()
        => this.Where(x => x.InstallationTypeTarget.HasFlag(InstallationType.NewInstall));

    public IEnumerable<InstallSetupStep> GetUpgradeSteps()
        => this.Where(x => x.InstallationTypeTarget.HasFlag(InstallationType.Upgrade));
}
