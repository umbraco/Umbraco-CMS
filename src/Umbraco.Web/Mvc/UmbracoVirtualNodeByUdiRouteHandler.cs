using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Mvc
{
    public class UmbracoVirtualNodeByUdiRouteHandler : UmbracoVirtualNodeRouteHandler
    {
        private readonly Udi _realNodeUdi;

        public UmbracoVirtualNodeByUdiRouteHandler(GuidUdi realNodeUdi)
        {
            _realNodeUdi = realNodeUdi;
        }

        protected sealed override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext)
        {
            var byId = umbracoContext.Content.GetById(_realNodeUdi);
            return byId == null ? null : FindContent(requestContext, umbracoContext, byId);
        }

        protected virtual IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext, IPublishedContent baseContent)
        {
            return baseContent;
        }
    }
}
