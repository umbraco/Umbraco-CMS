using System.Web;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Assigned to all SurfaceController's so that it returns our custom SurfaceMvcHandler to use for rendering
    /// </summary>
    internal class SurfaceRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new UmbracoMvcHandler(requestContext);
        }
    }
}
