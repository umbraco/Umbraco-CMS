using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestRedirectService
{
    /// <summary>
    ///     Retrieves the redirect URL (if any) for a requested content path
    /// </summary>
    IApiContentRoute? GetRedirectRoute(string requestedPath);
}
