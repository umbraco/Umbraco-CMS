using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Logging;
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
				CurrentDocument = docRequest.Node
			};

			//put essential data into the data tokens, the 'umbraco' key is required to be there for the view engine
			requestContext.RouteData.DataTokens.Add("umbraco", renderModel); //required for the RenderModelBinder
			requestContext.RouteData.DataTokens.Add("umbraco-doc-request", docRequest); //required for RenderMvcController
			requestContext.RouteData.DataTokens.Add("umbraco-context", UmbracoContext.Current); //required for RenderViewPage

			return GetHandlerForRoute(requestContext, docRequest);
			
		}

		#endregion

		/// <summary>
		/// Returns a RouteDefinition object based on the current renderModel
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="documentRequest"></param>
		/// <returns></returns>
		internal virtual RouteDefinition GetUmbracoRouteDefinition(RequestContext requestContext, DocumentRequest documentRequest)
		{
			//creates the default route definition which maps to the 'UmbracoController' controller
			var def = new RouteDefinition
				{
					ControllerName = ControllerExtensions.GetControllerName<RenderMvcController>(),
					Controller = new RenderMvcController(),
					DocumentRequest = documentRequest,
					ActionName = ((Route)requestContext.RouteData.Route).Defaults["action"].ToString()
				};

			//check that a template is defined)
			if (documentRequest.HasTemplate)
			{

				//check if there's a custom controller assigned, base on the document type alias.
				var controller = _controllerFactory.CreateController(requestContext, documentRequest.Node.DocumentTypeAlias);


				//check if that controller exists
				if (controller != null)
				{

					//ensure the controller is of type 'RenderMvcController'
					if (controller is RenderMvcController)
					{
						//set the controller and name to the custom one
						def.Controller = (ControllerBase)controller;
						def.ControllerName = ControllerExtensions.GetControllerName(controller.GetType());
					}
					else
					{
						LogHelper.Warn<RenderRouteHandler>("The current Document Type {0} matches a locally declared controller of type {1}. Custom Controllers for Umbraco routing must inherit from '{2}'.", documentRequest.Node.DocumentTypeAlias, controller.GetType().FullName, typeof(RenderMvcController).FullName);
						//exit as we cannnot route to the custom controller, just route to the standard one.
						return def;
					}

					//check if the custom controller has an action with the same name as the template name (we convert ToUmbracoAlias since the template name might have invalid chars).
					//NOTE: This also means that all custom actions MUST be PascalCase.. but that should be standard.
					var templateName = documentRequest.TemplateLookup.TemplateAlias.Split('.')[0].ToUmbracoAlias(StringAliasCaseType.PascalCase);
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