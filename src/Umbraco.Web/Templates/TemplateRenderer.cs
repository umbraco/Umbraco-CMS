using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.cms.businesslogic.language;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Templates
{
	/// <summary>
	/// This is used purely for the RenderTemplate functionality in Umbraco
	/// </summary>
	/// <remarks>
	/// This allows you to render either an MVC or Webforms template based purely off of a node id and an optional alttemplate id as string output.	
	/// </remarks>
	internal class TemplateRenderer
	{
		private readonly UmbracoContext _umbracoContext;
		private object _oldPageId;
		private object _oldPageElements;
		private PublishedContentRequest _oldPublishedContentRequest;
		private object _oldAltTemplate;

		public TemplateRenderer(UmbracoContext umbracoContext, int pageId, int? altTemplateId)
		{
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			PageId = pageId;
			AltTemplate = altTemplateId;
			_umbracoContext = umbracoContext;
		}

		/// <summary>
		/// Gets/sets the page id for the template to render
		/// </summary>
		public int PageId { get; private set; }

		/// <summary>
		/// Gets/sets the alt template to render if there is one
		/// </summary>
		public int? AltTemplate { get; private set; }

		public void Render(StringWriter writer)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			// instanciate a request a process
			// important to use CleanedUmbracoUrl - lowercase path-only version of the current url, though this isn't going to matter
			// terribly much for this implementation since we are just creating a doc content request to modify it's properties manually.
			var contentRequest = new PublishedContentRequest(_umbracoContext.CleanedUmbracoUrl, _umbracoContext.RoutingContext);

            var doc = contentRequest.RoutingContext.UmbracoContext.ContentCache.GetById(PageId);

			if (doc == null)
			{
				writer.Write("<!-- Could not render template for Id {0}, the document was not found -->", PageId);
				return;
			}

            //in some cases the UmbracoContext will not have a PublishedContentRequest assigned to it if we are not in the
            //execution of a front-end rendered page. In this case set the culture to the default.
            //set the culture to the same as is currently rendering
            if (_umbracoContext.PublishedContentRequest == null)
            {
                var defaultLanguage = Language.GetAllAsList().FirstOrDefault();
                contentRequest.Culture = defaultLanguage == null 
                    ? CultureInfo.CurrentUICulture 
                    : new CultureInfo(defaultLanguage.CultureAlias);
            }
            else
            {
                contentRequest.Culture = _umbracoContext.PublishedContentRequest.Culture;    
            }
			
			//set the doc that was found by id
			contentRequest.PublishedContent = doc;
			//set the template, either based on the AltTemplate found or the standard template of the doc
			contentRequest.TemplateModel = UmbracoConfig.For.UmbracoSettings().WebRouting.DisableAlternativeTemplates || AltTemplate.HasValue == false 
				? ApplicationContext.Current.Services.FileService.GetTemplate(doc.TemplateId)
				: ApplicationContext.Current.Services.FileService.GetTemplate(AltTemplate.Value);

			//if there is not template then exit
			if (!contentRequest.HasTemplate)
			{
				if (!AltTemplate.HasValue)
				{
					writer.Write("<!-- Could not render template for Id {0}, the document's template was not found with id {0}-->", doc.TemplateId);	
				}				
				else
				{
					writer.Write("<!-- Could not render template for Id {0}, the altTemplate was not found with id {0}-->", AltTemplate);	
				}
				return;
			}

			//First, save all of the items locally that we know are used in the chain of execution, we'll need to restore these
			//after this page has rendered.
			SaveExistingItems();

			//set the new items on context objects for this templates execution
			SetNewItemsOnContextObjects(contentRequest);

			//Render the template
			ExecuteTemplateRendering(writer, contentRequest);

			//restore items on context objects to continuing rendering the parent template
			RestoreItems();
		}

		private void ExecuteTemplateRendering(TextWriter sw, PublishedContentRequest contentRequest)
		{
			//NOTE: Before we used to build up the query strings here but this is not necessary because when we do a 
			// Server.Execute in the TemplateRenderer, we pass in a 'true' to 'preserveForm' which automatically preserves all current
			// query strings so there's no need for this. HOWEVER, once we get MVC involved, we might have to do some fun things,
			// though this will happen in the TemplateRenderer.

			//var queryString = _umbracoContext.HttpContext.Request.QueryString.AllKeys
			//	.ToDictionary(key => key, key => context.Request.QueryString[key]);
			
			switch (contentRequest.RenderingEngine)
			{
				case RenderingEngine.Mvc:
					var requestContext = new RequestContext(_umbracoContext.HttpContext, new RouteData()
					{
						Route = RouteTable.Routes["Umbraco_default"]
					});
					var routeHandler = new RenderRouteHandler(ControllerBuilder.Current.GetControllerFactory(), _umbracoContext);
					var routeDef = routeHandler.GetUmbracoRouteDefinition(requestContext, contentRequest);
					var renderModel = new RenderModel(contentRequest.PublishedContent, contentRequest.Culture);
					//manually add the action/controller, this is required by mvc
					requestContext.RouteData.Values.Add("action", routeDef.ActionName);
					requestContext.RouteData.Values.Add("controller", routeDef.ControllerName);
					//add the rest of the required route data
					routeHandler.SetupRouteDataForRequest(renderModel, requestContext, contentRequest);

                    var stringOutput = RenderUmbracoRequestToString(requestContext);
                    
					sw.Write(stringOutput);
					break;
				case RenderingEngine.WebForms:
				default:
					var webFormshandler = (global::umbraco.UmbracoDefault)BuildManager
						.CreateInstanceFromVirtualPath("~/default.aspx", typeof(global::umbraco.UmbracoDefault));
					//the 'true' parameter will ensure that the current query strings are carried through, we don't have
					// to build up the url again, it will just work.
					_umbracoContext.HttpContext.Server.Execute(webFormshandler, sw, true);
					break;
			}	
			
		}

        /// <summary>
        /// This will execute the UmbracoMvcHandler for the request specified and get the string output.
        /// </summary>
        /// <param name="requestContext">
        /// Assumes the RequestContext is setup specifically to render an Umbraco view.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// To acheive this we temporarily change the output text writer of the current HttpResponse, then
        ///   execute the controller via the handler which innevitably writes the result to the text writer
        ///   that has been assigned to the response. Then we change the response textwriter back to the original
        ///   before continuing .
        /// </remarks>
        private string RenderUmbracoRequestToString(RequestContext requestContext)
        {
            var currentWriter = requestContext.HttpContext.Response.Output;
            var newWriter = new StringWriter();
            requestContext.HttpContext.Response.Output = newWriter;

            var handler = new UmbracoMvcHandler(requestContext);
            handler.ExecuteUmbracoRequest();

            //reset it
            requestContext.HttpContext.Response.Output = currentWriter;
            return newWriter.ToString();
        }

		private void SetNewItemsOnContextObjects(PublishedContentRequest contentRequest)
		{
			// handlers like default.aspx will want it and most macros currently need it
			contentRequest.UmbracoPage = new page(contentRequest);
			//now, set the new ones for this page execution
			_umbracoContext.HttpContext.Items["pageID"] = contentRequest.PublishedContent.Id;
			_umbracoContext.HttpContext.Items["pageElements"] = contentRequest.UmbracoPage.Elements;
			_umbracoContext.HttpContext.Items[Umbraco.Core.Constants.Conventions.Url.AltTemplate] = null;
			_umbracoContext.PublishedContentRequest = contentRequest;
		}

		/// <summary>
		/// Save all items that we know are used for rendering execution to variables so we can restore after rendering
		/// </summary>
		private void SaveExistingItems()
		{
			//Many objects require that these legacy items are in the http context items... before we render this template we need to first
			//save the values in them so that we can re-set them after we render so the rest of the execution works as per normal.
			_oldPageId = _umbracoContext.HttpContext.Items["pageID"];
			_oldPageElements = _umbracoContext.HttpContext.Items["pageElements"];
			_oldPublishedContentRequest = _umbracoContext.PublishedContentRequest;
			_oldAltTemplate = _umbracoContext.HttpContext.Items[Umbraco.Core.Constants.Conventions.Url.AltTemplate];
		}

		/// <summary>
		/// Restores all items back to their context's to continue normal page rendering execution
		/// </summary>
		private void RestoreItems()
		{
			_umbracoContext.PublishedContentRequest = _oldPublishedContentRequest;
			_umbracoContext.HttpContext.Items["pageID"] = _oldPageId;
			_umbracoContext.HttpContext.Items["pageElements"] = _oldPageElements;
			_umbracoContext.HttpContext.Items[Umbraco.Core.Constants.Conventions.Url.AltTemplate] = _oldAltTemplate;
		}

	}
}