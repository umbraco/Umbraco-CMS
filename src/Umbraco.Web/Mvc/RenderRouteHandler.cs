using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Web.Mvc
{
	public class RenderRouteHandler : IRouteHandler
	{
		internal const string SingletonServiceName = "RenderRouteHandler";

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
			//need to ensure some http items are set!
			var path = requestContext.HttpContext.Request.QueryString["path"];
			var qry = requestContext.HttpContext.Request.QueryString["qry"];
			requestContext.HttpContext.Items["UmbPage"] = requestContext.HttpContext.Request.QueryString["path"];
			requestContext.HttpContext.Items["VirtualUrl"] = string.Format("{0}{1}", path, qry);

			//var handlerUrl = requestHandler.cleanUrl();
			//var v4Handler = new requestHandler(UmbracoContext.Current.GetXml(), handlerUrl);
			//var v4Page = new page(v4Handler.currentPage);

			////this is a fix for this issue:
			////https://bitbucket.org/Shandem/umbramvco/issue/1/need-to-set-pageid-in-conextitems
			////to support some of the @Library methods:
			//requestContext.HttpContext.Items["pageID"] = v4Page.PageID;

			////this is a fix for issue:
			//// https://bitbucket.org/Shandem/umbramvco/issue/2/current-culture
			//// make sure we have the correct culture
			//var tempCulture = v4Page.GetCulture();
			//if (tempCulture != "")
			//{
			//    Thread.CurrentThread.CurrentCulture =
			//        System.Globalization.CultureInfo.CreateSpecificCulture(tempCulture);
			//    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
			//}

			//var renderModel = new RenderModel()
			//{
			//    CurrentNode = new Node(v4Handler.currentPage),
			//    CurrentXmlNode = v4Handler.currentPage,
			//    UmbracoPage = v4Page
			//};

			////put the render model into the current RouteData
			//requestContext.RouteData.DataTokens.Add("umbraco", renderModel);

			//return GetHandlerForRoute(requestContext, renderModel);
			return null;
		}

		#endregion

		/// <summary>
		/// Returns a RouteDefinition object based on the current renderModel
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="renderModel"></param>
		/// <returns></returns>
		protected virtual RouteDefinition GetUmbracoRouteDefinition(RequestContext requestContext, RenderModel renderModel)
		{
			//creates the default route definition which maps to the 'UmbracoController' controller
			var def = new RouteDefinition
				{
					ControllerName = ControllerExtensions.GetControllerName<RenderMvcController>(),
					Controller = new RenderMvcController(),
					RenderModel = renderModel,
					ActionName = ((Route)requestContext.RouteData.Route).Defaults["action"].ToString()
				};

			var templateId = renderModel.UmbracoPage.Template;

			//check that a template is defined)
			if (templateId > 0)
			{

				//check if there's a custom controller assigned, base on the document type alias.
				var controller = _controllerFactory.CreateController(requestContext, renderModel.UmbracoPage.NodeTypeAlias);


				//check if that controller exists
				if (controller != null)
				{

					//ensure the controller is of type 'UmbracoController'
					if (controller is RenderMvcController)
					{
						//set the controller and name to the custom one
						def.Controller = (ControllerBase)controller;
						def.ControllerName = ControllerExtensions.GetControllerName(controller.GetType());
					}
					else
					{
						//LogHelper.Warn<RenderRouteHandler>("The current Document Type {0} matches a locally declared controller of type {1}. Custom Controllers for Umbraco routing must inherit from '{2}'.", renderModel.CurrentNode.ContentType.Alias, controller.GetType().FullName, typeof(UmbracoController).FullName);
						//exit as we cannnot route to the custom controller, just route to the standard one.
						return def;
					}


					var template = Template.GetTemplate(templateId);
					if (template != null)
					{
						//check if the custom controller has an action with the same name as the template name (we convert ToUmbracoAlias since the template name might have invalid chars).
						//NOTE: This also means that all custom actions MUST be PascalCase.. but that should be standard.
						var templateName = template.Alias.Split('.')[0].ToUmbracoAlias(StringAliasCaseType.PascalCase);
						def.ActionName = templateName;
					}
				}
			}

			return def;
		}

		/// <summary>
		/// this will determine the controller and set the values in the route data
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="renderModel"></param>
		protected internal IHttpHandler GetHandlerForRoute(RequestContext requestContext, RenderModel renderModel)
		{
			var routeDef = GetUmbracoRouteDefinition(requestContext, renderModel);

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