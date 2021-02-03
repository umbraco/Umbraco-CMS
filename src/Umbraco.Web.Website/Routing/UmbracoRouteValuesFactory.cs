using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Features;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Web.Website.Routing
{

    /// <summary>
    /// Used to create <see cref="UmbracoRouteValues"/>
    /// </summary>
    public class UmbracoRouteValuesFactory : IUmbracoRouteValuesFactory
    {
        private readonly IUmbracoRenderingDefaults _renderingDefaults;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UmbracoFeatures _umbracoFeatures;
        private readonly IControllerActionSearcher _controllerActionSearcher;
        private readonly IPublishedRouter _publishedRouter;
        private readonly Lazy<string> _defaultControllerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRouteValuesFactory"/> class.
        /// </summary>
        public UmbracoRouteValuesFactory(
            IUmbracoRenderingDefaults renderingDefaults,
            IShortStringHelper shortStringHelper,
            UmbracoFeatures umbracoFeatures,
            IControllerActionSearcher controllerActionSearcher,
            IPublishedRouter publishedRouter)
        {
            _renderingDefaults = renderingDefaults;
            _shortStringHelper = shortStringHelper;
            _umbracoFeatures = umbracoFeatures;
            _controllerActionSearcher = controllerActionSearcher;
            _publishedRouter = publishedRouter;
            _defaultControllerName = new Lazy<string>(() => ControllerExtensions.GetControllerName(_renderingDefaults.DefaultControllerType));
        }

        /// <summary>
        /// Gets the default controller name
        /// </summary>
        protected string DefaultControllerName => _defaultControllerName.Value;

        /// <inheritdoc/>
        public UmbracoRouteValues Create(HttpContext httpContext, IPublishedRequest request)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Type defaultControllerType = _renderingDefaults.DefaultControllerType;

            string customActionName = null;

            // check that a template is defined), if it doesn't and there is a hijacked route it will just route
            // to the index Action
            if (request.HasTemplate())
            {
                // the template Alias should always be already saved with a safe name.
                // if there are hyphens in the name and there is a hijacked route, then the Action will need to be attributed
                // with the action name attribute.
                customActionName = request.GetTemplateAlias()?.Split('.')[0].ToSafeAlias(_shortStringHelper);
            }

            // creates the default route definition which maps to the 'UmbracoController' controller
            var def = new UmbracoRouteValues(
                request,
                DefaultControllerName,
                defaultControllerType,
                templateName: customActionName);

            def = CheckHijackedRoute(def);

            def = CheckNoTemplate(def);

            return def;
        }

        /// <summary>
        /// Check if the route is hijacked and return new route values
        /// </summary>
        private UmbracoRouteValues CheckHijackedRoute(UmbracoRouteValues def)
        {
            IPublishedRequest request = def.PublishedRequest;

            var customControllerName = request.PublishedContent?.ContentType?.Alias;
            if (customControllerName != null)
            {
                ControllerActionSearchResult hijackedResult = _controllerActionSearcher.Find<IRenderController>(customControllerName, def.TemplateName);
                if (hijackedResult.Success)
                {
                    return new UmbracoRouteValues(
                        request,
                        hijackedResult.ControllerName,
                        hijackedResult.ControllerType,
                        hijackedResult.ActionName,
                        def.TemplateName,
                        true);
                }
            }

            return def;
        }

        /// <summary>
        /// Special check for when no template or hijacked route is done which needs to re-run through the routing pipeline again for last chance finders
        /// </summary>
        private UmbracoRouteValues CheckNoTemplate(UmbracoRouteValues def)
        {
            IPublishedRequest request = def.PublishedRequest;

            // Here we need to check if there is no hijacked route and no template assigned but there is a content item.
            // If this is the case we want to return a blank page.
            // We also check if templates have been disabled since if they are then we're allowed to render even though there's no template,
            // for example for json rendering in headless.
            if (request.HasPublishedContent()
                && !request.HasTemplate()
                && !_umbracoFeatures.Disabled.DisableTemplates
                && !def.HasHijackedRoute)
            {
                Core.Models.PublishedContent.IPublishedContent content = request.PublishedContent;

                // This is basically a 404 even if there is content found.
                // We then need to re-run this through the pipeline for the last
                // chance finders to work.
                IPublishedRequestBuilder builder = _publishedRouter.UpdateRequestToNotFound(request);

                if (builder == null)
                {
                    throw new InvalidOperationException($"The call to {nameof(IPublishedRouter.UpdateRequestToNotFound)} cannot return null");
                }

                request = builder.Build();

                def = new UmbracoRouteValues(
                        request,
                        def.ControllerName,
                        def.ControllerType,
                        def.ActionName,
                        def.TemplateName);

                // if the content has changed, we must then again check for hijacked routes
                if (content != request.PublishedContent)
                {
                    def = CheckHijackedRoute(def);
                }
            }

            return def;
        }
    }
}
