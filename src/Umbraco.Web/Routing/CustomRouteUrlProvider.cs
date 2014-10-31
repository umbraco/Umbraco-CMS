using System;
using System.Collections.Generic;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// This url provider is used purely to deal with umbraco custom routes that utilize UmbracoVirtualNodeRouteHandler and will return
    /// the URL returned from the current PublishedContentRequest.PublishedContent (virtual node) if the request is in fact a virtual route and 
    /// the id that is being requested matches the id of the current PublishedContentRequest.PublishedContent.
    /// </summary>
    internal class CustomRouteUrlProvider : IUrlProvider
    {
        /// <summary>
        /// This will simply return the URL that is returned by the assigned IPublishedContent if this is a custom route
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="id"></param>
        /// <param name="current"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string GetUrl(UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode)
        {
            if (umbracoContext == null) return null;
            if (umbracoContext.PublishedContentRequest == null) return null;
            if (umbracoContext.PublishedContentRequest.PublishedContent == null) return null;
            if (umbracoContext.HttpContext == null) return null;
            if (umbracoContext.HttpContext.Request == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext.RouteData == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext.RouteData.DataTokens == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext.RouteData.DataTokens.ContainsKey("umbraco-custom-route") == false) return null;
            //ok so it's a custom route with published content assigned, check if the id being requested for is the same id as the assigned published content
            return id == umbracoContext.PublishedContentRequest.PublishedContent.Id 
                ? umbracoContext.PublishedContentRequest.PublishedContent.Url 
                : null;
        }

        /// <summary>
        /// This always returns null because this url provider is used purely to deal with Umbraco custom routes with 
        /// UmbracoVirtualNodeRouteHandler, we really only care about the normal URL so that RedirectToCurrentUmbracoPage() works 
        /// with SurfaceControllers
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="id"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            return null;
        }
    }
}