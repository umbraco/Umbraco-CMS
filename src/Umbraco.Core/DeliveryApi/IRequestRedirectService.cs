using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a service that handles redirects for Delivery API requests.
/// </summary>
public interface IRequestRedirectService
{
    /// <summary>
    ///     Retrieves the redirect URL (if any) for a requested content path
    /// </summary>
    IApiContentRoute? GetRedirectRoute(string requestedPath);
}
