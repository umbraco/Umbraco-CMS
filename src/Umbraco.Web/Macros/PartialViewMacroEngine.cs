using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Umbraco.Web.Models;
using umbraco.IO;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace Umbraco.Web.Macros
{
	///// <summary>
	///// Controller to render macro content
	///// </summary>
	//public class MacroController : Controller
	//{
	//	private readonly UmbracoContext _umbracoContext;

	//	public MacroController(UmbracoContext umbracoContext)
	//	{
	//		_umbracoContext = umbracoContext;
	//	}

	//	/// <summary>
	//	/// Child action to render a macro
	//	/// </summary>
	//	/// <param name="macroAlias">The macro alias to render</param>
	//	/// <returns></returns>
	//	[ChildActionOnly]
	//	public ActionResult Index(string macroAlias)
	//	{
			
	//	}

	//}

	public abstract class PartialViewMacroPage : WebViewPage<PartialViewMacroModel>
	{
		
	}

	/// <summary>
	/// A macro engine using MVC Partial Views to execute
	/// </summary>
	public class PartialViewMacroEngine : IMacroEngine
	{
		private readonly Func<HttpContextBase> _getHttpContext;

		public const string EngineName = "Partial View Macro Engine";

		public PartialViewMacroEngine()
		{						
			_getHttpContext = () =>
				{
					if (HttpContext.Current == null)
						throw new InvalidOperationException("The " + this.GetType() + " cannot execute with a null HttpContext.Current reference");
					return new HttpContextWrapper(HttpContext.Current);
				};
		}

		/// <summary>
		/// Constructor generally used for unit testing
		/// </summary>
		/// <param name="httpContext"></param>
		internal PartialViewMacroEngine(HttpContextBase httpContext)
		{
			_getHttpContext = () => httpContext;
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
			string fileLocation = null;
			if (!string.IsNullOrEmpty(macro.ScriptName))
			{
				//Razor Is Already Contained In A File
				if (macro.ScriptName.StartsWith("~"))
					fileLocation = macro.ScriptName;
				else
					fileLocation = SystemDirectories.MacroScripts + "/" + macro.ScriptName;
			}
			if (string.IsNullOrEmpty(fileLocation))
				return String.Empty; //No File Location

			//var controller = new MacroController(UmbracoContext.Current);
			//controller.ControllerContext = new ControllerContext();
			var model = new PartialViewMacroModel(currentPage.ConvertFromNode(), new Dictionary<string, object>());
			var view = CompileAndInstantiate(fileLocation);
			var output = new StringWriter();
			view.ExecutePageHierarchy(new WebPageContext(_getHttpContext(), view, model), output);
			return output.ToString();
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
