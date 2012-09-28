using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Web.Mvc
{
	public class RenderRouteHandler : IRouteHandler
	{
		
		public RenderRouteHandler(IControllerFactory controllerFactory)
		{
			_controllerFactory = controllerFactory;
		}

		private readonly IControllerFactory _controllerFactory;

		#region IRouteHandler Members

		/// <summary>
		/// Assigns the correct controller based on the Umbraco request and returns a standard MvcHandler to prcess the response,
		/// this also stores the render model into the data tokens for the current RouteData.
		/// </summary>
		/// <param name="requestContext"></param>
		/// <returns></returns>
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{			
			if (UmbracoContext.Current == null)
			{
				throw new NullReferenceException("There is not current UmbracoContext, it must be initialized before the RenderRouteHandler executes");
			}
			var docRequest = UmbracoContext.Current.DocumentRequest;
			if (docRequest == null)
			{
				throw new NullReferenceException("There is not current DocumentRequest, it must be initialized before the RenderRouteHandler executes");
			}

			var renderModel = new RenderModel()
			{
				CurrentDocument = docRequest.Document
			};

			//put essential data into the data tokens, the 'umbraco' key is required to be there for the view engine
			requestContext.RouteData.DataTokens.Add("umbraco", renderModel); //required for the RenderModelBinder
			requestContext.RouteData.DataTokens.Add("umbraco-doc-request", docRequest); //required for RenderMvcController
			requestContext.RouteData.DataTokens.Add("umbraco-context", UmbracoContext.Current); //required for RenderViewPage

			return GetHandlerForRoute(requestContext, docRequest);
			
		}

		#endregion

		/// <summary>
		/// Checks the request and query strings to see if it matches the definition of having a Surface controller
		/// posted value, if so, then we return a PostedDataProxyInfo object with the correct information.
		/// </summary>
		/// <param name="requestContext"></param>
		/// <returns></returns>
		private static PostedDataProxyInfo GetPostedFormInfo(RequestContext requestContext)
		{
			if (requestContext.HttpContext.Request.RequestType != "POST")
				return null;

			//this field will contain a base64 encoded version of the surface route vals 
			if (requestContext.HttpContext.Request["uformpostroutevals"].IsNullOrWhiteSpace())
				return null;

			var encodedVal = requestContext.HttpContext.Request["uformpostroutevals"];
			var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedVal));
			//the value is formatted as query strings
			var decodedParts = decodedString.Split('&').Select(x => new { Key = x.Split('=')[0], Value = x.Split('=')[1] }).ToArray();

			//validate all required keys exist

			//the controller
			if (!decodedParts.Any(x => x.Key == "c"))
				return null;
			//the action
			if (!decodedParts.Any(x => x.Key == "a"))
				return null;
			//the area
			if (!decodedParts.Any(x => x.Key == "ar"))
				return null;

			////the controller type, if it contains this then it is a plugin controller, not locally declared.
			//if (decodedParts.Any(x => x.Key == "t"))
			//{
			//    return new PostedDataProxyInfo
			//    {
			//        ControllerName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "c").Value),
			//        ActionName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "a").Value),
			//        Area = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "ar").Value),
			//        ControllerType = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "t").Value)
			//    };				
			//}

			//return the proxy info without the surface id... could be a local controller.
			return new PostedDataProxyInfo
			{
				ControllerName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "c").Value),
				ActionName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "a").Value),
				Area = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "ar").Value),
			};

		}

		/// <summary>
		/// Handles a posted form to an Umbraco Url and ensures the correct controller is routed to and that
		/// the right DataTokens are set.
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="postedInfo"></param>
		/// <param name="routeDefinition">The original route definition that would normally be used to route if it were not a POST</param>
		private IHttpHandler HandlePostedValues(RequestContext requestContext, PostedDataProxyInfo postedInfo, RouteDefinition routeDefinition)
		{
			var standardArea = Umbraco.Core.Configuration.GlobalSettings.UmbracoMvcArea;

			//set the standard route values/tokens
			requestContext.RouteData.Values["controller"] = postedInfo.ControllerName;
			requestContext.RouteData.Values["action"] = postedInfo.ActionName;
			requestContext.RouteData.DataTokens["area"] = postedInfo.Area;

			IHttpHandler handler = new MvcHandler(requestContext);

			//ensure the controllerType is set if found, meaning it is a plugin, not locally declared
			if (postedInfo.Area != standardArea)
			{
				//requestContext.RouteData.Values["controllerType"] = postedInfo.ControllerType;
				//find the other data tokens for this route and merge... things like Namespace will be included here
				using (RouteTable.Routes.GetReadLock())
				{
					var surfaceRoute = RouteTable.Routes.OfType<Route>()
						.SingleOrDefault(x => x.Defaults != null &&
						                      x.Defaults.ContainsKey("controller") &&
						                      x.Defaults["controller"].ToString() == postedInfo.ControllerName &&
						                      x.DataTokens.ContainsKey("area") &&
						                      x.DataTokens["area"].ToString() == postedInfo.Area);
					if (surfaceRoute == null)
						throw new InvalidOperationException("Could not find a Surface controller route in the RouteTable for controller name " + postedInfo.ControllerName + " and within the area of " + postedInfo.Area);
					//set the 'Namespaces' token so the controller factory knows where to look to construct it
					if (surfaceRoute.DataTokens.ContainsKey("Namespaces"))
					{
						requestContext.RouteData.DataTokens["Namespaces"] = surfaceRoute.DataTokens["Namespaces"];
					}
					handler = surfaceRoute.RouteHandler.GetHttpHandler(requestContext);
				}

			}

			//store the original URL this came in on
			requestContext.RouteData.DataTokens["umbraco-item-url"] = requestContext.HttpContext.Request.Url.AbsolutePath;
			//store the original route definition
			requestContext.RouteData.DataTokens["umbraco-route-def"] = routeDefinition;

			return handler;
		}

		/// <summary>
		/// Returns a RouteDefinition object based on the current renderModel
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="documentRequest"></param>
		/// <returns></returns>
		internal virtual RouteDefinition GetUmbracoRouteDefinition(RequestContext requestContext, DocumentRequest documentRequest)
		{
			var defaultControllerName = ControllerExtensions.GetControllerName<RenderMvcController>();
			//creates the default route definition which maps to the 'UmbracoController' controller
			var def = new RouteDefinition
				{
					ControllerName = defaultControllerName,
					Controller = new RenderMvcController(),
					DocumentRequest = documentRequest,
					ActionName = ((Route)requestContext.RouteData.Route).Defaults["action"].ToString(),
					HasHijackedRoute = false
				};

			//check if there's a custom controller assigned, base on the document type alias.
			var controller = _controllerFactory.CreateController(requestContext, documentRequest.Document.DocumentTypeAlias);


			//check if that controller exists
			if (controller != null)
			{

				//ensure the controller is of type 'RenderMvcController'
				if (controller is RenderMvcController)
				{
					//set the controller and name to the custom one
					def.Controller = (ControllerBase)controller;
					def.ControllerName = ControllerExtensions.GetControllerName(controller.GetType());
					if (def.ControllerName != defaultControllerName)
					{
						def.HasHijackedRoute = true;	
					}
				}
				else
				{
					LogHelper.Warn<RenderRouteHandler>("The current Document Type {0} matches a locally declared controller of type {1}. Custom Controllers for Umbraco routing must inherit from '{2}'.", documentRequest.Document.DocumentTypeAlias, controller.GetType().FullName, typeof(RenderMvcController).FullName);
					//exit as we cannnot route to the custom controller, just route to the standard one.
					return def;
				}

				//check that a template is defined), if it doesn't and there is a hijacked route it will just route
				// to the index Action
				if (documentRequest.HasTemplate)
				{
					//check if the custom controller has an action with the same name as the template name (we convert ToUmbracoAlias since the template name might have invalid chars).
					//NOTE: This also means that all custom actions MUST be PascalCase.. but that should be standard.
					var templateName = documentRequest.Template.Alias.Split('.')[0].ToUmbracoAlias(StringAliasCaseType.PascalCase);
					def.ActionName = templateName;
				}
	
			}
			

			return def;
		}

		/// <summary>
		/// this will determine the controller and set the values in the route data
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="documentRequest"></param>
		internal IHttpHandler GetHandlerForRoute(RequestContext requestContext, DocumentRequest documentRequest)
		{
			var routeDef = GetUmbracoRouteDefinition(requestContext, documentRequest);

			//Need to check for a special case if there is form data being posted back to an Umbraco URL
			var postedInfo = GetPostedFormInfo(requestContext);
			if (postedInfo != null)
			{
				return HandlePostedValues(requestContext, postedInfo, routeDef);
			}

			//here we need to check if there is no hijacked route and no template assigned, if this is the case
			//we want to return a blank page, but we'll leave that up to the NoTemplateHandler.
			if (!documentRequest.HasTemplate && !routeDef.HasHijackedRoute)
			{
				return new NoTemplateHandler();
			}

			//no post values, just route to the controller/action requried (local)

			requestContext.RouteData.Values["controller"] = routeDef.ControllerName;
			if (!string.IsNullOrWhiteSpace(routeDef.ActionName))
			{
				requestContext.RouteData.Values["action"] = routeDef.ActionName;
			}
			return new MvcHandler(requestContext);
		}
	}
}