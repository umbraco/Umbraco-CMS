using System;
using System.Web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Event args used for event launched during a request (like in the UmbracoModule)
    /// </summary>
    public class UmbracoRequestEventArgs : EventArgs
    {
        public IUmbracoContext UmbracoContext { get; private set; }

        public UmbracoRequestEventArgs(IUmbracoContext umbracoContext)
        {
            UmbracoContext = umbracoContext;
        }
    }
}
