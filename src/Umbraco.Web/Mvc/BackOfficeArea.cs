using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.Editors;

namespace Umbraco.Web.Mvc
{
    // TODO: This has been ported to netcore, can be removed
    internal class BackOfficeArea : AreaRegistration
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public BackOfficeArea(GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
        }

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
                "Umbraco_preview",
                AreaName + "/preview/{action}/{editor}",
                new {controller = "Preview", action = "Index", editor = UrlParameter.Optional},
                new[] { "Umbraco.Web.Editors" });

            context.MapRoute(
                "Umbraco_back_office",
                AreaName + "/{action}/{id}",
                new {controller = "BackOffice", action = "Default", id = UrlParameter.Optional},
                //limit the action/id to only allow characters - this is so this route doesn't hog all other
                // routes like: /umbraco/channels/word.aspx, etc...
                new
                    {
                        action = @"[a-zA-Z]*",
                        id = @"[a-zA-Z]*"
                    },
                new[] {typeof (BackOfficeController).Namespace});
        }

        public override string AreaName => _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);
    }
}
