using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.UI.Custom.VirtualPage;

/// <summary>
///     Reproduction harness for https://github.com/umbraco/Umbraco-CMS/issues/14165.
///     A custom-routed controller that presents itself as an Umbraco page via
///     <see cref="IVirtualPageController" />, so we can verify whether a form rendered on the
///     page (and posted to a surface controller) submits and redirects correctly.
///
///     This variant resolves the item the way the official docs example does — reading the route
///     value from <see cref="ActionExecutingContext.ActionArguments" /> — to verify whether that
///     pattern works on the surface-controller POST (where SetupVirtualPageRoute builds the
///     ActionExecutingContext itself). It logs what it can read from ActionArguments vs RouteData
///     on every call so GET and POST can be compared.
///
///     Hit "/products/ABC123" (any SKU) to render the page, then submit the enquiry form.
/// </summary>
[Route("products")]
public class ProductVirtualPageController : UmbracoPageController, IVirtualPageController
{
    private readonly IPublishedContentQuery _publishedContentQuery;
    private readonly ILogger<ProductVirtualPageController> _logger;

    public ProductVirtualPageController(
        ILogger<ProductVirtualPageController> logger,
        ICompositeViewEngine compositeViewEngine,
        IPublishedContentQuery publishedContentQuery)
        : base(logger, compositeViewEngine)
    {
        _publishedContentQuery = publishedContentQuery;
        _logger = logger;
    }

    [HttpGet("{sku}")]
    public IActionResult Index(string sku)
        => View("~/Views/ProductVirtualPage.cshtml", new ProductPageViewModel(CurrentPage!, sku));

    /// <summary>
    ///     Docs-style content resolution: read the identifier from <see cref="ActionExecutingContext.ActionArguments" />.
    ///     Returns null (→ 404) when the identifier cannot be read, so a POST that fails to populate
    ///     ActionArguments will visibly fail. The log line shows exactly what was available.
    /// </summary>
    public IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext)
    {
        var skuFromActionArguments = actionExecutingContext.ActionArguments.TryGetValue("sku", out var argValue)
            ? argValue?.ToString()
            : null;
        var skuFromRouteData = actionExecutingContext.RouteData.Values.TryGetValue("sku", out var routeValue)
            ? routeValue?.ToString()
            : null;

        _logger.LogInformation(
            "FindContent #14165: method={Method} action={Action} skuFromActionArguments={FromArguments} skuFromRouteData={FromRouteData}",
            actionExecutingContext.HttpContext.Request.Method,
            (actionExecutingContext.ActionDescriptor as ControllerActionDescriptor)?.ActionName,
            skuFromActionArguments ?? "(null)",
            skuFromRouteData ?? "(null)");

        // Resolve using the value the docs read from (ActionArguments). If it is not there we cannot
        // identify the product, so this returns null - the behaviour under scrutiny for the POST path.
        if (string.IsNullOrEmpty(skuFromActionArguments))
        {
            return null;
        }

        return _publishedContentQuery.ContentAtRoot().FirstOrDefault();
    }
}
