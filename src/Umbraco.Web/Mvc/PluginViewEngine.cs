using System.Linq;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A view engine to look into the App_Plugins/Packages folder for views for packaged controllers
	/// </summary>
	public class PluginViewEngine : RazorViewEngine
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
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/{1}/{0}.cshtml"),
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/{1}/{0}.vbhtml")                    
				};

			//set all of the area view locations to the plugin folder
			AreaViewLocationFormats = viewLocationsArray
				.Concat(new[]
					{
						string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/Shared/{0}.cshtml"),
						string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/Shared/{0}.vbhtml")                        
					})
				.ToArray();

			AreaMasterLocationFormats = viewLocationsArray;

			AreaPartialViewLocationFormats = new[]
				{
					//will be used when we have partial view and child action macros
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/Partials/{0}.cshtml"),
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/Partials/{0}.vbhtml"),
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/MacroPartials/{0}.cshtml"),
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/MacroPartials/{0}.vbhtml"),                    
					//for partials                    
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/{1}/{0}.cshtml"),
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/{1}/{0}.vbhtml"),
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/Shared/{0}.cshtml"),
					string.Concat(Constants.PluginsLocation, "/Packages/{2}/Views/Shared/{0}.vbhtml")
				};

		}
	}
}