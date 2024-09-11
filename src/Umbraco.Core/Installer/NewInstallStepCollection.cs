using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Installer;

public class NewInstallStepCollection : BuilderCollectionBase<IInstallStep>
{
    public NewInstallStepCollection(Func<IEnumerable<IInstallStep>> items)
        : base(items)
    {
    }
}
