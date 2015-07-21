using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.macro;
using Umbraco.Core.Models;
using umbraco.interfaces;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core;
using System.Web.Mvc.Html;

namespace Umbraco.Web.Macros
{
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

		//NOTE: We do not return any supported extensions because we don't want the MacroEngineFactory to return this
		// macro engine when searching for engines via extension. Those types of engines are reserved for files that are
		// stored in the ~/macroScripts folder and each engine must support unique extensions. This is a total Hack until 
		// we rewrite how macro engines work.
		public IEnumerable<string> SupportedExtensions
		{
			get { return Enumerable.Empty<string>(); }		
		}

		//NOTE: We do not return any supported extensions because we don't want the MacroEngineFactory to return this
		// macro engine when searching for engines via extension. Those types of engines are reserved for files that are
		// stored in the ~/macroScripts folder and each engine must support unique extensions. This is a total Hack until 
		// we rewrite how macro engines work.
		public IEnumerable<string> SupportedUIExtensions
		{
			get { return Enumerable.Empty<string>(); }
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

        public string Execute(MacroModel macro, INode node)
        {
            if (node == null) return string.Empty;

            var umbCtx = _getUmbracoContext();
            //NOTE: This is a bit of a nasty hack to check if the INode is actually already based on an IPublishedContent 
            // (will be the case when using LegacyConvertedNode )
            return Execute(macro,
                (node is IPublishedContent)
                    ? (IPublishedContent)node
                    : umbCtx.ContentCache.GetById(node.Id));
        }

        public string Execute(MacroModel macro, IPublishedContent content)
        {
            if (macro == null) throw new ArgumentNullException("macro");
            if (content == null) throw new ArgumentNullException("content");
			if (macro.ScriptName.IsNullOrWhiteSpace()) throw new ArgumentException("The ScriptName property of the macro object cannot be null or empty");
		
            var http = _getHttpContext();
            var umbCtx = _getUmbracoContext();
            var routeVals = new RouteData();
            routeVals.Values.Add("controller", "PartialViewMacro");
            routeVals.Values.Add("action", "Index");
            routeVals.DataTokens.Add("umbraco-context", umbCtx); //required for UmbracoViewPage

			//lets render this controller as a child action
			var viewContext = new ViewContext {ViewData = new ViewDataDictionary()};;
            //try and extract the current view context from the route values, this would be set in the UmbracoViewPage or in 
            // the UmbracoPageResult if POSTing to an MVC controller but rendering in Webforms
            if (http.Request.RequestContext.RouteData.DataTokens.ContainsKey(Mvc.Constants.DataTokenCurrentViewContext))
            {
                viewContext = (ViewContext)http.Request.RequestContext.RouteData.DataTokens[Mvc.Constants.DataTokenCurrentViewContext];
            }
            routeVals.DataTokens.Add("ParentActionViewContext", viewContext);

            var request = new RequestContext(http, routeVals);
            string output;
            using (var controller = new PartialViewMacroController(macro, content))
            {
				//bubble up the model state from the main view context to our custom controller.
				//when merging we'll create a new dictionary, otherwise you might run into an enumeration error
				// caused from ModelStateDictionary
				controller.ModelState.Merge(new ModelStateDictionary(viewContext.ViewData.ModelState));
				controller.ControllerContext = new ControllerContext(request, controller);
				//call the action to render
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
