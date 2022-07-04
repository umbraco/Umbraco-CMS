using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Install.NewModels;

namespace Umbraco.Cms.Core.Install;

public class NewInstallStepCollection : BuilderCollectionBase<NewInstallSetupStep>
{
    public NewInstallStepCollection(Func<IEnumerable<NewInstallSetupStep>> items)
        : base(items)
    {
    }

    public IEnumerable<NewInstallSetupStep> GetInstallSteps()
        => this.Where(x => x.InstallationTypeTarget.HasFlag(InstallationType.NewInstall));

    public IEnumerable<NewInstallSetupStep> GetUpgradeSteps()
        => this.Where(x => x.InstallationTypeTarget.HasFlag(InstallationType.Upgrade));
}
