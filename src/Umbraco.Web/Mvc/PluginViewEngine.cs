using System.IO;
using System.Linq;
using System.Web.Mvc;
using Lucene.Net.Util;
using Microsoft.Web.Mvc;
using Umbraco.Core.IO;

namespace Umbraco.Web.Mvc
{
    /// <summary>
	/// A view engine to look into the App_Plugins folder for views for packaged controllers
	/// </summary>
    public class PluginViewEngine : ReflectedFixedRazorViewEngine
	{
		
		/// <summary>
		/// Constructor
		/// </summary>
		public PluginViewEngine()
		{
			SetViewLocations();
		}

		private void SetViewLocations()
		{
			//these are the originals:

			//base.AreaViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
			//base.AreaMasterLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
			//base.AreaPartialViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
			//base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
			//base.MasterLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
			//base.PartialViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
			//base.FileExtensions = new string[] { "cshtml", "vbhtml" };

			var viewLocationsArray = new[]
				{
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.cshtml"),
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.vbhtml")                    
				};

			//set all of the area view locations to the plugin folder
			AreaViewLocationFormats = viewLocationsArray
				.Concat(new[]
					{
						string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.cshtml"),
						string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.vbhtml")                        
					})
				.ToArray();

            AreaMasterLocationFormats = viewLocationsArray;

            AreaPartialViewLocationFormats = new[]
				{
					//will be used when we have partial view and child action macros
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/Partials/{0}.cshtml"),
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/Partials/{0}.vbhtml"),
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/MacroPartials/{0}.cshtml"),
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/MacroPartials/{0}.vbhtml"),                    
					//for partials                    
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.cshtml"),
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/{1}/{0}.vbhtml"),
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.cshtml"),
					string.Concat(SystemDirectories.AppPlugins, "/{2}/Views/Shared/{0}.vbhtml")
				};

		}

		/// <summary>
		/// Ensures that the correct web.config for razor exists in the /Views folder.
		/// </summary>
		private void EnsureFolderAndWebConfig(ViewEngineResult result)
		{
			if (result.View == null) return;
			var razorResult = result.View as RazorView;
			if (razorResult == null) return;

			var folder = Path.GetDirectoryName(IOHelper.MapPath(razorResult.ViewPath));
			//now we need to get the /View/ folder
			var viewFolder = folder.Substring(0, folder.LastIndexOf("\\Views\\")) + "\\Views";

			//ensure the web.config file is in the ~/Views folder
			Directory.CreateDirectory(viewFolder);
			if (!File.Exists(Path.Combine(viewFolder, "web.config")))
			{
				using (var writer = File.CreateText(Path.Combine(viewFolder, "web.config")))
				{
                    writer.Write(Strings.WebConfigTemplate);
				}
			}
		}

		public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{			
			var result = base.FindView(controllerContext, viewName, masterName, useCache);
			EnsureFolderAndWebConfig(result);
			return result;
		}

		public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{			
			var result = base.FindPartialView(controllerContext, partialViewName, useCache);
			EnsureFolderAndWebConfig(result);
			return result;
		}
	}
}