namespace Umbraco.Cms.Core.ContentApi;

public interface IRequestRoutingService
{
    string GetContentRoute(string requestedPath);
}
