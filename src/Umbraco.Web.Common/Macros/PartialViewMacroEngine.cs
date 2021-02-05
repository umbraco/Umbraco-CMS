using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Umbraco.Web.Macros;
using static Umbraco.Core.Constants.Web.Routing;

namespace Umbraco.Web.Common.Macros
{
    /// <summary>
    /// A macro engine using MVC Partial Views to execute.
    /// </summary>
    public class PartialViewMacroEngine
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        //private readonly Func<IUmbracoContext> _getUmbracoContext;

        public PartialViewMacroEngine(
            //IUmbracoContextAccessor umbracoContextAccessor,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;

            //_getUmbracoContext = () =>
            //{
            //    var context = umbracoContextAccessor.UmbracoContext;
            //    if (context == null)
            //    {
            //        throw new InvalidOperationException(
            //            $"The {GetType()} cannot execute with a null UmbracoContext.Current reference.");
            //    }

            //    return context;
            //};
        }

        //public bool Validate(string code, string tempFileName, IPublishedContent currentPage, out string errorMessage)
        //{
        //    var temp = GetVirtualPathFromPhysicalPath(tempFileName);
        //    try
        //    {
        //        CompileAndInstantiate(temp);
        //    }
        //    catch (Exception exception)
        //    {
        //        errorMessage = exception.Message;
        //        return false;
        //    }

        //    errorMessage = string.Empty;
        //    return true;
        //}

        public MacroContent Execute(MacroModel macro, IPublishedContent content)
        {
            if (macro == null)
            {
                throw new ArgumentNullException(nameof(macro));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (string.IsNullOrWhiteSpace(macro.MacroSource))
            {
                throw new ArgumentException("The MacroSource property of the macro object cannot be null or empty");
            }

            var httpContext = _httpContextAccessor.GetRequiredHttpContext();
            //var umbCtx = _getUmbracoContext();
            var routeVals = new RouteData();
            routeVals.Values.Add(ControllerToken, "PartialViewMacro");
            routeVals.Values.Add(ActionToken, "Index");

            //TODO: Was required for UmbracoViewPage need to figure out if we still need that, i really don't think this is necessary
            //routeVals.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbCtx); 

            var modelMetadataProvider = httpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
            var tempDataProvider = httpContext.RequestServices.GetRequiredService<ITempDataProvider>();

            var viewContext = new ViewContext(
                new ActionContext(httpContext, httpContext.GetRouteData(), new ControllerActionDescriptor()),
                new FakeView(),
                new ViewDataDictionary(modelMetadataProvider, new ModelStateDictionary()),
                new TempDataDictionary(httpContext, tempDataProvider),
                TextWriter.Null,
                new HtmlHelperOptions()
            );


            routeVals.DataTokens.Add("ParentActionViewContext", viewContext);

            var viewComponent = new PartialViewMacroViewComponent(macro, content);

            var writer = new StringWriter();
            var viewComponentContext = new ViewComponentContext(
                new ViewComponentDescriptor(),
                new Dictionary<string, object>(),
                HtmlEncoder.Default,
                viewContext,
                writer);

            viewComponent.InvokeAsync().GetAwaiter().GetResult().Execute(viewComponentContext);

            var output = writer.GetStringBuilder().ToString();

            return new MacroContent { Text = output };
        }

        private class FakeView : IView
        {
            /// <inheritdoc />
            public Task RenderAsync(ViewContext context)
            {
                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public string Path { get; } = "View";
        }

        //private string GetVirtualPathFromPhysicalPath(string physicalPath)
        //{
        //    var rootpath = _hostingEnvironment.MapPathContentRoot("~/");
        //    physicalPath = physicalPath.Replace(rootpath, "");
        //    physicalPath = physicalPath.Replace("\\", "/");
        //    return "~/" + physicalPath;
        //}

        //private static PartialViewMacroPage CompileAndInstantiate(string virtualPath)
        //{
        //    // //Compile Razor - We Will Leave This To ASP.NET Compilation Engine & ASP.NET WebPages
        //    // //Security in medium trust is strict around here, so we can only pass a virtual file path
        //    // //ASP.NET Compilation Engine caches returned types
        //    // //Changed From BuildManager As Other Properties Are Attached Like Context Path/
        //    // var webPageBase = WebPageBase.CreateInstanceFromVirtualPath(virtualPath);
        //    // var webPage = webPageBase as PartialViewMacroPage;
        //    // if (webPage == null)
        //    //     throw new InvalidCastException("All Partial View Macro views must inherit from " + typeof(PartialViewMacroPage).FullName);
        //    // return webPage;

        //    //TODO? How to check this
        //    return null;
        //}
    }

}
