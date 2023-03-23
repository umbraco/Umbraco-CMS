namespace Umbraco.Cms.Core.ContentApi;

public class NoopRequestRoutingService : IRequestRoutingService
{
    public string GetContentRoute(string requestedPath) => requestedPath;
}
