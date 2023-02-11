using Umbraco.Cms.Core.Composing;

namespace Umbraco.New.Cms.Core.Installer;

public class UpgradeStepCollection : BuilderCollectionBase<IUpgradeStep>
{
    public UpgradeStepCollection(Func<IEnumerable<IUpgradeStep>> items)
        : base(items)
    {
    }
}
