using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Web.Macros;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Templates
{
    /// <summary>
    /// This is used purely for the RenderTemplate functionality in Umbraco
    /// </summary>
    /// <remarks>
    /// This allows you to render an MVC template based purely off of a node id and an optional alttemplate id as string output.
    /// </remarks>
    internal class TemplateRenderer : ITemplateRenderer
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedRouter _publishedRouter;
        private readonly IFileService _fileService;
        private readonly ILocalizationService _languageService;
        private readonly IWebRoutingSection _webRoutingSection;

        public TemplateRenderer(IUmbracoContextAccessor umbracoContextAccessor, IPublishedRouter publishedRouter, IFileService fileService, ILocalizationService textService, IWebRoutingSection webRoutingSection)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedRouter = publishedRouter ?? throw new ArgumentNullException(nameof(publishedRouter));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _languageService = textService ?? throw new ArgumentNullException(nameof(textService));
            _webRoutingSection = webRoutingSection ?? throw new ArgumentNullException(nameof(webRoutingSection));
        }
        
        public void Render(int pageId, int? altTemplateId, StringWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            // instantiate a request and process
            // important to use CleanedUmbracoUrl - lowercase path-only version of the current URL, though this isn't going to matter
            // terribly much for this implementation since we are just creating a doc content request to modify it's properties manually.
            var contentRequest = _publishedRouter.CreateRequest(_umbracoContextAccessor.UmbracoContext);

            var doc = contentRequest.UmbracoContext.Content.GetById(pageId);

            if (doc == null)
            {
                writer.Write("<!-- Could not render template for Id {0}, the document was not found -->", pageId);
                return;
            }

            //in some cases the UmbracoContext will not have a PublishedRequest assigned to it if we are not in the
            //execution of a front-end rendered page. In this case set the culture to the default.
            //set the culture to the same as is currently rendering
            if (_umbracoContextAccessor.UmbracoContext.PublishedRequest == null)
            {
                var defaultLanguage = _languageService.GetAllLanguages().FirstOrDefault();
                contentRequest.Culture = defaultLanguage == null
                    ? CultureInfo.CurrentUICulture
                    : defaultLanguage.CultureInfo;
            }
            else
            {
                contentRequest.Culture = _umbracoContextAccessor.UmbracoContext.PublishedRequest.Culture;
            }

            //set the doc that was found by id
            contentRequest.PublishedContent = doc;
            //set the template, either based on the AltTemplate found or the standard template of the doc
            var templateId = _webRoutingSection.DisableAlternativeTemplates || !altTemplateId.HasValue
                ? doc.TemplateId
                : altTemplateId.Value;
            if (templateId.HasValue)
                contentRequest.TemplateModel = _fileService.GetTemplate(templateId.Value);

            //if there is not template then exit
            if (contentRequest.HasTemplate == false)
            {
                if (altTemplateId.HasValue == false)
                {
                    writer.Write("<!-- Could not render template for Id {0}, the document's template was not found with id {0}-->", doc.TemplateId);
                }
                else
                {
                    writer.Write("<!-- Could not render template for Id {0}, the altTemplate was not found with id {0}-->", altTemplateId);
                }
                return;
            }

            //First, save all of the items locally that we know are used in the chain of execution, we'll need to restore these
            //after this page has rendered.
            SaveExistingItems(out var oldPublishedRequest, out var oldAltTemplate);

            try
            {
                //set the new items on context objects for this templates execution
                SetNewItemsOnContextObjects(contentRequest);

                //Render the template
                ExecuteTemplateRendering(writer, contentRequest);
            }
            finally
            {
                //restore items on context objects to continuing rendering the parent template
                RestoreItems(oldPublishedRequest, oldAltTemplate);
            }

        }

        private void ExecuteTemplateRendering(TextWriter sw, PublishedRequest request)
        {
            //NOTE: Before we used to build up the query strings here but this is not necessary because when we do a
            // Server.Execute in the TemplateRenderer, we pass in a 'true' to 'preserveForm' which automatically preserves all current
            // query strings so there's no need for this. HOWEVER, once we get MVC involved, we might have to do some fun things,
            // though this will happen in the TemplateRenderer.

            //var queryString = _umbracoContext.HttpContext.Request.QueryString.AllKeys
            //    .ToDictionary(key => key, key => context.Request.QueryString[key]);

            var requestContext = new RequestContext(_umbracoContextAccessor.UmbracoContext.HttpContext, new RouteData()
            {
                Route = RouteTable.Routes["Umbraco_default"]
            });
            var routeHandler = new RenderRouteHandler(_umbracoContextAccessor, ControllerBuilder.Current.GetControllerFactory());
            var routeDef = routeHandler.GetUmbracoRouteDefinition(requestContext, request);
            var renderModel = new ContentModel(request.PublishedContent);
            //manually add the action/controller, this is required by mvc
            requestContext.RouteData.Values.Add("action", routeDef.ActionName);
            requestContext.RouteData.Values.Add("controller", routeDef.ControllerName);
            //add the rest of the required route data
            routeHandler.SetupRouteDataForRequest(renderModel, requestContext, request);

            var stringOutput = RenderUmbracoRequestToString(requestContext);

            sw.Write(stringOutput);
        }

        /// <summary>
        /// This will execute the UmbracoMvcHandler for the request specified and get the string output.
        /// </summary>
        /// <param name="requestContext">
        /// Assumes the RequestContext is setup specifically to render an Umbraco view.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// To achieve this we temporarily change the output text writer of the current HttpResponse, then
        ///   execute the controller via the handler which inevitably writes the result to the text writer
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

        private void SetNewItemsOnContextObjects(PublishedRequest request)
        {
            //now, set the new ones for this page execution
            _umbracoContextAccessor.UmbracoContext.HttpContext.Items[Core.Constants.Conventions.Url.AltTemplate] = null;
            _umbracoContextAccessor.UmbracoContext.PublishedRequest = request;
        }

        /// <summary>
        /// Save all items that we know are used for rendering execution to variables so we can restore after rendering
        /// </summary>
        private void SaveExistingItems(out PublishedRequest oldPublishedRequest, out object oldAltTemplate)
        {
            //Many objects require that these legacy items are in the http context items... before we render this template we need to first
            //save the values in them so that we can re-set them after we render so the rest of the execution works as per normal
            oldPublishedRequest = _umbracoContextAccessor.UmbracoContext.PublishedRequest;
            oldAltTemplate = _umbracoContextAccessor.UmbracoContext.HttpContext.Items[Core.Constants.Conventions.Url.AltTemplate];
        }

        /// <summary>
        /// Restores all items back to their context's to continue normal page rendering execution
        /// </summary>
        private void RestoreItems(PublishedRequest oldPublishedRequest, object oldAltTemplate)
        {
            _umbracoContextAccessor.UmbracoContext.PublishedRequest = oldPublishedRequest;
            _umbracoContextAccessor.UmbracoContext.HttpContext.Items[Core.Constants.Conventions.Url.AltTemplate] = oldAltTemplate;
        }
    }
}
