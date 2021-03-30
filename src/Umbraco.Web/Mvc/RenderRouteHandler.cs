using System;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using System.Collections.Generic;
using Current = Umbraco.Web.Composing.Current;
using Umbraco.Web.Features;

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
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly UmbracoContext _umbracoContext;

        public RenderRouteHandler(IUmbracoContextAccessor umbracoContextAccessor, IControllerFactory controllerFactory)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        public RenderRouteHandler(UmbracoContext umbracoContext, IControllerFactory controllerFactory)
        {
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        }

        private UmbracoContext UmbracoContext => _umbracoContext ?? _umbracoContextAccessor.UmbracoContext;

        private UmbracoFeatures Features => Current.Factory.GetInstance<UmbracoFeatures>(); // TODO: inject

        #region IRouteHandler Members

        /// <summary>
        /// Assigns the correct controller based on the Umbraco request and returns a standard MvcHandler to process the response,
        /// this also stores the render model into the data tokens for the current RouteData.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
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

            SetupRouteDataForRequest(
                new ContentModel(request.PublishedContent),
                requestContext,
                request);

            return GetHandlerForRoute(requestContext, request);

        }

        #endregion

        /// <summary>
        /// Ensures that all of the correct DataTokens are added to the route values which are all required for rendering front-end umbraco views
        /// </summary>
        /// <param name="contentModel"></param>
        /// <param name="requestContext"></param>
        /// <param name="frequest"></param>
        internal void SetupRouteDataForRequest(ContentModel contentModel, RequestContext requestContext, PublishedRequest frequest)
        {
            //put essential data into the data tokens, the 'umbraco' key is required to be there for the view engine
            requestContext.RouteData.DataTokens.Add(Core.Constants.Web.UmbracoDataToken, contentModel); //required for the ContentModelBinder and view engine
            requestContext.RouteData.DataTokens.Add(Core.Constants.Web.PublishedDocumentRequestDataToken, frequest); //required for RenderMvcController
            requestContext.RouteData.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, UmbracoContext); //required for UmbracoViewPage
        }

        private void UpdateRouteDataForRequest(ContentModel contentModel, RequestContext requestContext)
        {
            if (contentModel == null) throw new ArgumentNullException(nameof(contentModel));
            if (requestContext == null) throw new ArgumentNullException(nameof(requestContext));

            requestContext.RouteData.DataTokens[Core.Constants.Web.UmbracoDataToken] = contentModel;
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
        /// Returns a RouteDefinition object based on the current content request
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        internal virtual RouteDefinition GetUmbracoRouteDefinition(RequestContext requestContext, PublishedRequest request)
        {
            if (requestContext == null) throw new ArgumentNullException(nameof(requestContext));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var defaultControllerType = Current.DefaultRenderMvcControllerType;
            var defaultControllerName = ControllerExtensions.GetControllerName(defaultControllerType);
            //creates the default route definition which maps to the 'UmbracoController' controller
            var def = new RouteDefinition
                {
                    ControllerName = defaultControllerName,
                    ControllerType = defaultControllerType,
                    PublishedRequest = request,
                    ActionName = ((Route)requestContext.RouteData.Route).Defaults["action"].ToString(),
                    HasHijackedRoute = false
                };

            //check that a template is defined), if it doesn't and there is a hijacked route it will just route
            // to the index Action
            if (request.HasTemplate)
            {
                //the template Alias should always be already saved with a safe name.
                //if there are hyphens in the name and there is a hijacked route, then the Action will need to be attributed
                // with the action name attribute.
                var templateName = request.TemplateAlias.Split(Umbraco.Core.Constants.CharArrays.Period)[0].ToSafeAlias();
                def.ActionName = templateName;
            }

            //check if there's a custom controller assigned, base on the document type alias.
            var controllerType = _controllerFactory.GetControllerTypeInternal(requestContext, request.PublishedContent.ContentType.Alias);

            //check if that controller exists
            if (controllerType != null)
            {
                //ensure the controller is of type IRenderMvcController and ControllerBase
                if (TypeHelper.IsTypeAssignableFrom<IRenderController>(controllerType)
                    && TypeHelper.IsTypeAssignableFrom<ControllerBase>(controllerType))
                {
                    //set the controller and name to the custom one
                    def.ControllerType = controllerType;
                    def.ControllerName = ControllerExtensions.GetControllerName(controllerType);
                    if (def.ControllerName != defaultControllerName)
                    {
                        def.HasHijackedRoute = true;
                    }
                }
                else
                {
                    Current.Logger.Warn<RenderRouteHandler>("The current Document Type {ContentTypeAlias} matches a locally declared controller of type {ControllerName}. Custom Controllers for Umbraco routing must implement '{UmbracoRenderController}' and inherit from '{UmbracoControllerBase}'.",
                        request.PublishedContent.ContentType.Alias,
                        controllerType.FullName,
                        typeof(IRenderController).FullName,
                        typeof(ControllerBase).FullName);

                    //we cannot route to this custom controller since it is not of the correct type so we'll continue with the defaults
                    // that have already been set above.
                }
            }

            //store the route definition
            requestContext.RouteData.DataTokens[Core.Constants.Web.UmbracoRouteDefinitionDataToken] = def;

            return def;
        }

        internal IHttpHandler GetHandlerOnMissingTemplate(PublishedRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // missing template, so we're in a 404 here
            // so the content, if any, is a custom 404 page of some sort

            if (request.HasPublishedContent == false)
                // means the builder could not find a proper document to handle 404
                return new PublishedContentNotFoundHandler();

            if (request.HasTemplate == false)
                // means the engine could find a proper document, but the document has no template
                // at that point there isn't much we can do and there is no point returning
                // to Mvc since Mvc can't do much
                return new PublishedContentNotFoundHandler("In addition, no template exists to render the custom 404.");

            return null;
        }

        /// <summary>
        /// this will determine the controller and set the values in the route data
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="request"></param>
        internal IHttpHandler GetHandlerForRoute(RequestContext requestContext, PublishedRequest request)
        {
            if (requestContext == null) throw new ArgumentNullException(nameof(requestContext));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var routeDef = GetUmbracoRouteDefinition(requestContext, request);

            //Need to check for a special case if there is form data being posted back to an Umbraco URL
            var postedInfo = GetFormInfo(requestContext);
            if (postedInfo != null)
            {
                return HandlePostedValues(requestContext, postedInfo);
            }


            //Here we need to check if there is no hijacked route and no template assigned,
            //if this is the case we want to return a blank page, but we'll leave that up to the NoTemplateHandler.
            //We also check if templates have been disabled since if they are then we're allowed to render even though there's no template,
            //for example for json rendering in headless.
            if ((request.HasTemplate == false && Features.Disabled.DisableTemplates == false)
                && routeDef.HasHijackedRoute == false)
            {
                request.UpdateToNotFound(); // request will go 404

                // HandleHttpResponseStatus returns a value indicating that the request should
                // not be processed any further, eg because it has been redirect. then, exit.
                if (UmbracoModule.HandleHttpResponseStatus(requestContext.HttpContext, request, Current.Logger))
                    return null;

                var handler = GetHandlerOnMissingTemplate(request);

                // if it's not null it's the PublishedContentNotFoundHandler (no document was found to handle 404, or document with no template was found)
                // if it's null it means that a document was found

                // if we have a handler, return now
                if (handler != null)
                    return handler;

                // else we are running Mvc
                // update the route data - because the PublishedContent has changed
                UpdateRouteDataForRequest(
                    new ContentModel(request.PublishedContent),
                    requestContext);
                // update the route definition
                routeDef = GetUmbracoRouteDefinition(requestContext, request);
            }

            //no post values, just route to the controller/action required (local)

            requestContext.RouteData.Values["controller"] = routeDef.ControllerName;
            if (string.IsNullOrWhiteSpace(routeDef.ActionName) == false)
                requestContext.RouteData.Values["action"] = routeDef.ActionName;

            // Set the session state requirements
            requestContext.HttpContext.SetSessionStateBehavior(GetSessionStateBehavior(requestContext, routeDef.ControllerName));

            // reset the friendly path so in the controllers and anything occurring after this point in time,
            //the URL is reset back to the original request.
            requestContext.HttpContext.RewritePath(UmbracoContext.OriginalRequestUrl.PathAndQuery);

            return new UmbracoMvcHandler(requestContext);
        }

        private SessionStateBehavior GetSessionStateBehavior(RequestContext requestContext, string controllerName)
        {
            return _controllerFactory.GetControllerSessionBehavior(requestContext, controllerName);
        }
    }
}
