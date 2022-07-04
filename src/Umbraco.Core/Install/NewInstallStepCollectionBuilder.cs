using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Install.NewModels;

namespace Umbraco.Cms.Core.Install;

public class NewInstallStepCollectionBuilder : OrderedCollectionBuilderBase<NewInstallStepCollectionBuilder, NewInstallStepCollection, NewInstallSetupStep>
{
    protected override NewInstallStepCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
