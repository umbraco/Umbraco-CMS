using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Mvc;
using Umbraco.Core;
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

            //TODO: We can remove this when we re-build the back office package installer
            //Create the install routes
            context.MapHttpRoute(
                "Umbraco_install_packages",
                "Install/PackageInstaller/{action}/{id}",
                new { controller = "InstallPackage", action = "Index", id = UrlParameter.Optional },
                new[] { typeof(InstallPackageController).Namespace });

            context.MapHttpRoute(
                "umbraco-install-api",
                "install/api/{action}/{id}",
                new { controller = "InstallApi", action = "Status", id = RouteParameter.Optional },
                new[] { typeof(InstallApiController).Namespace });
        }

        public override string AreaName
        {
            get { return "UmbracoInstall"; }
        }
    }
}