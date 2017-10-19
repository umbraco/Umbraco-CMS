﻿using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
    public abstract class UmbracoVirtualNodeRouteHandler : IRouteHandler
    {
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
        ///        //TODO: Here you can EnsureContext , please note that the requestContext is passed in 
        ///        //therefore your should refrain from using other singletons like HttpContext.Current since
        ///        //you will already have a reference to it. Also if you need an ApplicationContext you should
        ///        //pass this in via a ctor instead of using the ApplicationContext.Current singleton.
        ///    }
        ///    return ctx;
        /// }
        /// ]]>
        /// </example>
        /// </remarks>
        protected virtual UmbracoContext GetUmbracoContext(RequestContext requestContext)
        {
            return UmbracoContext.Current;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var umbracoContext = GetUmbracoContext(requestContext);
            
            var found = FindContent(requestContext, umbracoContext);
            if (found == null) return new NotFoundHandler();
            
            umbracoContext.PublishedContentRequest = new PublishedContentRequest(
                umbracoContext.CleanedUmbracoUrl, umbracoContext.RoutingContext, 
                UmbracoConfig.For.UmbracoSettings().WebRouting, s => Roles.Provider.GetRolesForUser(s))
            {
                PublishedContent = found
            };

            //allows inheritors to change the pcr
            PreparePublishedContentRequest(umbracoContext.PublishedContentRequest);

            //create the render model
            var renderModel = new RenderModel(umbracoContext.PublishedContentRequest.PublishedContent, umbracoContext.PublishedContentRequest.Culture);

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
                requestContext.RouteData.DataTokens[Umbraco.Core.Constants.Web.UmbracoRouteDefinitionDataToken] = def;

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
