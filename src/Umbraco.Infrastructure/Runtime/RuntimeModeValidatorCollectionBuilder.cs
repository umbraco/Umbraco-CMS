using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Runtime;

public class RuntimeModeValidatorCollectionBuilder : SetCollectionBuilderBase<RuntimeModeValidatorCollectionBuilder, RuntimeModeValidatorCollection, IRuntimeModeValidator>
{
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient;

    protected override RuntimeModeValidatorCollectionBuilder This => this;
}
