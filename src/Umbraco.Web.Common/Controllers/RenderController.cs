using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Represents the default front-end rendering controller.
/// </summary>
[ModelBindingException]
[PublishedRequestFilter]
public class RenderController : UmbracoPageController, IRenderController
{
    private readonly ILogger<RenderController> _logger;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderController" /> class.
    /// </summary>
    public RenderController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
        : base(logger, compositeViewEngine)
    {
        _logger = logger;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    /// <summary>
    ///     Gets the umbraco context
    /// </summary>
    protected IUmbracoContext UmbracoContext
    {
        get
        {
            IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            return umbracoContext;
        }
    }

    /// <summary>
    ///     The default action to render the front-end view.
    /// </summary>
    public virtual IActionResult Index() => CurrentTemplate(new ContentModel(CurrentPage));

    /// <summary>
    ///     Before the controller executes we will handle redirects and not founds
    /// </summary>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        IPublishedRequest pcr = UmbracoRouteValues.PublishedRequest;

        _logger.LogDebug(
            "Response status: Content={Content}, StatusCode={ResponseStatusCode}, Culture={Culture}",
            pcr.PublishedContent?.Id ?? -1,
            pcr.ResponseStatusCode,
            pcr.Culture);

        UmbracoRouteResult routeStatus = pcr.GetRouteResult();
        switch (routeStatus)
        {
            case UmbracoRouteResult.Redirect:

                // set the redirect result and do not call next to short circuit
                context.Result = pcr.IsRedirectPermanent()
                    ? RedirectPermanent(pcr.RedirectUrl!)
                    : Redirect(pcr.RedirectUrl!);
                break;
            case UmbracoRouteResult.NotFound:
                // set the redirect result and do not call next to short circuit
                context.Result = GetNoTemplateResult(pcr);
                break;
            case UmbracoRouteResult.Success:
            default:

                // Check if there's a ProxyViewDataFeature in the request.
                // If there it is means that we are proxying/executing this controller
                // from another controller and we need to merge it's ViewData with this one
                // since this one will be empty.
                ProxyViewDataFeature? saveViewData = HttpContext.Features.Get<ProxyViewDataFeature>();
                if (saveViewData != null)
                {
                    foreach (KeyValuePair<string, object?> kv in saveViewData.ViewData)
                    {
                        ViewData[kv.Key] = kv.Value;
                    }
                }

                // continue normally
                await next();
                break;
        }
    }

    /// <summary>
    ///     Gets an action result based on the template name found in the route values and a model.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model.</param>
    /// <returns>The action result.</returns>
    /// <remarks>
    ///     If the template found in the route values doesn't physically exist, Umbraco not found result is returned.
    /// </remarks>
    protected override IActionResult CurrentTemplate<T>(T model)
    {
        if (EnsurePhsyicalViewExists(UmbracoRouteValues.TemplateName) == false)
        {
            // no physical template file was found
            return new PublishedContentNotFoundResult(UmbracoContext);
        }

        return View(UmbracoRouteValues.TemplateName, model);
    }

    private PublishedContentNotFoundResult GetNoTemplateResult(IPublishedRequest pcr)
    {
        // missing template, so we're in a 404 here
        // so the content, if any, is a custom 404 page of some sort
        if (!pcr.HasPublishedContent())
        {
            // means the builder could not find a proper document to handle 404
            return new PublishedContentNotFoundResult(UmbracoContext);
        }

        if (!pcr.HasTemplate())
        {
            // means the engine could find a proper document, but the document has no template
            // at that point there isn't much we can do
            return new PublishedContentNotFoundResult(
                UmbracoContext,
                "In addition, no template exists to render the custom 404.");
        }

        return new PublishedContentNotFoundResult(UmbracoContext);
    }
}
