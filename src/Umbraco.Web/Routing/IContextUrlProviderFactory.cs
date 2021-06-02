namespace Umbraco.Web.Routing
{
    public interface IContextUrlProviderFactory
    {
        UrlProvider Build(UmbracoContext umbracoContext);
    }
}