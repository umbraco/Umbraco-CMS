using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Macros;

/// <summary>
///     A macro engine using MVC Partial Views to execute.
/// </summary>
public class PartialViewMacroEngine
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public PartialViewMacroEngine(
        IHttpContextAccessor httpContextAccessor,
        IModelMetadataProvider modelMetadataProvider,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _modelMetadataProvider = modelMetadataProvider;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

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

        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();

        RouteData currentRouteData = httpContext.GetRouteData();

        // Check if there's proxied ViewData (i.e. returned from a SurfaceController)
        ProxyViewDataFeature? proxyViewDataFeature = httpContext.Features.Get<ProxyViewDataFeature>();
        ViewDataDictionary viewData = proxyViewDataFeature?.ViewData ??
                                      new ViewDataDictionary(_modelMetadataProvider, new ModelStateDictionary());
        ITempDataDictionary tempData =
            proxyViewDataFeature?.TempData ?? _tempDataDictionaryFactory.GetTempData(httpContext);

        var viewContext = new ViewContext(
            new ActionContext(httpContext, currentRouteData, new ControllerActionDescriptor()),
            new FakeView(),
            viewData,
            tempData,
            TextWriter.Null,
            new HtmlHelperOptions());

        var writer = new StringWriter();
        var viewComponentContext = new ViewComponentContext(
            new ViewComponentDescriptor(),
            new Dictionary<string, object?>(),
            HtmlEncoder.Default,
            viewContext,
            writer);

        var viewComponent = new PartialViewMacroViewComponent(macro, content, viewComponentContext);

        viewComponent.Invoke().Execute(viewComponentContext);

        var output = writer.GetStringBuilder().ToString();

        return new MacroContent { Text = output };
    }

    private class FakeView : IView
    {
        /// <inheritdoc />
        public string Path { get; } = "View";

        /// <inheritdoc />
        public Task RenderAsync(ViewContext context) => Task.CompletedTask;
    }
}
