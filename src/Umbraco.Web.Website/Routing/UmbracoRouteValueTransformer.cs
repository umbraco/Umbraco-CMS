using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.Controllers;
using RouteDirection = Umbraco.Web.Routing.RouteDirection;

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
        private readonly IRoutableDocumentFilter _routableDocumentFilter;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IControllerActionSearcher _controllerActionSearcher;

        internal const string ControllerToken = "controller";
        internal const string ActionToken = "action";

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
            IUmbracoRouteValuesFactory routeValuesFactory,
            IRoutableDocumentFilter routableDocumentFilter,
            IDataProtectionProvider dataProtectionProvider,
            IControllerActionSearcher controllerActionSearcher)
        {
            if (globalSettings is null)
            {
                throw new ArgumentNullException(nameof(globalSettings));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedRouter = publishedRouter ?? throw new ArgumentNullException(nameof(publishedRouter));
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _routeValuesFactory = routeValuesFactory ?? throw new ArgumentNullException(nameof(routeValuesFactory));
            _routableDocumentFilter = routableDocumentFilter ?? throw new ArgumentNullException(nameof(routableDocumentFilter));
            _dataProtectionProvider = dataProtectionProvider;
            _controllerActionSearcher = controllerActionSearcher;
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

            if (!_routableDocumentFilter.IsDocumentRequest(httpContext.Request.Path))
            {
                return values;
            }

            // Check if there is no existing content and return the no content controller
            if (!_umbracoContextAccessor.UmbracoContext.Content.HasContent())
            {
                values[ControllerToken] = ControllerExtensions.GetControllerName<RenderNoContentController>();
                values[ActionToken] = nameof(RenderNoContentController.Index);

                return values;
            }

            IPublishedRequest publishedRequest = await RouteRequestAsync(_umbracoContextAccessor.UmbracoContext);

            UmbracoRouteValues umbracoRouteValues = _routeValuesFactory.Create(httpContext, publishedRequest);

            // Store the route values as a httpcontext feature
            httpContext.Features.Set(umbracoRouteValues);

            // Need to check if there is form data being posted back to an Umbraco URL
            PostedDataProxyInfo postedInfo = GetFormInfo(httpContext, values);
            if (postedInfo != null)
            {
                return HandlePostedValues(postedInfo, httpContext, values);
            }

            values[ControllerToken] = umbracoRouteValues.ControllerName;
            if (string.IsNullOrWhiteSpace(umbracoRouteValues.ActionName) == false)
            {
                values[ActionToken] = umbracoRouteValues.ActionName;
            }

            return values;
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
            IPublishedRequest routedRequest = await _publishedRouter.RouteRequestAsync(requestBuilder, new RouteRequestOptions(RouteDirection.Inbound));
            umbracoContext.PublishedRequest = routedRequest;

            return routedRequest;
        }

        /// <summary>
        /// Checks the request and query strings to see if it matches the definition of having a Surface controller
        /// posted/get value, if so, then we return a PostedDataProxyInfo object with the correct information.
        /// </summary>
        private PostedDataProxyInfo GetFormInfo(HttpContext httpContext, RouteValueDictionary values)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // if it is a POST/GET then a value must be in the request
            if (!httpContext.Request.Query.TryGetValue("ufprt", out StringValues encodedVal)
                && (!httpContext.Request.HasFormContentType || !httpContext.Request.Form.TryGetValue("ufprt", out encodedVal)))
            {
                return null;
            }

            if (!EncryptionHelper.DecryptAndValidateEncryptedRouteString(
                _dataProtectionProvider,
                encodedVal,
                out IDictionary<string, string> decodedParts))
            {
                return null;
            }

            // Get all route values that are not the default ones and add them separately so they eventually get to action parameters
            foreach (KeyValuePair<string, string> item in decodedParts.Where(x => ReservedAdditionalKeys.AllKeys.Contains(x.Key) == false))
            {
                values[item.Key] = item.Value;
            }

            // return the proxy info without the surface id... could be a local controller.
            return new PostedDataProxyInfo
            {
                ControllerName = WebUtility.UrlDecode(decodedParts.First(x => x.Key == ReservedAdditionalKeys.Controller).Value),
                ActionName = WebUtility.UrlDecode(decodedParts.First(x => x.Key == ReservedAdditionalKeys.Action).Value),
                Area = WebUtility.UrlDecode(decodedParts.First(x => x.Key == ReservedAdditionalKeys.Area).Value),
            };
        }

        private RouteValueDictionary HandlePostedValues(PostedDataProxyInfo postedInfo, HttpContext httpContext, RouteValueDictionary values)
        {
            // set the standard route values/tokens
            values[ControllerToken] = postedInfo.ControllerName;
            values[ActionToken] = postedInfo.ActionName;

            ControllerActionDescriptor surfaceControllerDescriptor = _controllerActionSearcher.Find<SurfaceController>(httpContext, postedInfo.ControllerName, postedInfo.ActionName);

            if (surfaceControllerDescriptor == null)
            {
                throw new InvalidOperationException("Could not find a Surface controller route in the RouteTable for controller name " + postedInfo.ControllerName);
            }

            // set the area if one is there.
            if (!postedInfo.Area.IsNullOrWhiteSpace())
            {
                values["area"] = postedInfo.Area;
            }

            return values;
        }

        private class PostedDataProxyInfo
        {
            public string ControllerName { get; set; }

            public string ActionName { get; set; }

            public string Area { get; set; }
        }

        // Define reserved dictionary keys for controller, action and area specified in route additional values data
        private static class ReservedAdditionalKeys
        {
            internal static readonly string[] AllKeys = new[]
            {
                Controller,
                Action,
                Area
            };

            internal const string Controller = "c";
            internal const string Action = "a";
            internal const string Area = "ar";
        }
    }
}
