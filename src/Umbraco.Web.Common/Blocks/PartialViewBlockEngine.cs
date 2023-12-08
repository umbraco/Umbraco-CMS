using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Blocks;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Blocks;

internal sealed class PartialViewBlockEngine : IPartialViewBlockEngine
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public PartialViewBlockEngine(
        IHttpContextAccessor httpContextAccessor,
        IModelMetadataProvider modelMetadataProvider,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _modelMetadataProvider = modelMetadataProvider;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

    public async Task<string> ExecuteAsync(IBlockReference<IPublishedElement, IPublishedElement> blockReference)
    {
        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();
        RouteData currentRouteData = httpContext.GetRouteData();

        // Check if there's proxied ViewData (i.e. returned from a SurfaceController)
        ProxyViewDataFeature? proxyViewDataFeature = httpContext.Features.Get<ProxyViewDataFeature>();
        ViewDataDictionary viewData = proxyViewDataFeature?.ViewData
                                      ?? new ViewDataDictionary(_modelMetadataProvider, new ModelStateDictionary());
        viewData.Model = blockReference;

        ITempDataDictionary tempData = proxyViewDataFeature?.TempData
                                       ?? _tempDataDictionaryFactory.GetTempData(httpContext);

        var actionContext = new ActionContext(httpContext, currentRouteData, new ControllerActionDescriptor());
        IRazorViewEngine razorViewEngine = httpContext.RequestServices.GetRequiredService<IRazorViewEngine>();

        var viewPath = $"~/Views/Partials/richtext/Components/{blockReference.Content.ContentType.Alias}.cshtml";
        ViewEngineResult viewResult = razorViewEngine.GetView(null, viewPath, false);

        if (viewResult.View is null)
        {
            throw new ArgumentException($"{viewPath} does not match any available view");
        }

        await using var writer = new StringWriter();

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);

        return writer.ToString();
    }
}
