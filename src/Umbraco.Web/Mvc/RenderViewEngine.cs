using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A view engine to look into the template location specified in the config for the front-end/Rendering part of the cms,
	/// this includes paths to render partial macros and media item templates.
	/// </summary>
    public class RenderViewEngine : ReflectedFixedRazorViewEngine
	{

		private readonly IEnumerable<string> _supplementedViewLocations = new[] { "/{0}.cshtml" };
		//NOTE: we will make the main view location the last to be searched since if it is the first to be searched and there is both a view and a partial
		// view in both locations and the main view is rendering a partial view with the same name, we will get a stack overflow exception. 
		// http://issues.umbraco.org/issue/U4-1287, http://issues.umbraco.org/issue/U4-1215
		private readonly IEnumerable<string> _supplementedPartialViewLocations = new[] { "/Partials/{0}.cshtml", "/MacroPartials/{0}.cshtml", "/{0}.cshtml" };

		/// <summary>
		/// Constructor
		/// </summary>
		public RenderViewEngine()
		{
			const string templateFolder = Constants.ViewLocation;

			var replaceWithUmbracoFolder = _supplementedViewLocations.ForEach(location => templateFolder + location);
			var replacePartialWithUmbracoFolder = _supplementedPartialViewLocations.ForEach(location => templateFolder + location);

			//The Render view engine doesn't support Area's so make those blank
            ViewLocationFormats = replaceWithUmbracoFolder.ToArray();
			PartialViewLocationFormats = replacePartialWithUmbracoFolder.ToArray();

			AreaPartialViewLocationFormats = new string[] { };
			AreaViewLocationFormats = new string[] { };

			EnsureFoldersAndFiles();
		}

		/// <summary>
		/// Ensures that the correct web.config for razor exists in the /Views folder, the partials folder exist and the ViewStartPage exists.
		/// </summary>
		private void EnsureFoldersAndFiles()
		{
			var viewFolder = IOHelper.MapPath(Constants.ViewLocation);
			//ensure the web.config file is in the ~/Views folder
			Directory.CreateDirectory(viewFolder);
			if (!File.Exists(Path.Combine(viewFolder, "web.config")))
			{
				using (var writer = File.CreateText(Path.Combine(viewFolder, "web.config")))
				{
					writer.Write(Strings.WebConfigTemplate);
				}
			}
			//auto create the partials folder
			var partialsFolder = Path.Combine(viewFolder, "Partials");
			Directory.CreateDirectory(partialsFolder);			

			//We could create a _ViewStart page if it isn't there as well, but we may not allow editing of this page in the back office.
		}

		public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			if (!ShouldFindView(controllerContext, false))
			{
				return new ViewEngineResult(new string[] { });
			}

			return base.FindView(controllerContext, viewName, masterName, useCache);
		}

		public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			if (!ShouldFindView(controllerContext, true))
			{
				return new ViewEngineResult(new string[] { });
			}

			return base.FindPartialView(controllerContext, partialViewName, useCache);
		}

		/// <summary>
		/// Determines if the view should be found, this is used for view lookup performance and also to ensure 
		/// less overlap with other user's view engines. This will return true if the Umbraco back office is rendering
		/// and its a partial view or if the umbraco front-end is rendering but nothing else.
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="isPartial"></param>
		/// <returns></returns>
        private bool ShouldFindView(ControllerContext controllerContext, bool isPartial)
        {
            var umbracoToken = controllerContext.GetDataTokenInViewContextHierarchy("umbraco");

            //first check if we're rendering a partial view for the back office, or surface controller, etc...
            //anything that is not IUmbracoRenderModel as this should only pertain to Umbraco views.
            if (isPartial && ((umbracoToken is RenderModel) == false))
            {
                return true;
            }

            //only find views if we're rendering the umbraco front end
            if (umbracoToken is RenderModel)
            {
                return true;
            }

            return false;
        }

	}
}