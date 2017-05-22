using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using LightInject;

namespace Umbraco.Web.Mvc
{
    public abstract class UmbracoVirtualNodeRouteHandler : IRouteHandler
    {
        // todo - try lazy property injection?
        private FacadeRouter FacadeRouter => Core.DI.Current.Container.GetInstance<FacadeRouter>();

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var umbracoContext = UmbracoContext.Current;

            var found = FindContent(requestContext, umbracoContext);
            if (found == null) return new NotFoundHandler();

            var request = FacadeRouter.CreateRequest(umbracoContext);
            request.PublishedContent = found;
            umbracoContext.PublishedContentRequest = request;

            //allows inheritors to change the pcr
            PreparePublishedContentRequest(umbracoContext.PublishedContentRequest);

            //create the render model
            var renderModel = new ContentModel(umbracoContext.PublishedContentRequest.PublishedContent);

            //assigns the required tokens to the request
            requestContext.RouteData.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, renderModel);
            requestContext.RouteData.DataTokens.Add(Core.Constants.Web.PublishedDocumentRequestDataToken, umbracoContext.PublishedContentRequest);
            requestContext.RouteData.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbracoContext);
            //this is used just for a flag that this is an umbraco custom route
            requestContext.RouteData.DataTokens.Add(Core.Constants.Web.CustomRouteDataToken, true);

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
                requestContext.RouteData.DataTokens[Core.Constants.Web.UmbracoRouteDefinitionDataToken] = def;

                return RenderRouteHandler.HandlePostedValues(requestContext, formInfo);
            }

            return new MvcHandler(requestContext);
        }

        protected abstract IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext);

        protected virtual void PreparePublishedContentRequest(PublishedContentRequest request)
        {
            FacadeRouter.PrepareRequest(request);
        }
    }
}
