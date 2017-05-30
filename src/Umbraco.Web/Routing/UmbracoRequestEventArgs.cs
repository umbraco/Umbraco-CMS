using System;
using System.Web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Event args used for event launched during a request (like in the UmbracoModule)
    /// </summary>
    public class UmbracoRequestEventArgs : EventArgs
    {
        public UmbracoContext UmbracoContext { get; private set; }
        public HttpContextBase HttpContext { get; private set; }

        public UmbracoRequestEventArgs(UmbracoContext umbracoContext, HttpContextBase httpContext)
        {
            UmbracoContext = umbracoContext;
            HttpContext = httpContext;
        }
    }
}