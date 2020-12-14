using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Middleware;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// The route value transformer for Umbraco front-end routes
    /// </summary>
    /// <remarks>
    /// NOTE: In aspnet 5 DynamicRouteValueTransformer has been improved, see https://github.com/dotnet/aspnetcore/issues/21471
    /// It seems as though with the "State" parameter we could more easily assign the IPublishedRequest or IPublishedContent
    /// or UmbracoContext more easily that way. In the meantime we will rely on assigning the IPublishedRequest to the
    /// route values along with the IPublishedContent to the umbraco context
    /// have created a GH discussion here https://github.com/dotnet/aspnetcore/discussions/28562 we'll see if anyone responds
    /// </remarks>
    public class UmbracoRouteValueTransformer : DynamicRouteValueTransformer
    {
        private readonly ILogger<UmbracoRouteValueTransformer> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IUmbracoRenderingDefaults _renderingDefaults;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly IPublishedRouter _publishedRouter;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRouteValueTransformer"/> class.
        /// </summary>
        public UmbracoRouteValueTransformer(
            ILogger<UmbracoRouteValueTransformer> logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoRenderingDefaults renderingDefaults,
            IShortStringHelper shortStringHelper,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IPublishedRouter publishedRouter,
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _renderingDefaults = renderingDefaults;
            _shortStringHelper = shortStringHelper;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _publishedRouter = publishedRouter;
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <inheritdoc/>
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            // will be null for any client side requests like JS, etc...
            if (_umbracoContextAccessor.UmbracoContext == null)
            {
                return values;
                // throw new InvalidOperationException($"There is no current UmbracoContext, it must be initialized before the {nameof(UmbracoRouteValueTransformer)} executes, ensure that {nameof(UmbracoRequestMiddleware)} is registered prior to 'UseRouting'");
            }

            // Check for back office request
            // TODO: This is how the module was doing it before but could just as easily be part of the RoutableDocumentFilter
            // which still needs to be migrated.
            if (httpContext.Request.IsDefaultBackOfficeRequest(_globalSettings, _hostingEnvironment))
            {
                return values;
            }

            bool routed = RouteRequest(_umbracoContextAccessor.UmbracoContext, out IPublishedRequest publishedRequest);
            if (!routed)
            {
                return values;
                // TODO: Deal with it not being routable, perhaps this should be an enum result?
            }

            UmbracoRouteValues routeDef = GetUmbracoRouteDefinition(httpContext, values, publishedRequest);
            values["controller"] = routeDef.ControllerName;
            if (string.IsNullOrWhiteSpace(routeDef.ActionName) == false)
            {
                values["action"] = routeDef.ActionName;
            }

            return await Task.FromResult(values);
        }

        /// <summary>
        /// Returns a <see cref="UmbracoRouteValues"/> object based on the current content request
        /// </summary>
        private UmbracoRouteValues GetUmbracoRouteDefinition(HttpContext httpContext, RouteValueDictionary values, IPublishedRequest request)
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

            string customActionName = null;
            var customControllerName = request.PublishedContent.ContentType.Alias; // never null

            // check that a template is defined), if it doesn't and there is a hijacked route it will just route
            // to the index Action
            if (request.HasTemplate)
            {
                // the template Alias should always be already saved with a safe name.
                // if there are hyphens in the name and there is a hijacked route, then the Action will need to be attributed
                // with the action name attribute.
                customActionName = request.TemplateAlias.Split('.')[0].ToSafeAlias(_shortStringHelper);
            }

            // creates the default route definition which maps to the 'UmbracoController' controller
            var def = new UmbracoRouteValues(
                request.PublishedContent,
                defaultControllerName,
                defaultControllerType,
                templateName: customActionName);

            IReadOnlyList<ControllerActionDescriptor> candidates = FindControllerCandidates(customControllerName, customActionName, def.ActionName);

            // check if there's a custom controller assigned, base on the document type alias.
            var customControllerCandidates = candidates.Where(x => x.ControllerName.InvariantEquals(customControllerName)).ToList();

            // check if that custom controller exists
            if (customControllerCandidates.Count > 0)
            {
                ControllerActionDescriptor controllerDescriptor = customControllerCandidates[0];

                // ensure the controller is of type IRenderController and ControllerBase
                if (TypeHelper.IsTypeAssignableFrom<IRenderController>(controllerDescriptor.ControllerTypeInfo)
                    && TypeHelper.IsTypeAssignableFrom<ControllerBase>(controllerDescriptor.ControllerTypeInfo))
                {
                    // now check if the custom action matches
                    var customActionExists = customActionName != null && customControllerCandidates.Any(x => x.ActionName.InvariantEquals(customActionName));

                    def = new UmbracoRouteValues(
                        request.PublishedContent,
                        controllerDescriptor.ControllerName,
                        controllerDescriptor.ControllerTypeInfo,
                        customActionExists ? customActionName : def.ActionName,
                        customActionName,
                        true); // Hijacked = true
                }
                else
                {
                    _logger.LogWarning(
                        "The current Document Type {ContentTypeAlias} matches a locally declared controller of type {ControllerName}. Custom Controllers for Umbraco routing must implement '{UmbracoRenderController}' and inherit from '{UmbracoControllerBase}'.",
                        request.PublishedContent.ContentType.Alias,
                        controllerDescriptor.ControllerTypeInfo.FullName,
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

        /// <summary>
        /// Return a list of controller candidates that match the custom controller and action names
        /// </summary>
        private IReadOnlyList<ControllerActionDescriptor> FindControllerCandidates(string customControllerName, string customActionName, string defaultActionName)
        {
            var descriptors = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                .Cast<ControllerActionDescriptor>()
                .Where(x => x.ControllerName.InvariantEquals(customControllerName)
                        && (x.ActionName.InvariantEquals(defaultActionName) || (customActionName != null && x.ActionName.InvariantEquals(customActionName))))
                .ToList();

            return descriptors;
        }

        private bool RouteRequest(IUmbracoContext umbracoContext, out IPublishedRequest publishedRequest)
        {
            // TODO: I suspect one day this will be async

            // ok, process

            // note: requestModule.UmbracoRewrite also did some stripping of &umbPage
            // from the querystring... that was in v3.x to fix some issues with pre-forms
            // auth. Paul Sterling confirmed in Jan. 2013 that we can get rid of it.

            // instantiate, prepare and process the published content request
            // important to use CleanedUmbracoUrl - lowercase path-only version of the current url
            IPublishedRequest request = _publishedRouter.CreateRequest(umbracoContext);

            // TODO: This is ugly with the re-assignment to umbraco context also because IPublishedRequest is mutable
            publishedRequest = umbracoContext.PublishedRequest = request;
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
