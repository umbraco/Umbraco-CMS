namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IRequestRoutingService"/> that returns the requested path as-is.
/// </summary>
public sealed class NoopRequestRoutingService : IRequestRoutingService
{
    /// <inheritdoc />
    public string GetContentRoute(string requestedPath) => requestedPath;
}
