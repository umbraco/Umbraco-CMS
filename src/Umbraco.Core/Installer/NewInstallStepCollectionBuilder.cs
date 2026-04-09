using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Installer;

/// <summary>
/// A builder for creating a <see cref="NewInstallStepCollection"/> with ordered <see cref="IInstallStep"/> instances.
/// </summary>
public class NewInstallStepCollectionBuilder : OrderedCollectionBuilderBase<NewInstallStepCollectionBuilder, NewInstallStepCollection, IInstallStep>
{
    /// <inheritdoc />
    protected override NewInstallStepCollectionBuilder This => this;

    /// <inheritdoc />
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
