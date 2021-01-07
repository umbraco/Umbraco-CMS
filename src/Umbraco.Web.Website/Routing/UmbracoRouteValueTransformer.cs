using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.Web.Common.Routing;
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
        private readonly IPublishedRouter _publishedRouter;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtime;
        private readonly IUmbracoRouteValuesFactory _routeValuesFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRouteValueTransformer"/> class.
        /// </summary>
        public UmbracoRouteValueTransformer(
            ILogger<UmbracoRouteValueTransformer> logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            IPublishedRouter publishedRouter,
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtime,
            IUmbracoRouteValuesFactory routeValuesFactory)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _publishedRouter = publishedRouter;
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _runtime = runtime;
            _routeValuesFactory = routeValuesFactory;
        }

        /// <inheritdoc/>
        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            // If we aren't running, then we have nothing to route
            if (_runtime.Level != RuntimeLevel.Run)
            {
                return values;
            }

            // will be null for any client side requests like JS, etc...
            if (_umbracoContextAccessor.UmbracoContext == null)
            {
                return values;
            }

            // Check for back office request
            // TODO: This is how the module was doing it before but could just as easily be part of the RoutableDocumentFilter
            // which still needs to be migrated.
            if (httpContext.Request.IsDefaultBackOfficeRequest(_globalSettings, _hostingEnvironment))
            {
                return values;
            }

            // Check if there is no existing content and return the no content controller
            if (!_umbracoContextAccessor.UmbracoContext.Content.HasContent())
            {
                values["controller"] = ControllerExtensions.GetControllerName<RenderNoContentController>();
                values["action"] = nameof(RenderNoContentController.Index);

                return await Task.FromResult(values);
            }

            IPublishedRequest publishedRequest = await RouteRequestAsync(_umbracoContextAccessor.UmbracoContext);

            UmbracoRouteValues routeDef = _routeValuesFactory.Create(httpContext, values, publishedRequest);

            values["controller"] = routeDef.ControllerName;
            if (string.IsNullOrWhiteSpace(routeDef.ActionName) == false)
            {
                values["action"] = routeDef.ActionName;
            }

            return await Task.FromResult(values);
        }

        private async Task<IPublishedRequest> RouteRequestAsync(IUmbracoContext umbracoContext)
        {
            // ok, process

            // instantiate, prepare and process the published content request
            // important to use CleanedUmbracoUrl - lowercase path-only version of the current url
            IPublishedRequestBuilder requestBuilder = await _publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);

            // TODO: This is ugly with the re-assignment to umbraco context but at least its now
            // an immutable object. The only way to make this better would be to have a RouteRequest
            // as part of UmbracoContext but then it will require a PublishedRouter dependency so not sure that's worth it.
            // Maybe could be a one-time Set method instead?
            IPublishedRequest publishedRequest = umbracoContext.PublishedRequest = await _publishedRouter.RouteRequestAsync(requestBuilder);

            return publishedRequest;
        }
    }
}
