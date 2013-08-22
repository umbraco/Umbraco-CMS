using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// An area registration for back office components
    /// </summary>
    internal class BackOfficeArea : AreaRegistration
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
            //Create the install routes
            context.MapRoute(
                "Umbraco_install_packages",
                "Install/PackageInstaller/{action}/{id}",
                new {controller = "InstallPackage", action = "Index", id = UrlParameter.Optional},
                new[] {typeof (InstallPackageController).Namespace});
            
            //Create the REST/web/script service routes
            context.MapRoute(
                "Umbraco_web_services",
                GlobalSettings.UmbracoMvcArea + "/RestServices/{controller}/{action}/{id}",
                new {controller = "SaveFileController", action = "Index", id = UrlParameter.Optional},
                //look in this namespace for controllers
                new[] {"Umbraco.Web.WebServices"});
        }

        public override string AreaName
        {
            get { return GlobalSettings.UmbracoMvcArea; }
        }
    }
}