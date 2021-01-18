using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.ActionsResults;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.Controllers
{
    /// <summary>
    /// Represents the default front-end rendering controller.
    /// </summary>
    [ModelBindingException]
    [PublishedRequestFilter]
    public class RenderController : UmbracoController, IRenderController
    {
        private readonly ILogger<RenderController> _logger;
        private readonly ICompositeViewEngine _compositeViewEngine;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private UmbracoRouteValues _umbracoRouteValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderController"/> class.
        /// </summary>
        public RenderController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _logger = logger;
            _compositeViewEngine = compositeViewEngine;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Gets the current content item.
        /// </summary>
        protected IPublishedContent CurrentPage
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
        /// Gets the umbraco context
        /// </summary>
        protected IUmbracoContext UmbracoContext => _umbracoContextAccessor.UmbracoContext;

        /// <summary>
        /// Gets the <see cref="UmbracoRouteValues"/>
        /// </summary>
        protected UmbracoRouteValues UmbracoRouteValues
        {
            get
            {
                if (_umbracoRouteValues != null)
                {
                    return _umbracoRouteValues;
                }

                _umbracoRouteValues = HttpContext.GetRouteValue(Core.Constants.Web.UmbracoRouteDefinitionDataToken) as UmbracoRouteValues;

                if (_umbracoRouteValues == null)
                {
                    throw new InvalidOperationException($"No route value found with key {Core.Constants.Web.UmbracoRouteDefinitionDataToken}");
                }

                return _umbracoRouteValues;
            }
        }

        /// <summary>
        /// Ensures that a physical view file exists on disk.
        /// </summary>
        /// <param name="template">The view name.</param>
        protected bool EnsurePhsyicalViewExists(string template)
        {
            ViewEngineResult result = _compositeViewEngine.FindView(ControllerContext, template, false);
            if (result.View != null)
            {
                return true;
            }

            _logger.LogWarning("No physical template file was found for template {Template}", template);
            return false;
        }

        /// <summary>
        /// Gets an action result based on the template name found in the route values and a model.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="model">The model.</param>
        /// <returns>The action result.</returns>
        /// <exception cref="InvalidOperationException">If the template found in the route values doesn't physically exist and exception is thrown</exception>
        protected IActionResult CurrentTemplate<T>(T model)
        {
            if (EnsurePhsyicalViewExists(UmbracoRouteValues.TemplateName) == false)
            {
                throw new InvalidOperationException("No physical template file was found for template " + UmbracoRouteValues.TemplateName);
            }

            return View(UmbracoRouteValues.TemplateName, model);
        }

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
