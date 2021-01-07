using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UmbracoRouteValuesFactory> _logger;
        private readonly IUmbracoRenderingDefaults _renderingDefaults;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UmbracoFeatures _umbracoFeatures;
        private readonly HijackedRouteEvaluator _hijackedRouteEvaluator;
        private readonly Lazy<string> _defaultControllerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRouteValuesFactory"/> class.
        /// </summary>
        public UmbracoRouteValuesFactory(
            ILogger<UmbracoRouteValuesFactory> logger,
            IUmbracoRenderingDefaults renderingDefaults,
            IShortStringHelper shortStringHelper,
            UmbracoFeatures umbracoFeatures,
            HijackedRouteEvaluator hijackedRouteEvaluator)
        {
            _logger = logger;
            _renderingDefaults = renderingDefaults;
            _shortStringHelper = shortStringHelper;
            _umbracoFeatures = umbracoFeatures;
            _hijackedRouteEvaluator = hijackedRouteEvaluator;
            _defaultControllerName = new Lazy<string>(() => ControllerExtensions.GetControllerName(_renderingDefaults.DefaultControllerType));
        }

        /// <summary>
        /// Gets the default controller name
        /// </summary>
        protected string DefaultControllerName => _defaultControllerName.Value;

        /// <inheritdoc/>
        public UmbracoRouteValues Create(HttpContext httpContext, RouteValueDictionary values, IPublishedRequest request)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
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

            var customControllerName = request.PublishedContent?.ContentType.Alias;
            if (customControllerName != null)
            {
                HijackedRouteResult hijackedResult = _hijackedRouteEvaluator.Evaluate(customControllerName, customActionName);
                if (hijackedResult.Success)
                {
                    def = new UmbracoRouteValues(
                        request,
                        hijackedResult.ControllerName,
                        hijackedResult.ControllerType,
                        hijackedResult.ActionName,
                        customActionName,
                        true);
                }
            }

            // Here we need to check if there is no hijacked route and no template assigned,
            // if this is the case we want to return a blank page.
            // We also check if templates have been disabled since if they are then we're allowed to render even though there's no template,
            // for example for json rendering in headless.
            if (!request.HasTemplate()
                && !_umbracoFeatures.Disabled.DisableTemplates
                && !def.HasHijackedRoute)
            {
                // TODO: this is basically a 404, in v8 this will re-run the pipeline
                // in order to see if a last chance finder finds some content
                // At this point we're already done building the request and we have an immutable
                // request. in v8 it was re-mutated :/ I really don't want to do that

                // In this case we'll render the NoTemplate action
                def = new UmbracoRouteValues(
                        request,
                        DefaultControllerName,
                        defaultControllerType,
                        nameof(RenderController.NoTemplate));
            }

            // store the route definition
            values.TryAdd(Constants.Web.UmbracoRouteDefinitionDataToken, def);

            return def;
        }
    }
}
