using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Umbraco.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// A macro engine using MVC Partial Views to execute.
    /// </summary>
    public class PartialViewMacroEngine
    {
        private readonly Func<HttpContextBase> _getHttpContext;
        private readonly Func<UmbracoContext> _getUmbracoContext;

        public PartialViewMacroEngine()
        {
            _getHttpContext = () =>
            {
                if (HttpContext.Current == null)
                    throw new InvalidOperationException($"The {GetType()} cannot execute with a null HttpContext.Current reference.");
                return new HttpContextWrapper(HttpContext.Current);
            };

            _getUmbracoContext = () =>
            {
                if (UmbracoContext.Current == null)
                    throw new InvalidOperationException($"The {GetType()} cannot execute with a null UmbracoContext.Current reference.");
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

        public bool Validate(string code, string tempFileName, IPublishedContent currentPage, out string errorMessage)
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

        public MacroContent Execute(MacroModel macro, IPublishedContent content)
        {
            if (macro == null) throw new ArgumentNullException(nameof(macro));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (macro.MacroSource.IsNullOrWhiteSpace()) throw new ArgumentException("The MacroSource property of the macro object cannot be null or empty");

            var http = _getHttpContext();
            var umbCtx = _getUmbracoContext();
            var routeVals = new RouteData();
            routeVals.Values.Add("controller", "PartialViewMacro");
            routeVals.Values.Add("action", "Index");
            routeVals.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbCtx); //required for UmbracoViewPage

            //lets render this controller as a child action
            var viewContext = new ViewContext { ViewData = new ViewDataDictionary() };
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
                controller.ViewData = viewContext.ViewData;

                controller.ControllerContext = new ControllerContext(request, controller);

                //call the action to render
                var result = controller.Index();
                output = controller.RenderViewResultAsString(result);
            }

            return new MacroContent { Text = output };
        }

        private string GetVirtualPathFromPhysicalPath(string physicalPath)
        {
            var rootpath = _getHttpContext().Server.MapPath("~/");
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
