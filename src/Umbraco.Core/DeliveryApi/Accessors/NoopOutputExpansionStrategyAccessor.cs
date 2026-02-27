using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DeliveryApi.Accessors;

/// <summary>
///     A no-operation implementation of <see cref="IOutputExpansionStrategyAccessor"/> that always returns a <see cref="NoopOutputExpansionStrategy"/>.
/// </summary>
public sealed class NoopOutputExpansionStrategyAccessor : IOutputExpansionStrategyAccessor
{
    /// <inheritdoc />
    public bool TryGetValue([NotNullWhen(true)] out IOutputExpansionStrategy? outputExpansionStrategy)
    {
        outputExpansionStrategy = new NoopOutputExpansionStrategy();
        return true;
    }
}
