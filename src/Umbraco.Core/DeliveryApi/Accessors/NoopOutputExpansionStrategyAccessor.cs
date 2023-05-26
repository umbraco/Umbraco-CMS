using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DeliveryApi.Accessors;

public sealed class NoopOutputExpansionStrategyAccessor : IOutputExpansionStrategyAccessor
{
    public bool TryGetValue([NotNullWhen(true)] out IOutputExpansionStrategy? outputExpansionStrategy)
    {
        outputExpansionStrategy = new NoopOutputExpansionStrategy();
        return true;
    }
}
