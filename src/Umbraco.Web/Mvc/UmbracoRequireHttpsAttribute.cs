using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// If umbracoUseSSL property in web.config is set to true, this filter will redirect any http access to https.
    /// </summary>
    public class UmbracoRequireHttpsAttribute : RequireHttpsAttribute
    {
        /// <summary>
        /// If umbracoUseSSL is true and we have a non-HTTPS request, handle redirect.
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            // If umbracoUseSSL is set, let base method handle redirect.  Otherwise, we don't care.
            if (Current.Configs.Global().UseHttps)
            {
                base.HandleNonHttpsRequest(filterContext);
            }
        }

        /// <summary>
        /// Check to see if HTTPS is currently being used if umbracoUseSSL is true.
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // If umbracoSSL is set, let base method handle checking for HTTPS.  Otherwise, we don't care.
            if (Current.Configs.Global().UseHttps)
            {
                base.OnAuthorization(filterContext);
            }
        }


    }
}
