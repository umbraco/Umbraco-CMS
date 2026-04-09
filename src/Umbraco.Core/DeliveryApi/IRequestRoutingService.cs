namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a service that handles routing for Delivery API requests.
/// </summary>
public interface IRequestRoutingService
{
    /// <summary>
    ///     Retrieves the actual route for content in the content cache from a requested content path
    /// </summary>
    string GetContentRoute(string requestedPath);
}
