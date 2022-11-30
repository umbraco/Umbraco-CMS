using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Templates;

/// <summary>
///     This is used purely for the RenderTemplate functionality in Umbraco
/// </summary>
/// <remarks>
///     This allows you to render an MVC template based purely off of a node id and an optional alttemplate id as string
///     output.
/// </remarks>
internal class TemplateRenderer : ITemplateRenderer
{
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _languageService;
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly IPublishedRouter _publishedRouter;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ICompositeViewEngine _viewEngine;
    private WebRoutingSettings _webRoutingSettings;

    public TemplateRenderer(
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedRouter publishedRouter,
        IFileService fileService,
        ILocalizationService textService,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IHttpContextAccessor httpContextAccessor,
        ICompositeViewEngine viewEngine,
        IModelMetadataProvider modelMetadataProvider,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _umbracoContextAccessor =
            umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        _publishedRouter = publishedRouter ?? throw new ArgumentNullException(nameof(publishedRouter));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _languageService = textService ?? throw new ArgumentNullException(nameof(textService));
        _webRoutingSettings = webRoutingSettings.CurrentValue ??
                              throw new ArgumentNullException(nameof(webRoutingSettings));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        _modelMetadataProvider = modelMetadataProvider;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;

        webRoutingSettings.OnChange(x => _webRoutingSettings = x);
    }

    public async Task RenderAsync(int pageId, int? altTemplateId, StringWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();

        // instantiate a request and process
        // important to use CleanedUmbracoUrl - lowercase path-only version of the current URL, though this isn't going to matter
        // terribly much for this implementation since we are just creating a doc content request to modify it's properties manually.
        IPublishedRequestBuilder requestBuilder =
            await _publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);

        IPublishedContent? doc = umbracoContext.Content?.GetById(pageId);

        if (doc == null)
        {
            writer.Write("<!-- Could not render template for Id {0}, the document was not found -->", pageId);
            return;
        }

        // in some cases the UmbracoContext will not have a PublishedRequest assigned to it if we are not in the
        // execution of a front-end rendered page. In this case set the culture to the default.
        // set the culture to the same as is currently rendering
        if (umbracoContext.PublishedRequest == null)
        {
            ILanguage? defaultLanguage = _languageService.GetAllLanguages().FirstOrDefault();

            requestBuilder.SetCulture(defaultLanguage == null
                ? CultureInfo.CurrentUICulture.Name
                : defaultLanguage.IsoCode);
        }
        else
        {
            requestBuilder.SetCulture(umbracoContext.PublishedRequest.Culture);
        }

        // set the doc that was found by id
        requestBuilder.SetPublishedContent(doc);

        // set the template, either based on the AltTemplate found or the standard template of the doc
        var templateId = _webRoutingSettings.DisableAlternativeTemplates || !altTemplateId.HasValue
            ? doc.TemplateId
            : altTemplateId.Value;

        if (templateId.HasValue)
        {
            requestBuilder.SetTemplate(_fileService.GetTemplate(templateId.Value));
        }

        // if there is not template then exit
        if (requestBuilder.HasTemplate() == false)
        {
            if (altTemplateId.HasValue == false)
            {
                writer.Write(
                    "<!-- Could not render template for Id {0}, the document's template was not found with id {0}-->",
                    doc.TemplateId);
            }
            else
            {
                writer.Write(
                    "<!-- Could not render template for Id {0}, the altTemplate was not found with id {0}-->",
                    altTemplateId);
            }

            return;
        }

        // First, save all of the items locally that we know are used in the chain of execution, we'll need to restore these
        // after this page has rendered.
        SaveExistingItems(out IPublishedRequest? oldPublishedRequest);

        IPublishedRequest contentRequest = requestBuilder.Build();

        try
        {
            // set the new items on context objects for this templates execution
            SetNewItemsOnContextObjects(contentRequest);

            // Render the template
            ExecuteTemplateRendering(writer, contentRequest);
        }
        finally
        {
            // restore items on context objects to continuing rendering the parent template
            RestoreItems(oldPublishedRequest);
        }
    }

    private void ExecuteTemplateRendering(TextWriter sw, IPublishedRequest request)
    {
        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();

        // isMainPage is set to true here to ensure ViewStart(s) found in the view hierarchy are rendered
        ViewEngineResult viewResult = _viewEngine.GetView(null, $"~/Views/{request.GetTemplateAlias()}.cshtml", true);

        if (viewResult.Success == false)
        {
            throw new InvalidOperationException(
                $"A view with the name {request.GetTemplateAlias()} could not be found");
        }

        var viewData = new ViewDataDictionary(_modelMetadataProvider, new ModelStateDictionary())
        {
            Model = request.PublishedContent,
        };

        var writer = new StringWriter();
        var viewContext = new ViewContext(
            new ActionContext(httpContext, httpContext.GetRouteData(), new ControllerActionDescriptor()),
            viewResult.View,
            viewData,
            _tempDataDictionaryFactory.GetTempData(httpContext),
            writer,
            new HtmlHelperOptions());

        viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();

        var output = writer.GetStringBuilder().ToString();

        sw.Write(output);
    }

    // TODO: I feel like we need to do more than this, pretty sure we need to replace the UmbracoRouteValues
    // HttpRequest feature too while this renders.
    private void SetNewItemsOnContextObjects(IPublishedRequest request)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();

        // now, set the new ones for this page execution
        umbracoContext.PublishedRequest = request;
    }

    /// <summary>
    ///     Save all items that we know are used for rendering execution to variables so we can restore after rendering
    /// </summary>
    private void SaveExistingItems(out IPublishedRequest? oldPublishedRequest)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();

        // Many objects require that these legacy items are in the http context items... before we render this template we need to first
        // save the values in them so that we can re-set them after we render so the rest of the execution works as per normal
        oldPublishedRequest = umbracoContext.PublishedRequest;
    }

    /// <summary>
    ///     Restores all items back to their context's to continue normal page rendering execution
    /// </summary>
    private void RestoreItems(IPublishedRequest? oldPublishedRequest)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        umbracoContext.PublishedRequest = oldPublishedRequest;
    }
}
