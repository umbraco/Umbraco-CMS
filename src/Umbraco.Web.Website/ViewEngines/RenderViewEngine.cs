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
    public class RenderViewEngine : RazorViewEngine, IRenderViewEngine
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
                AreaViewLocationFormats =
                {
                    "/Partials/{0}.cshtml",
                    "/MacroPartials/{0}.cshtml",
                    "/{0}.cshtml"
                }
            });
        }

        public new ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            return ShouldFindView(context, isMainPage)
            ? base.FindView(context, viewName, isMainPage)
            : ViewEngineResult.NotFound(viewName, Array.Empty<string>());
        }

        /// <summary>
        /// Determines if the view should be found, this is used for view lookup performance and also to ensure
        /// less overlap with other user's view engines. This will return true if the Umbraco back office is rendering
        /// and its a partial view or if the umbraco front-end is rendering but nothing else.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="isPartial"></param>
        /// <returns></returns>
        private static bool ShouldFindView(ActionContext context, bool isMainPage)
        {
            //In v8, this was testing recursively into if it was a child action, but child action do not exist anymore,
            //And my best guess is that it
            context.RouteData.DataTokens.TryGetValue(Core.Constants.Web.UmbracoDataToken, out var umbracoToken);
            // first check if we're rendering a partial view for the back office, or surface controller, etc...
            // anything that is not ContentModel as this should only pertain to Umbraco views.
            if (!isMainPage && !(umbracoToken is ContentModel))
                return true;

            // only find views if we're rendering the umbraco front end
            return umbracoToken is ContentModel;
        }


    }
}
