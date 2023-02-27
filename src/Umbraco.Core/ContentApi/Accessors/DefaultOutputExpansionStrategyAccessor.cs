using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi.Accessors;

public class DefaultOutputExpansionStrategyAccessor : IOutputExpansionStrategyAccessor
{
    public bool TryGetValue([NotNullWhen(true)] out IOutputExpansionStrategy? outputExpansionStrategy)
    {
        outputExpansionStrategy = new DefaultOutputExpansionStrategy();
        return true;
    }
}
