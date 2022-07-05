using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Install.Models;

namespace Umbraco.New.Cms.Core.Installer;

public class NewInstallStepCollection : BuilderCollectionBase<IInstallStep>
{
    public NewInstallStepCollection(Func<IEnumerable<IInstallStep>> items)
        : base(items)
    {
    }

    public IEnumerable<IInstallStep> GetInstallSteps()
        => this.Where(x => x.InstallationTypeTarget.HasFlag(InstallationType.NewInstall));

    public IEnumerable<IInstallStep> GetUpgradeSteps()
        => this.Where(x => x.InstallationTypeTarget.HasFlag(InstallationType.Upgrade));
}
