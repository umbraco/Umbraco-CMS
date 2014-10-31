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
            //this is used just for a flag that this is an umbraco custom route
            requestContext.RouteData.DataTokens.Add("umbraco-custom-route", true);

            //Here we need to detect if a SurfaceController has posted
            var formInfo = RenderRouteHandler.GetFormInfo(requestContext);
            if (formInfo != null)
            {
                var def = new RouteDefinition
                {
                    ActionName = requestContext.RouteData.GetRequiredString("action"),
                    ControllerName = requestContext.RouteData.GetRequiredString("controller"),
                    PublishedContentRequest = umbracoContext.PublishedContentRequest
                };

                //set the special data token to the current route definition
                requestContext.RouteData.DataTokens["umbraco-route-def"] = def;

                return RenderRouteHandler.HandlePostedValues(requestContext, formInfo);
            }

            return new MvcHandler(requestContext);
        }

        protected abstract IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext);

        protected virtual void PreparePublishedContentRequest(PublishedContentRequest publishedContentRequest)
        {
            publishedContentRequest.Prepare();
        }
    }
}
