using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{

    /// <summary>
    /// Represents the default front-end rendering controller.
    /// </summary>
    [PreRenderViewActionFilter]
    [TypeFilter(typeof(ModelBindingExceptionFilter))]
    public class RenderMvcController : UmbracoController, IRenderMvcController
    {
        private IPublishedRequest _publishedRequest;
        private readonly ILogger<RenderMvcController> _logger;
        private readonly ICompositeViewEngine _compositeViewEngine;

        public RenderMvcController(ILogger<RenderMvcController> logger, ICompositeViewEngine compositeViewEngine)
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
                    return _publishedRequest;
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
            var result = _compositeViewEngine.FindView(ControllerContext, template, false);
            if (result.View != null) return true;

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
                throw new Exception("No physical template file was found for template " + template);
            return View(template, model);
        }

        /// <summary>
        /// The default action to render the front-end view.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [RenderIndexActionSelector]
        public virtual IActionResult Index(ContentModel model)
        {
            return CurrentTemplate(model);
        }
    }
}
