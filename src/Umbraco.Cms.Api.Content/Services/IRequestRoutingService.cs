namespace Umbraco.Cms.Api.Content.Services;

public interface IRequestRoutingService
{
    string GetContentRoute(string requestedPath);
}
