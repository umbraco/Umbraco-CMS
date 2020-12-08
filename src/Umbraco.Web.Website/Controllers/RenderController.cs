using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Website.Controllers
{

    /// <summary>
    /// Represents the default front-end rendering controller.
    /// </summary>
    [TypeFilter(typeof(ModelBindingExceptionFilter))]
    public class RenderController : UmbracoController, IRenderController
    {
        private IPublishedRequest _publishedRequest;
        private readonly ILogger<RenderController> _logger;
        private readonly ICompositeViewEngine _compositeViewEngine;

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
        protected IPublishedContent CurrentPage => PublishedRequest.PublishedContent;

        /// <summary>
        /// Gets the current published content request.
        /// </summary>
        protected internal virtual IPublishedRequest PublishedRequest
        {
            get
            {
                if (_publishedRequest != null)
                {
                    return _publishedRequest;
                }

                if (RouteData.DataTokens.ContainsKey(Core.Constants.Web.PublishedDocumentRequestDataToken) == false)
                {
                    throw new InvalidOperationException("DataTokens must contain an 'umbraco-doc-request' key with a PublishedRequest object");
                }

                _publishedRequest = (IPublishedRequest)RouteData.DataTokens[Core.Constants.Web.PublishedDocumentRequestDataToken];
                return _publishedRequest;
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
        /// <remarks>If the template found in the route values doesn't physically exist, then an empty ContentResult will be returned.</remarks>
        protected IActionResult CurrentTemplate<T>(T model)
        {
            var template = ControllerContext.RouteData.Values["action"].ToString();
            if (EnsurePhsyicalViewExists(template) == false)
            {
                throw new InvalidOperationException("No physical template file was found for template " + template);
            }

            return View(template, model);
        }

        /// <summary>
        /// The default action to render the front-end view.
        /// </summary>
        [RenderIndexActionSelector]
        public virtual IActionResult Index(ContentModel model) => CurrentTemplate(model);
    }
}
