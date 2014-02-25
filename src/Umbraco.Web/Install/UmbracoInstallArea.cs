using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Configuration;
using Umbraco.Web.Editors;
using Umbraco.Web.Install.Controllers;

namespace Umbraco.Web.Install
{
    /// <summary>
    /// An area registration for back office components
    /// </summary>
    internal class UmbracoInstallArea : AreaRegistration
    {

        /// <summary>
        /// Create the routes for the area
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>
        /// By using the context to register the routes it means that the area is already applied to them all 
        /// and that the namespaces searched for the controllers are ONLY the ones specified.
        /// </remarks>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "umbraco-install",
                "Install",
                new { controller = "Install", action = "Index", id = UrlParameter.Optional },
                new[] { typeof(InstallController).Namespace });
            
            var apiRoute = context.Routes.MapHttpRoute(
                "umbraco-install-api",
                "install/api/{action}/{id}",
                new { controller = "InstallApi", action = "Status", id = RouteParameter.Optional });
            //web api routes don't set the data tokens object
            if (apiRoute.DataTokens == null)
            {
                apiRoute.DataTokens = new RouteValueDictionary();
            }
            apiRoute.DataTokens.Add("Namespaces", new[] { typeof(InstallApiController).Namespace }); //look in this namespace to create the controller
            apiRoute.DataTokens.Add("UseNamespaceFallback", false); //Don't look anywhere else except this namespace!
        }

        public override string AreaName
        {
            get { return "UmbracoInstall"; }
        }
    }
}