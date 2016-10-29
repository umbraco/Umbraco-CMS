using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using umbraco;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// API controller to deal with Macro data
    /// </summary>
    [PluginController("UmbracoApi")]
    public class MacroController : UmbracoAuthorizedJsonController
    {
        

        /// <summary>
        /// Gets the macro parameters to be filled in for a particular macro
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Note that ALL logged in users have access to this method because editors will need to isnert macros into rte (content/media/members) and it's used for 
        /// inserting into templates/views/etc... it doesn't expose any sensitive data.
        /// </remarks>
        public IEnumerable<MacroParameter> GetMacroParameters(int macroId)
        {
            var macro = Services.MacroService.GetById(macroId);
            if (macro == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IEnumerable<MacroParameter>>(macro).OrderBy(x => x.SortOrder);
        }

        /// <summary>
        /// Gets a rendered macro as html for rendering in the rich text editor
        /// </summary>
        /// <param name="macroAlias"></param>
        /// <param name="pageId"></param>
        /// <param name="macroParams">
        /// To send a dictionary as a GET parameter the query should be structured like:
        /// 
        /// ?macroAlias=Test&pageId=3634&macroParams[0].key=myKey&macroParams[0].value=myVal&macroParams[1].key=anotherKey&macroParams[1].value=anotherVal
        /// 
        /// </param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetMacroResultAsHtmlForEditor(string macroAlias, int pageId, [FromUri] IDictionary<string, object> macroParams)
        {
            return GetMacroResultAsHtml(macroAlias, pageId, macroParams);
        }

        /// <summary>
        /// Gets a rendered macro as html for rendering in the rich text editor.
        /// Using HTTP POST instead of GET allows for more parameters to be passed as it's not dependant on URL-length limitations like GET.
        /// The method using GET is kept to maintain backwards compatibility
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetMacroResultAsHtmlForEditor(MacroParameterModel model)
        {
            return GetMacroResultAsHtml(model.MacroAlias, model.PageId, model.MacroParams);
        }

        public class MacroParameterModel
        {
            public string MacroAlias { get; set; }
            public int PageId { get; set; }
            public IDictionary<string, object> MacroParams { get; set; }
        }

        private HttpResponseMessage GetMacroResultAsHtml(string macroAlias, int pageId, IDictionary<string, object> macroParams)
        {
            // note - here we should be using the cache, provided that the preview content is in the cache...

            var doc = Services.ContentService.GetById(pageId);
            if (doc == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //need to get a legacy macro object - eventually we'll have a new format but nto yet
            var macro = new macro(macroAlias);
            if (macro == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //if it isn't supposed to be rendered in the editor then return an empty string
            if (macro.DontRenderInEditor)
            {
                var response = Request.CreateResponse();
                //need to create a specific content result formatted as html since this controller has been configured
                //with only json formatters.
                response.Content = new StringContent(string.Empty, Encoding.UTF8, "text/html");

                return response;
            }

            //because macro's are filled with insane legacy bits and pieces we need all sorts of wierdness to make them render.
            //the 'easiest' way might be to create an IPublishedContent manually and populate the legacy 'page' object with that
            //and then set the legacy parameters.

            var legacyPage = new global::umbraco.page(doc);
            UmbracoContext.HttpContext.Items["pageID"] = doc.Id;
            UmbracoContext.HttpContext.Items["pageElements"] = legacyPage.Elements;
            UmbracoContext.HttpContext.Items[global::Umbraco.Core.Constants.Conventions.Url.AltTemplate] = null;

            var renderer = new UmbracoComponentRenderer(UmbracoContext);

            var result = Request.CreateResponse();
            //need to create a specific content result formatted as html since this controller has been configured
            //with only json formatters.
            result.Content = new StringContent(
                renderer.RenderMacro(macro, macroParams, legacyPage).ToString(),
                Encoding.UTF8,
                "text/html");
            return result;
        }

    }
}