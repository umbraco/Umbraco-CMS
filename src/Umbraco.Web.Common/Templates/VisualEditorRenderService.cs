using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Templates;

internal sealed class VisualEditorRenderService : IVisualEditorRenderService
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IPublishedRouter _publishedRouter;
    private readonly ITemplateService _templateService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICompositeViewEngine _viewEngine;
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IVisualEditorContentFactory _contentFactory;

    public VisualEditorRenderService(
        IUmbracoContextFactory umbracoContextFactory,
        IPublishedRouter publishedRouter,
        ITemplateService templateService,
        IHttpContextAccessor httpContextAccessor,
        ICompositeViewEngine viewEngine,
        IModelMetadataProvider modelMetadataProvider,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IVisualEditorContentFactory contentFactory)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _publishedRouter = publishedRouter;
        _templateService = templateService;
        _httpContextAccessor = httpContextAccessor;
        _viewEngine = viewEngine;
        _modelMetadataProvider = modelMetadataProvider;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _contentFactory = contentFactory;
    }

    public async Task<string> RenderAsync(
        Guid documentKey,
        string? culture,
        string? segment,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides)
    {
        using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
        IUmbracoContext umbracoContext = contextReference.UmbracoContext;

        IPublishedContent? content = await _contentFactory.CreateWithOverridesAsync(documentKey, overrides);
        if (content?.TemplateId is null)
        {
            return string.Empty;
        }

        ITemplate? template = await _templateService.GetAsync(content.TemplateId.Value);
        if (template is null)
        {
            return string.Empty;
        }

        IPublishedRequestBuilder requestBuilder = await _publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
        requestBuilder.SetCulture(culture);
        requestBuilder.SetSegment(segment);
        requestBuilder.SetPublishedContent(content);
        requestBuilder.SetTemplate(template);
        IPublishedRequest request = requestBuilder.Build();

        IPublishedRequest? oldRequest = umbracoContext.PublishedRequest;
        VisualEditorPropertyTracker.Enable();
        try
        {
            umbracoContext.PublishedRequest = request;
            return ExecuteTemplateRendering(request);
        }
        finally
        {
            VisualEditorPropertyTracker.Disable();
            umbracoContext.PublishedRequest = oldRequest;
        }
    }

    private string ExecuteTemplateRendering(IPublishedRequest request)
    {
        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();

        // isMainPage is set to true here to ensure ViewStart(s) found in the view hierarchy are rendered
        ViewEngineResult viewResult = _viewEngine.GetView(null, $"~/Views/{request.GetTemplateAlias()}.cshtml", true);
        if (viewResult.View is null)
        {
            return string.Empty;
        }

        var viewData = new ViewDataDictionary(_modelMetadataProvider, new ModelStateDictionary())
        {
            Model = request.PublishedContent,
        };

        using var writer = new StringWriter();
        var viewContext = new ViewContext(
            new ActionContext(httpContext, httpContext.GetRouteData(), new ControllerActionDescriptor()),
            viewResult.View,
            viewData,
            _tempDataDictionaryFactory.GetTempData(httpContext),
            writer,
            new HtmlHelperOptions());

        viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
        return writer.GetStringBuilder().ToString();
    }
}
