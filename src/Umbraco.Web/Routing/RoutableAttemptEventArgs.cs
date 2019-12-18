using System.Web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Event args containing information about why the request was not routable, or if it is routable
    /// </summary>
    public class RoutableAttemptEventArgs : UmbracoRequestEventArgs
    {
        public EnsureRoutableOutcome Outcome { get; private set; }

        public RoutableAttemptEventArgs(EnsureRoutableOutcome reason, UmbracoContext umbracoContext, HttpContextBase httpContext)
            : base(umbracoContext, httpContext)
        {
            Outcome = reason;
        }
    }
}
