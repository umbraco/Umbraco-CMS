using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.New.Cms.Core.Installer;

public class NewInstallStepCollectionBuilder : OrderedCollectionBuilderBase<NewInstallStepCollectionBuilder, NewInstallStepCollection, IInstallStep>
{
    protected override NewInstallStepCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
