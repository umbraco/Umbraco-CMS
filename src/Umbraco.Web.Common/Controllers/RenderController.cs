using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Web.Common.ActionsResults;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.Controllers
{

    /// <summary>
    /// Represents the default front-end rendering controller.
    /// </summary>
    [ModelBindingException]
    [PublishedRequestFilter]
    public class RenderController : UmbracoPageController, IRenderController
    {
        private readonly ILogger<RenderController> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderController"/> class.
        /// </summary>
        public RenderController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Gets the umbraco context
        /// </summary>
        protected IUmbracoContext UmbracoContext => _umbracoContextAccessor.UmbracoContext;

        /// <summary>
        /// The default action to render the front-end view.
        /// </summary>
        public virtual IActionResult Index() => CurrentTemplate(new ContentModel(CurrentPage));

        /// <summary>
        /// Before the controller executes we will handle redirects and not founds
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
                        ? RedirectPermanent(pcr.RedirectUrl)
                        : Redirect(pcr.RedirectUrl);
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
                    ProxyViewDataFeature saveViewData = HttpContext.Features.Get<ProxyViewDataFeature>();
                    if (saveViewData != null)
                    {
                        foreach (KeyValuePair<string, object> kv in saveViewData.ViewData)
                        {
                            ViewData[kv.Key] = kv.Value;
                        }
                    }

                    // continue normally
                    await next();
                    break;
            }
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
            else if (!pcr.HasTemplate())
            {
                // means the engine could find a proper document, but the document has no template
                // at that point there isn't much we can do
                return new PublishedContentNotFoundResult(
                    UmbracoContext,
                    "In addition, no template exists to render the custom 404.");
            }
            else
            {
                return new PublishedContentNotFoundResult(UmbracoContext);
            }
        }
    }
}
