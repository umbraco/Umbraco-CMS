using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
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
    public class RenderController : UmbracoController, IRenderController
    {
        private readonly ILogger<RenderController> _logger;
        private readonly ICompositeViewEngine _compositeViewEngine;
        private UmbracoRouteValues _umbracoRouteValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderController"/> class.
        /// </summary>
        public RenderController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine)
        {
            _logger = logger;
            _compositeViewEngine = compositeViewEngine;
        }

        /// <summary>
        /// Gets the current content item.
        /// </summary>
        protected IPublishedContent CurrentPage => UmbracoRouteValues.PublishedContent;

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

                if (!ControllerContext.RouteData.Values.TryGetValue(Core.Constants.Web.UmbracoRouteDefinitionDataToken, out var def))
                {
                    throw new InvalidOperationException($"No route value found with key {Core.Constants.Web.UmbracoRouteDefinitionDataToken}");
                }

                _umbracoRouteValues = (UmbracoRouteValues)def;
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
    }
}
