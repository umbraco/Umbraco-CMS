using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
    public abstract class UmbracoVirtualNodeRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var umbracoContext = UmbracoContext.Current;

            var found = FindContent(requestContext, umbracoContext);
            if (found == null) return new NotFoundHandler();
            
            umbracoContext.PublishedContentRequest = new PublishedContentRequest(
                umbracoContext.CleanedUmbracoUrl, umbracoContext.RoutingContext)
            {
                PublishedContent = found
            };

            //allows inheritors to change the pcr
            PreparePublishedContentRequest(umbracoContext.PublishedContentRequest);

            //create the render model
            var renderModel = new RenderModel(umbracoContext.PublishedContentRequest.PublishedContent, umbracoContext.PublishedContentRequest.Culture);

            //assigns the required tokens to the request
            requestContext.RouteData.DataTokens.Add("umbraco", renderModel);
            requestContext.RouteData.DataTokens.Add("umbraco-doc-request", umbracoContext.PublishedContentRequest);
            requestContext.RouteData.DataTokens.Add("umbraco-context", umbracoContext);

            umbracoContext.PublishedContentRequest.ConfigureRequest();

            return new MvcHandler(requestContext);
        }

        protected abstract IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext);

        protected virtual void PreparePublishedContentRequest(PublishedContentRequest publishedContentRequest)
        {
            //need to set the culture for this to work
            if (publishedContentRequest.Culture == null)
            {
                //none specified so get the default
                var defaultLanguage = global::umbraco.cms.businesslogic.language.Language.GetAllAsList().FirstOrDefault();
                publishedContentRequest.Culture = defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.CultureAlias);
            }
        }
    }
}
