using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Configuration;
using Umbraco.Web.Editors;
using Umbraco.Web.Install;
using Umbraco.Web.Install.Controllers;

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
            context.MapRoute(
                "Umbraco_back_office",
                GlobalSettings.UmbracoMvcArea + "/{action}/{id}",
                new {controller = "BackOffice", action = "Default", id = UrlParameter.Optional},
                //limit the action/id to only allow characters - this is so this route doesn't hog all other 
                // routes like: /umbraco/channels/word.aspx, etc...
                new
                    {
                        action = @"[a-zA-Z]*", 
                        id = @"[a-zA-Z]*"
                    },
                new[] {typeof (BackOfficeController).Namespace});
            
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