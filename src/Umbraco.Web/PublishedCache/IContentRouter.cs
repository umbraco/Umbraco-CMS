namespace Umbraco.Web.PublishedCache
{
    public interface IContentRouter
    {
        ContentRoutingResult GetIdByRoute(bool preview, string route, bool? hideTopLevelNode, string culture);
        ContentRoutingResult GetIdByRoute(string route, bool? hideTopLevelNode, string culture);
        string GetRouteById(bool preview, int contentId, string culture = null);
        string GetRouteById(int contentId, string culture = null);
    }
}
