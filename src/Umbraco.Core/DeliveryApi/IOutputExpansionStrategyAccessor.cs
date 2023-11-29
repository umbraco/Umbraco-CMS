using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IOutputExpansionStrategyAccessor
{
    bool TryGetValue([NotNullWhen(true)] out IOutputExpansionStrategy? outputExpansionStrategy);
}
