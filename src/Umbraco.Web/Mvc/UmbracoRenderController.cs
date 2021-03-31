using System;
using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The base class for front-end rendering controllers.
    /// </summary>
    [PreRenderViewActionFilter]
    [ModelBindingExceptionFilter]
    public abstract class UmbracoRenderController : UmbracoController, IRenderController
    {
        // TODO: In vNext, make this the default controller, add Index, and remove RenderMvcController

        private PublishedRequest _publishedRequest;

        public UmbracoRenderController()
        {
            ActionInvoker = new RenderActionInvoker();
        }

        protected UmbracoRenderController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper) : base(globalSettings, umbracoContextAccessor, services, appCaches, profilingLogger, umbracoHelper)
        {
            ActionInvoker = new RenderActionInvoker();
        }

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public override UmbracoContext UmbracoContext => PublishedRequest.UmbracoContext; //TODO: Why?

        /// <summary>
        /// Gets the current content item.
        /// </summary>
        protected IPublishedContent CurrentPage => PublishedRequest.PublishedContent;

        /// <summary>
        /// Gets the current published content request.
        /// </summary>
        protected internal virtual PublishedRequest PublishedRequest
        {
            // TODO: I think this is legacy from v7, we can 'just' use UmbracoContext.PublishedRequest.PublishedContent
            // I think perhaps in v7 it was required due to either mixing webforms and/or postbacks in some way.

            get
            {
                if (_publishedRequest != null)
                    return _publishedRequest;
                if (RouteData.DataTokens.ContainsKey(Core.Constants.Web.PublishedDocumentRequestDataToken) == false)
                {
                    throw new InvalidOperationException("DataTokens must contain an 'umbraco-doc-request' key with a PublishedRequest object");
                }
                _publishedRequest = (PublishedRequest)RouteData.DataTokens[Core.Constants.Web.PublishedDocumentRequestDataToken];
                return _publishedRequest;
            }
        }

        /// <summary>
        /// Ensures that a physical view file exists on disk.
        /// </summary>
        /// <param name="template">The view name.</param>
        protected bool EnsurePhsyicalViewExists(string template)
        {
            var result = ViewEngines.Engines.FindView(ControllerContext, template, null);
            if (result.View != null) return true;

            Logger.Warn<RenderMvcController>("No physical template file was found for template {Template}", template);
            return false;
        }

        /// <summary>
        /// Gets an action result based on the template name found in the route values and a model.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="model">The model.</param>
        /// <returns>The action result.</returns>
        /// <remarks>If the template found in the route values doesn't physically exist, then an empty ContentResult will be returned.</remarks>
        protected ActionResult CurrentTemplate<T>(T model)
        {
            var template = ControllerContext.RouteData.Values["action"].ToString();
            if (EnsurePhsyicalViewExists(template) == false)
                throw new Exception("No physical template file was found for template " + template);
            return View(template, model);
        }

    }
}
