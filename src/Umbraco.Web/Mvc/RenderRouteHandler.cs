using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Strings;
using Umbraco.Web.Features;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using Umbraco.Core.Strings;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Mvc
{
    public class RenderRouteHandler : IRouteHandler
    {
        // Define reserved dictionary keys for controller, action and area specified in route additional values data
        internal static class ReservedAdditionalKeys
        {
            internal const string Controller = "c";
            internal const string Action = "a";
            internal const string Area = "ar";
        }

        private readonly IControllerFactory _controllerFactory;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IUmbracoContext _umbracoContext;

        public RenderRouteHandler(IUmbracoContextAccessor umbracoContextAccessor, IControllerFactory controllerFactory, IShortStringHelper shortStringHelper)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        }

        public RenderRouteHandler(IUmbracoContext umbracoContext, IControllerFactory controllerFactory, IShortStringHelper shortStringHelper)
        {
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        }

        private IUmbracoContext UmbracoContext => _umbracoContext ?? _umbracoContextAccessor.UmbracoContext;

        private UmbracoFeatures Features => Current.Factory.GetRequiredService<UmbracoFeatures>(); // TODO: inject

        #region IRouteHandler Members

        /// <summary>
        /// Assigns the correct controller based on the Umbraco request and returns a standard MvcHandler to process the response,
        /// this also stores the render model into the data tokens for the current RouteData.
        /// </summary>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            if (UmbracoContext == null)
            {
                throw new NullReferenceException("There is no current UmbracoContext, it must be initialized before the RenderRouteHandler executes");
            }
            var request = UmbracoContext.PublishedRequest;
            if (request == null)
            {
                throw new NullReferenceException("There is no current PublishedRequest, it must be initialized before the RenderRouteHandler executes");
            }

            return GetHandlerForRoute(requestContext, request);

        }

        #endregion

        private void UpdateRouteDataForRequest(ContentModel contentModel, RequestContext requestContext)
        {
            if (contentModel == null) throw new ArgumentNullException(nameof(contentModel));
            if (requestContext == null) throw new ArgumentNullException(nameof(requestContext));

            // requestContext.RouteData.DataTokens[Core.Constants.Web.UmbracoDataToken] = contentModel;
            // the rest should not change -- it's only the published content that has changed
        }

        /// <summary>
        /// Checks the request and query strings to see if it matches the definition of having a Surface controller
        /// posted/get value, if so, then we return a PostedDataProxyInfo object with the correct information.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        internal static PostedDataProxyInfo GetFormInfo(RequestContext requestContext)
        {
            if (requestContext == null) throw new ArgumentNullException(nameof(requestContext));

            //if it is a POST/GET then a value must be in the request
            if (requestContext.HttpContext.Request.QueryString["ufprt"].IsNullOrWhiteSpace()
                && requestContext.HttpContext.Request.Form["ufprt"].IsNullOrWhiteSpace())
            {
                return null;
            }

            string encodedVal;

            switch (requestContext.HttpContext.Request.RequestType)
            {
                case "POST":
                    //get the value from the request.
                    //this field will contain an encrypted version of the surface route vals.
                    encodedVal = requestContext.HttpContext.Request.Form["ufprt"];
                    break;
                case "GET":
                    //this field will contain an encrypted version of the surface route vals.
                    encodedVal = requestContext.HttpContext.Request.QueryString["ufprt"];
                    break;
                default:
                    return null;
            }

            if (!UmbracoHelper.DecryptAndValidateEncryptedRouteString(encodedVal, out var decodedParts))
                return null;

            foreach (var item in decodedParts.Where(x => new[] {
                ReservedAdditionalKeys.Controller,
                ReservedAdditionalKeys.Action,
                ReservedAdditionalKeys.Area }.Contains(x.Key) == false))
            {
                // Populate route with additional values which aren't reserved values so they eventually to action parameters
                requestContext.RouteData.Values[item.Key] = item.Value;
            }

            //return the proxy info without the surface id... could be a local controller.
            return new PostedDataProxyInfo
            {
                ControllerName = HttpUtility.UrlDecode(decodedParts.Single(x => x.Key == ReservedAdditionalKeys.Controller).Value),
                ActionName = HttpUtility.UrlDecode(decodedParts.Single(x => x.Key == ReservedAdditionalKeys.Action).Value),
                Area = HttpUtility.UrlDecode(decodedParts.Single(x => x.Key == ReservedAdditionalKeys.Area).Value),
            };
        }



        /// <summary>
        /// Handles a posted form to an Umbraco URL and ensures the correct controller is routed to and that
        /// the right DataTokens are set.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="postedInfo"></param>
        internal static IHttpHandler HandlePostedValues(RequestContext requestContext, PostedDataProxyInfo postedInfo)
        {
            if (requestContext == null) throw new ArgumentNullException(nameof(requestContext));
            if (postedInfo == null) throw new ArgumentNullException(nameof(postedInfo));

            //set the standard route values/tokens
            requestContext.RouteData.Values["controller"] = postedInfo.ControllerName;
            requestContext.RouteData.Values["action"] = postedInfo.ActionName;

            IHttpHandler handler;

            //get the route from the defined routes
            using (RouteTable.Routes.GetReadLock())
            {
                Route surfaceRoute;

                //find the controller in the route table
                var surfaceRoutes = RouteTable.Routes.OfType<Route>()
                                        .Where(x => x.Defaults != null &&
                                                    x.Defaults.ContainsKey("controller") &&
                                                    x.Defaults["controller"].ToString().InvariantEquals(postedInfo.ControllerName) &&
                                                    // Only return surface controllers
                                                    x.DataTokens["umbraco"].ToString().InvariantEquals("surface") &&
                                                    // Check for area token if the area is supplied
                                                    (postedInfo.Area.IsNullOrWhiteSpace() ? !x.DataTokens.ContainsKey("area") : x.DataTokens["area"].ToString().InvariantEquals(postedInfo.Area)))
                                        .ToList();

                // If more than one route is found, find one with a matching action
                if (surfaceRoutes.Count > 1)
                {
                    surfaceRoute = surfaceRoutes.FirstOrDefault(x =>
                        x.Defaults["action"] != null &&
                        x.Defaults["action"].ToString().InvariantEquals(postedInfo.ActionName));
                }
                else
                {
                    surfaceRoute = surfaceRoutes.FirstOrDefault();
                }

                if (surfaceRoute == null)
                    throw new InvalidOperationException("Could not find a Surface controller route in the RouteTable for controller name " + postedInfo.ControllerName);

                //set the area if one is there.
                if (surfaceRoute.DataTokens.ContainsKey("area"))
                {
                    requestContext.RouteData.DataTokens["area"] = surfaceRoute.DataTokens["area"];
                }

                //set the 'Namespaces' token so the controller factory knows where to look to construct it
                if (surfaceRoute.DataTokens.ContainsKey("Namespaces"))
                {
                    requestContext.RouteData.DataTokens["Namespaces"] = surfaceRoute.DataTokens["Namespaces"];
                }
                handler = surfaceRoute.RouteHandler.GetHttpHandler(requestContext);

            }

            return handler;
        }

        /// <summary>
        /// this will determine the controller and set the values in the route data
        /// </summary>
        internal IHttpHandler GetHandlerForRoute(RequestContext requestContext, IPublishedRequest request)
        {
            if (requestContext == null) throw new ArgumentNullException(nameof(requestContext));
            if (request == null) throw new ArgumentNullException(nameof(request));

            //var routeDef = GetUmbracoRouteDefinition(requestContext, request);

            // TODO: Need to port this to netcore
            // Need to check for a special case if there is form data being posted back to an Umbraco URL
            var postedInfo = GetFormInfo(requestContext);
            if (postedInfo != null)
            {
                return HandlePostedValues(requestContext, postedInfo);
            }

            // NOTE: Code here has been removed and ported to netcore
            throw new NotSupportedException("This code was already ported to netcore");
        }

    }
}
