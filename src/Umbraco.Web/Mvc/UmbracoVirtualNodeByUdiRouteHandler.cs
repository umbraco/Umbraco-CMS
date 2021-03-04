using System.Web.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    public class UmbracoVirtualNodeByUdiRouteHandler : UmbracoVirtualNodeRouteHandler
    {
        private readonly Udi _realNodeUdi;

        public UmbracoVirtualNodeByUdiRouteHandler(GuidUdi realNodeUdi)
        {
            _realNodeUdi = realNodeUdi;
        }

        protected sealed override IPublishedContent FindContent(RequestContext requestContext, IUmbracoContext umbracoContext)
        {
            var byId = umbracoContext.Content.GetById(_realNodeUdi);
            return byId == null ? null : FindContent(requestContext, umbracoContext, byId);
        }

        protected virtual IPublishedContent FindContent(RequestContext requestContext, IUmbracoContext umbracoContext, IPublishedContent baseContent)
        {
            return baseContent;
        }
    }
}
