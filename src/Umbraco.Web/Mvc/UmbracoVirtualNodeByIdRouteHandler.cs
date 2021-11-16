using System.Web.Routing;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Mvc
{
    public class UmbracoVirtualNodeByIdRouteHandler : UmbracoVirtualNodeRouteHandler
    {
        private readonly int _realNodeId;

        public UmbracoVirtualNodeByIdRouteHandler(int realNodeId)
        {
            _realNodeId = realNodeId;
        }

        protected sealed override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext)
        {
            var byId = umbracoContext.Content.GetById(_realNodeId);
            return byId == null ? null : FindContent(requestContext, umbracoContext, byId);
        }

        protected virtual IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext, IPublishedContent baseContent)
        {
            return baseContent;
        }
    }
}
