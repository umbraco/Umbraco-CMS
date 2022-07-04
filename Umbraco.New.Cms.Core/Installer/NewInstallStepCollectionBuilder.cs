using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.New.Cms.Core.Installer.Steps;

namespace Umbraco.New.Cms.Core.Installer;

public class NewInstallStepCollectionBuilder : OrderedCollectionBuilderBase<NewInstallStepCollectionBuilder, NewInstallStepCollection, NewInstallSetupStep>
{
    protected override NewInstallStepCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
