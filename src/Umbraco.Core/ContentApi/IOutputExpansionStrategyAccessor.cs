using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.ContentApi;

public interface IOutputExpansionStrategyAccessor
{
    bool TryGetValue([NotNullWhen(true)] out IOutputExpansionStrategy? outputExpansionStrategy);
}
