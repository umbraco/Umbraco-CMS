using System.Web;
using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// A custom WebApi route handler that enables session on the HttpContext - use with caution! 
    /// </summary>
    /// <remarks>
    /// WebApi controllers (and REST in general) shouldn't have session state enabled since it's stateless,
    /// enabling session state puts additional locks on requests so only use this when absolutley needed 
    /// </remarks>
    internal class SessionHttpControllerRouteHandler : HttpControllerRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new SessionHttpControllerHandler(requestContext.RouteData);
        }

        /// <summary>
        /// A custom WebApi handler that enables session on the HttpContext
        /// </summary>        
        private class SessionHttpControllerHandler : HttpControllerHandler, IRequiresSessionState
        {
            public SessionHttpControllerHandler(RouteData routeData) : base(routeData)
            { }
        }
    }
}