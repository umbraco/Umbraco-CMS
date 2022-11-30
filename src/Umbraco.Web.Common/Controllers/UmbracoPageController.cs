using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     An abstract controller for a front-end Umbraco page
/// </summary>
public abstract class UmbracoPageController : UmbracoController
{
    private readonly ICompositeViewEngine _compositeViewEngine;
    private readonly ILogger<UmbracoPageController> _logger;
    private UmbracoRouteValues? _umbracoRouteValues;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoPageController" /> class.
    /// </summary>
    protected UmbracoPageController(ILogger<UmbracoPageController> logger, ICompositeViewEngine compositeViewEngine)
    {
        _logger = logger;
        _compositeViewEngine = compositeViewEngine;
    }

    /// <summary>
    ///     Gets the <see cref="UmbracoRouteValues" />
    /// </summary>
    protected virtual UmbracoRouteValues UmbracoRouteValues
    {
        get
        {
            if (_umbracoRouteValues != null)
            {
                return _umbracoRouteValues;
            }

            _umbracoRouteValues = HttpContext.Features.Get<UmbracoRouteValues>();

            if (_umbracoRouteValues == null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(UmbracoRouteValues)} feature was found in the HttpContext");
            }

            return _umbracoRouteValues;
        }
    }

    /// <summary>
    ///     Gets the current content item.
    /// </summary>
    protected virtual IPublishedContent? CurrentPage
    {
        get
        {
            if (!UmbracoRouteValues.PublishedRequest.HasPublishedContent())
            {
                // This will never be accessed this way since the controller will handle redirects and not founds
                // before this can be accessed but we need to be explicit.
                throw new InvalidOperationException("There is no published content found in the request");
            }

            return UmbracoRouteValues.PublishedRequest.PublishedContent;
        }
    }

    /// <summary>
    ///     Gets an action result based on the template name found in the route values and a model.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model.</param>
    /// <returns>The action result.</returns>
    /// <exception cref="InvalidOperationException">
    ///     If the template found in the route values doesn't physically exist and
    ///     exception is thrown
    /// </exception>
    protected virtual IActionResult CurrentTemplate<T>(T model)
    {
        if (EnsurePhsyicalViewExists(UmbracoRouteValues.TemplateName) == false)
        {
            throw new InvalidOperationException("No physical template file was found for template " +
                                                UmbracoRouteValues.TemplateName);
        }

        return View(UmbracoRouteValues.TemplateName, model);
    }

    /// <summary>
    ///     Ensures that a physical view file exists on disk.
    /// </summary>
    /// <param name="template">The view name.</param>
    protected bool EnsurePhsyicalViewExists(string? template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            var docTypeAlias = UmbracoRouteValues.PublishedRequest.PublishedContent?.ContentType.Alias;
            _logger.LogWarning(
                "No physical template file was found for document type with alias {Alias}",
                docTypeAlias);
            return false;
        }

        ViewEngineResult result = _compositeViewEngine.FindView(ControllerContext, template, false);
        if (result.View != null)
        {
            return true;
        }

        _logger.LogWarning("No physical template file was found for template {Template}", template);
        return false;
    }
}
