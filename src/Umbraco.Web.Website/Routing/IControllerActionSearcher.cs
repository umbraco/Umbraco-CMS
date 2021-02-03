namespace Umbraco.Web.Website.Routing
{
    public interface IControllerActionSearcher
    {
        ControllerActionSearchResult Find<T>(string controller, string action);
    }
}