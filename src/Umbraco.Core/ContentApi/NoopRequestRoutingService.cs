namespace Umbraco.Cms.Core.ContentApi;

public sealed class NoopRequestRoutingService : IRequestRoutingService
{
    /// <inheritdoc />
    public string GetContentRoute(string requestedPath) => requestedPath;
}
