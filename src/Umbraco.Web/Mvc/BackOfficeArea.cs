using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.Editors;

namespace Umbraco.Web.Mvc
{
    // TODO: This has been ported to netcore, can be removed
    // Has preview been migrated?
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

        }

        public override string AreaName => _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);
    }
}
