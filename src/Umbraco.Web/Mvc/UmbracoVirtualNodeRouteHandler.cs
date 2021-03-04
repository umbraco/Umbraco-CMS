using System;
using System.Web;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    public abstract class UmbracoVirtualNodeRouteHandler : IRouteHandler
    {
        // TODO: Need to port this to netcore and figure out if its needed or how this should work (part of a different task)

        // TODO: try lazy property injection?
        private IPublishedRouter PublishedRouter => Current.Factory.GetRequiredService<IPublishedRouter>();

        /// <summary>
        /// Returns the UmbracoContext for this route handler
        /// </summary>
        /// <remarks>
        /// By default this uses the UmbracoContext singleton, this could be overridden to check for null in the case
        /// that this handler is used for a request where an UmbracoContext is not created by default see http://issues.umbraco.org/issue/U4-9384
        /// <example>
        /// <![CDATA[
        /// protected override UmbracoContext GetUmbracoContext(RequestContext requestContext)
        /// {
        ///    var ctx = base.GetUmbracoContext(requestContext);
        ///    //check if context is null, we know it will be null if we are dealing with a request that
        ///    //has an extension and by default no Umb ctx is created for the request
        ///    if (ctx == null) {
        ///        // TODO: Here you can EnsureContext , please note that the requestContext is passed in
        ///        //therefore your should refrain from using other singletons like HttpContext.Current since
        ///        //you will already have a reference to it. Also if you need an ApplicationContext you should
        ///        //pass this in via a ctor instead of using the ApplicationContext.Current singleton.
        ///    }
        ///    return ctx;
        /// }
        /// ]]>
        /// </example>
        /// </remarks>
        protected virtual IUmbracoContext GetUmbracoContext(RequestContext requestContext)
        {
            return Composing.Current.UmbracoContext;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            throw new NotImplementedException();

        //    var umbracoContext = GetUmbracoContext(requestContext);

        //    var found = FindContent(requestContext, umbracoContext);
        //    if (found == null) return new NotFoundHandler();

        //    var request = PublishedRouter.CreateRequest(umbracoContext);
        //    request.PublishedContent = found;
        //    umbracoContext.PublishedRequest = request;

        //    // allows inheritors to change the published content request
        //    PreparePublishedContentRequest(umbracoContext.PublishedRequest);

        //    // create the render model
        //    var renderModel = new ContentModel(umbracoContext.PublishedRequest.PublishedContent);

        //    // assigns the required tokens to the request
        //    //requestContext.RouteData.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, renderModel);
        //    //requestContext.RouteData.DataTokens.Add(Core.Constants.Web.PublishedDocumentRequestDataToken, umbracoContext.PublishedRequest);
        //    //requestContext.RouteData.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbracoContext);

        //    //// this is used just for a flag that this is an umbraco custom route
        //    //requestContext.RouteData.DataTokens.Add(Core.Constants.Web.CustomRouteDataToken, true);

        //    // Here we need to detect if a SurfaceController has posted
        //    var formInfo = RenderRouteHandler.GetFormInfo(requestContext);
        //    if (formInfo != null)
        //    {
        //        var def = new RouteDefinition
        //        {
        //            ActionName = requestContext.RouteData.GetRequiredString("action"),
        //            ControllerName = requestContext.RouteData.GetRequiredString("controller"),
        //            PublishedRequest = umbracoContext.PublishedRequest
        //        };

        //        // set the special data token to the current route definition
        //        requestContext.RouteData.Values[Core.Constants.Web.UmbracoRouteDefinitionDataToken] = def;

        //        return RenderRouteHandler.HandlePostedValues(requestContext, formInfo);
        //    }

        //    return new MvcHandler(requestContext);
        }

        protected abstract IPublishedContent FindContent(RequestContext requestContext, IUmbracoContext umbracoContext);

        //protected virtual void PreparePublishedContentRequest(IPublishedRequest request)
        //{
        //    PublishedRouter.PrepareRequest(request);
        //}
    }
}
