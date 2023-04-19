namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestRoutingService
{
    /// <summary>
    ///     Retrieves the actual route for content in the content cache from a requested content path
    /// </summary>
    string GetContentRoute(string requestedPath);
}
