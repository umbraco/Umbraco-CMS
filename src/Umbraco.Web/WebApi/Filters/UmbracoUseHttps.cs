using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using System.Web.Mvc;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// If umbracoUseSSL property in web.config is set to true, this filter will redirect any http access to https.
    /// </summary>
    public class UmbracoUseHttps : RequireHttpsAttribute
    {
        /// <summary>
        /// If umbracoUseSSL is true and we have a non-HTTPS request, handle redirect.
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            // If umbracoUseSSL is set, let base method handle redirect.  Otherwise, we don't care.
            if (GlobalSettings.UseSSL)
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
            if (GlobalSettings.UseSSL)
            {
                base.OnAuthorization(filterContext);
            }
        }


    }
}
