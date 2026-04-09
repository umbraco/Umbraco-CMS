using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines an accessor for retrieving the current <see cref="IRequestStartItemProvider"/>.
/// </summary>
public interface IRequestStartItemProviderAccessor
{
    /// <summary>
    ///     Attempts to get the current request start item provider.
    /// </summary>
    /// <param name="requestStartItemProvider">When this method returns, contains the request start item provider if available; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the request start item provider was retrieved; otherwise, <c>false</c>.</returns>
    bool TryGetValue([NotNullWhen(true)] out IRequestStartItemProvider? requestStartItemProvider);
}
