using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.Web.Models;

namespace Umbraco.Web.Website.ViewEngines
{
    /// <summary>
    /// A view engine to look into the template location specified in the config for the front-end/Rendering part of the cms,
    /// this includes paths to render partial macros and media item templates.
    /// </summary>
    public class RenderViewEngine : RazorViewEngine
    {

        public RenderViewEngine(
            IRazorPageFactoryProvider pageFactory,
            IRazorPageActivator pageActivator,
            HtmlEncoder htmlEncoder,
            ILoggerFactory loggerFactory,
            DiagnosticListener diagnosticListener)
            : base(pageFactory, pageActivator, htmlEncoder, OverrideViewLocations(), loggerFactory, diagnosticListener)
        {
        }

        private static IOptions<RazorViewEngineOptions> OverrideViewLocations()
        {
            return Options.Create<RazorViewEngineOptions>(new RazorViewEngineOptions()
            {
                //NOTE: we will make the main view location the last to be searched since if it is the first to be searched and there is both a view and a partial
                // view in both locations and the main view is rendering a partial view with the same name, we will get a stack overflow exception.
                // http://issues.umbraco.org/issue/U4-1287, http://issues.umbraco.org/issue/U4-1215
                ViewLocationFormats =
                {
                    "/Partials/{0}.cshtml",
                    "/MacroPartials/{0}.cshtml",
                    "/{0}.cshtml"
                },
            });
        }

        public new ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            return ShouldFindView(context, viewName, isMainPage)
            ? base.FindView(context, viewName, isMainPage)
            : ViewEngineResult.NotFound(viewName, Array.Empty<string>());
        }

        // /// <summary>
        // /// Constructor
        // /// </summary>
        // public RenderViewEngine(IHostingEnvironment hostingEnvironment)
        // {
        //     _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        //
        //     const string templateFolder = Constants.ViewLocation;
        //
        //     // the Render view engine doesn't support Area's so make those blank
        //     ViewLocationFormats = _supplementedViewLocations.Select(x => templateFolder + x).ToArray();
        //     PartialViewLocationFormats = _supplementedPartialViewLocations.Select(x => templateFolder + x).ToArray();
        //
        //     AreaPartialViewLocationFormats = Array.Empty<string>();
        //     AreaViewLocationFormats = Array.Empty<string>();
        //
        //     EnsureFoldersAndFiles();
        // }


        // public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        // {
        //     return ShouldFindView(controllerContext, false)
        //         ? base.FindView(controllerContext, viewName, masterName, useCache)
        //         : new ViewEngineResult(new string[] { });
        // }
        //
        // public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        // {
        //     return ShouldFindView(controllerContext, true)
        //         ? base.FindPartialView(controllerContext, partialViewName, useCache)
        //         : new ViewEngineResult(new string[] { });
        // }

        /// <summary>
        /// Determines if the view should be found, this is used for view lookup performance and also to ensure
        /// less overlap with other user's view engines. This will return true if the Umbraco back office is rendering
        /// and its a partial view or if the umbraco front-end is rendering but nothing else.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="isPartial"></param>
        /// <returns></returns>
        private static bool ShouldFindView(ActionContext context, string viewName, bool isMainPage)
        {
            var umbracoToken = context.GetDataTokenInViewContextHierarchy(Core.Constants.Web.UmbracoDataToken);

            context.ActionDescriptor.
            // first check if we're rendering a partial view for the back office, or surface controller, etc...
            // anything that is not ContentModel as this should only pertain to Umbraco views.
            if (isPartial && !(umbracoToken is ContentModel))
                return true;

            // only find views if we're rendering the umbraco front end
            return umbracoToken is ContentModel;
        }


    }
}
