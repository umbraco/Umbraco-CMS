using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Middleware;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// The route value transformer for Umbraco front-end routes
    /// </summary>
    public class UmbracoRouteValueTransformer : DynamicRouteValueTransformer
    {
        private readonly ILogger<UmbracoRouteValueTransformer> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IUmbracoRenderingDefaults _renderingDefaults;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly IPublishedRouter _publishedRouter;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRouteValueTransformer"/> class.
        /// </summary>
        public UmbracoRouteValueTransformer(
            ILogger<UmbracoRouteValueTransformer> logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoRenderingDefaults renderingDefaults,
            IShortStringHelper shortStringHelper,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IPublishedRouter publishedRouter)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _renderingDefaults = renderingDefaults;
            _shortStringHelper = shortStringHelper;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _publishedRouter = publishedRouter;
        }

        /// <inheritdoc/>
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (_umbracoContextAccessor.UmbracoContext == null)
            {
                throw new InvalidOperationException($"There is no current UmbracoContext, it must be initialized before the {nameof(UmbracoRouteValueTransformer)} executes, ensure that {nameof(UmbracoRequestMiddleware)} is registered prior to 'UseRouting'");
            }

            bool routed = RouteRequest(_umbracoContextAccessor.UmbracoContext);
            if (!routed)
            {
                // TODO: Deal with it not being routable, perhaps this should be an enum result?
            }

            IPublishedRequest request = _umbracoContextAccessor.UmbracoContext.PublishedRequest; // This cannot be null here

            SetupRouteDataForRequest(
                new ContentModel(request.PublishedContent),
                request,
                values);

            RouteDefinition routeDef = GetUmbracoRouteDefinition(httpContext, values, request);
            values["controller"] = routeDef.ControllerName;
            if (string.IsNullOrWhiteSpace(routeDef.ActionName) == false)
            {
                values["action"] = routeDef.ActionName;
            }

            return await Task.FromResult(values);
        }

        /// <summary>
        /// Ensures that all of the correct DataTokens are added to the route values which are all required for rendering front-end umbraco views
        /// </summary>
        private void SetupRouteDataForRequest(ContentModel contentModel, IPublishedRequest frequest, RouteValueDictionary values)
        {
            // put essential data into the data tokens, the 'umbraco' key is required to be there for the view engine

            // required for the ContentModelBinder and view engine.
            // TODO: Are we sure, seems strange to need this in netcore
            values.TryAdd(Constants.Web.UmbracoDataToken, contentModel);

            // required for RenderMvcController
            // TODO: Are we sure, seems strange to need this in netcore
            values.TryAdd(Constants.Web.PublishedDocumentRequestDataToken, frequest);

            // required for UmbracoViewPage
            // TODO: Are we sure, seems strange to need this in netcore
            values.TryAdd(Constants.Web.UmbracoContextDataToken, _umbracoContextAccessor.UmbracoContext);
        }

        /// <summary>
        /// Returns a <see cref="RouteDefinition"/> object based on the current content request
        /// </summary>
        private RouteDefinition GetUmbracoRouteDefinition(HttpContext httpContext, RouteValueDictionary values, IPublishedRequest request)
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
            var defaultControllerName = ControllerExtensions.GetControllerName(defaultControllerType);

            // creates the default route definition which maps to the 'UmbracoController' controller
            var def = new RouteDefinition
            {
                ControllerName = defaultControllerName,
                ControllerType = defaultControllerType,
                PublishedRequest = request,

                // ActionName = ((Route)requestContext.RouteData.Route).Defaults["action"].ToString(),
                ActionName = "Index",
                HasHijackedRoute = false
            };

            // check that a template is defined), if it doesn't and there is a hijacked route it will just route
            // to the index Action
            if (request.HasTemplate)
            {
                // the template Alias should always be already saved with a safe name.
                // if there are hyphens in the name and there is a hijacked route, then the Action will need to be attributed
                // with the action name attribute.
                var templateName = request.TemplateAlias.Split('.')[0].ToSafeAlias(_shortStringHelper);
                def.ActionName = templateName;
            }

            // check if there's a custom controller assigned, base on the document type alias.
            Type controllerType = FindControllerType(request.PublishedContent.ContentType.Alias);

            // check if that controller exists
            if (controllerType != null)
            {
                // ensure the controller is of type IRenderController and ControllerBase
                if (TypeHelper.IsTypeAssignableFrom<IRenderController>(controllerType)
                    && TypeHelper.IsTypeAssignableFrom<ControllerBase>(controllerType))
                {
                    // set the controller and name to the custom one
                    def.ControllerType = controllerType;
                    def.ControllerName = ControllerExtensions.GetControllerName(controllerType);
                    if (def.ControllerName != defaultControllerName)
                    {
                        def.HasHijackedRoute = true;
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "The current Document Type {ContentTypeAlias} matches a locally declared controller of type {ControllerName}. Custom Controllers for Umbraco routing must implement '{UmbracoRenderController}' and inherit from '{UmbracoControllerBase}'.",
                        request.PublishedContent.ContentType.Alias,
                        controllerType.FullName,
                        typeof(IRenderController).FullName,
                        typeof(ControllerBase).FullName);

                    // we cannot route to this custom controller since it is not of the correct type so we'll continue with the defaults
                    // that have already been set above.
                }
            }

            // store the route definition
            values.TryAdd(Constants.Web.UmbracoRouteDefinitionDataToken, def);

            return def;
        }

        private Type FindControllerType(string controllerName)
        {
            ControllerActionDescriptor descriptor = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                .Cast<ControllerActionDescriptor>()
                .First(x =>
                    x.ControllerName.Equals(controllerName));

            return descriptor?.ControllerTypeInfo;
        }

        private bool RouteRequest(IUmbracoContext umbracoContext)
        {
            // TODO: I suspect one day this will be async

            // ok, process

            // note: requestModule.UmbracoRewrite also did some stripping of &umbPage
            // from the querystring... that was in v3.x to fix some issues with pre-forms
            // auth. Paul Sterling confirmed in Jan. 2013 that we can get rid of it.

            // instantiate, prepare and process the published content request
            // important to use CleanedUmbracoUrl - lowercase path-only version of the current url
            IPublishedRequest request = _publishedRouter.CreateRequest(umbracoContext);
            umbracoContext.PublishedRequest = request; // TODO: This is ugly
            bool prepared = _publishedRouter.PrepareRequest(request);
            return prepared && request.HasPublishedContent;

            // // HandleHttpResponseStatus returns a value indicating that the request should
            // // not be processed any further, eg because it has been redirect. then, exit.
            // if (UmbracoModule.HandleHttpResponseStatus(httpContext, request, _logger))
            //    return;
            // if (!request.HasPublishedContent == false)
            // {
            //     // httpContext.RemapHandler(new PublishedContentNotFoundHandler());
            // }
            // else
            // {
            //     // RewriteToUmbracoHandler(httpContext, request);
            // }
        }
    }
}
