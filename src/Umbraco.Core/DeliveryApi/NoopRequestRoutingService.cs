namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopRequestRoutingService : IRequestRoutingService
{
    /// <inheritdoc />
    public string GetContentRoute(string requestedPath) => requestedPath;
}
