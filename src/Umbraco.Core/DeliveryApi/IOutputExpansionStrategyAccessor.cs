using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines an accessor for retrieving the current <see cref="IOutputExpansionStrategy"/>.
/// </summary>
public interface IOutputExpansionStrategyAccessor
{
    /// <summary>
    ///     Attempts to get the current output expansion strategy.
    /// </summary>
    /// <param name="outputExpansionStrategy">When this method returns, contains the output expansion strategy if available; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the output expansion strategy was retrieved; otherwise, <c>false</c>.</returns>
    bool TryGetValue([NotNullWhen(true)] out IOutputExpansionStrategy? outputExpansionStrategy);
}
