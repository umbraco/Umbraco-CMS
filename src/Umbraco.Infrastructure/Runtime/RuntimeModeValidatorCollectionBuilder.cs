using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <summary>
/// Provides functionality to build and manage a collection of runtime mode validators in Umbraco.
/// </summary>
public class RuntimeModeValidatorCollectionBuilder : SetCollectionBuilderBase<RuntimeModeValidatorCollectionBuilder, RuntimeModeValidatorCollection, IRuntimeModeValidator>
{
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient;

    protected override RuntimeModeValidatorCollectionBuilder This => this;
}
