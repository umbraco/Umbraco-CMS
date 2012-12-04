using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Umbraco.Core.IO;
using Umbraco.Web.Models;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Macros
{
	/// <summary>
	/// Controller to render macro content
	/// </summary>
	internal class PartialViewMacroController : Controller
	{
		private readonly UmbracoContext _umbracoContext;
		private readonly MacroModel _macro;
		private readonly INode _currentPage;
		
		public PartialViewMacroController(UmbracoContext umbracoContext, MacroModel macro, INode currentPage)
		{
			_umbracoContext = umbracoContext;
			_macro = macro;
			_currentPage = currentPage;
		}

		/// <summary>
		/// Child action to render a macro
		/// </summary>
		/// <returns></returns>
		[ChildActionOnly]
		public PartialViewResult Index()
		{
			var model = new PartialViewMacroModel(_currentPage.ConvertFromNode(), new Dictionary<string, object>());
			return PartialView(_macro.ScriptName, model);
		}

	}

	public abstract class PartialViewMacroPage : WebViewPage<PartialViewMacroModel>
	{
		
	}

	/// <summary>
	/// A macro engine using MVC Partial Views to execute
	/// </summary>
	public class PartialViewMacroEngine : IMacroEngine
	{
		private readonly Func<HttpContextBase> _getHttpContext;
		private readonly Func<UmbracoContext> _getUmbracoContext;

		public const string EngineName = "Partial View Macro Engine";

		public PartialViewMacroEngine()
		{						
			_getHttpContext = () =>
				{
					if (HttpContext.Current == null)
						throw new InvalidOperationException("The " + this.GetType() + " cannot execute with a null HttpContext.Current reference");
					return new HttpContextWrapper(HttpContext.Current);
				};

			_getUmbracoContext = () =>
			{
				if (UmbracoContext.Current == null)
					throw new InvalidOperationException("The " + this.GetType() + " cannot execute with a null UmbracoContext.Current reference");
				return UmbracoContext.Current;
			};
		}

		/// <summary>
		/// Constructor generally used for unit testing
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="umbracoContext"> </param>
		internal PartialViewMacroEngine(HttpContextBase httpContext, UmbracoContext umbracoContext)
		{
			_getHttpContext = () => httpContext;
			_getUmbracoContext = () => umbracoContext;
		}

		public string Name
		{
			get { return EngineName; }
		}
		public IEnumerable<string> SupportedExtensions
		{
			get { return new[] {"cshtml", "vbhtml"}; }
		}
		public IEnumerable<string> SupportedUIExtensions
		{
			get { return new[] { "cshtml", "vbhtml" }; }
		}
		public Dictionary<string, IMacroGuiRendering> SupportedProperties
		{
			get { throw new NotSupportedException(); }
		}

		public bool Validate(string code, string tempFileName, INode currentPage, out string errorMessage)
		{
			var temp = GetVirtualPathFromPhysicalPath(tempFileName);
			try
			{
				CompileAndInstantiate(temp);
			}
			catch (Exception exception)
			{
				errorMessage = exception.Message;
				return false;
			}
			errorMessage = string.Empty;
			return true;
		}

		public string Execute(MacroModel macro, INode currentPage)
		{
			if (macro == null) throw new ArgumentNullException("macro");
			if (currentPage == null) throw new ArgumentNullException("currentPage");

			if (!macro.ScriptName.StartsWith(SystemDirectories.MvcViews + "/MacroPartials/"))
			{
				throw new InvalidOperationException("Cannot render the Partial View Macro with file: " + macro.ScriptName + ". All Partial View Macros must exist in the " + SystemDirectories.MvcViews + "/MacroPartials/ folder");
			}

			var http = _getHttpContext();
			var umbCtx = _getUmbracoContext();
			var routeVals = new RouteData();
			routeVals.Values.Add("controller", "PartialViewMacro");
			routeVals.Values.Add("action", "Index");
			var request = new RequestContext(http, routeVals);
			string output;
			using (var controller = new PartialViewMacroController(umbCtx, macro, currentPage))
			{
				controller.ControllerContext = new ControllerContext(request, controller);
				var result = controller.Index();				
				output = controller.RenderViewResultAsString(result);	
			}

			return output;
		}

		private string GetVirtualPathFromPhysicalPath(string physicalPath)
		{
			string rootpath = _getHttpContext().Server.MapPath("~/");
			physicalPath = physicalPath.Replace(rootpath, "");
			physicalPath = physicalPath.Replace("\\", "/");
			return "~/" + physicalPath;
		}

		private static PartialViewMacroPage CompileAndInstantiate(string virtualPath)
		{
			//Compile Razor - We Will Leave This To ASP.NET Compilation Engine & ASP.NET WebPages
			//Security in medium trust is strict around here, so we can only pass a virtual file path
			//ASP.NET Compilation Engine caches returned types
			//Changed From BuildManager As Other Properties Are Attached Like Context Path/
			var webPageBase = WebPageBase.CreateInstanceFromVirtualPath(virtualPath);
			var webPage = webPageBase as PartialViewMacroPage;
			if (webPage == null)
				throw new InvalidCastException("All Partial View Macro views must inherit from " + typeof(PartialViewMacroPage).FullName);
			return webPage;
		}
	}
}
