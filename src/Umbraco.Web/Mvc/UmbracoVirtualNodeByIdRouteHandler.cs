using System.Web.Routing;
using Umbraco.Core.Models;

namespace Umbraco.Web.Mvc
{
    public abstract class UmbracoVirtualNodeByIdRouteHandler : UmbracoVirtualNodeRouteHandler
    {
        private readonly int _realNodeId;

        protected UmbracoVirtualNodeByIdRouteHandler(int realNodeId)
        {
            _realNodeId = realNodeId;
        }

        protected sealed override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext)
        {
            var byId = umbracoContext.ContentCache.GetById(_realNodeId);
            if (byId == null) return null;

            return FindContent(requestContext, umbracoContext, byId);
        }

        protected abstract IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext, IPublishedContent baseContent);
    }
}