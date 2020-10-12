using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.SessionState;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// API controller to deal with Macro data
    /// </summary>
    /// <remarks>
    /// Note that this implements IRequiresSessionState which will enable HttpContext.Session - generally speaking we don't normally
    /// enable this for webapi controllers, however since this controller is used to render macro content and macros can access
    /// Session, we don't want it to throw null reference exceptions.
    /// </remarks>
    [PluginController("UmbracoApi")]
    public class MacroRenderingController : UmbracoAuthorizedJsonController, IRequiresSessionState
    {
        private readonly IMacroService _macroService;
        private readonly IContentService _contentService;
        private readonly IUmbracoComponentRenderer _componentRenderer;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public MacroRenderingController(IUmbracoComponentRenderer componentRenderer, IVariationContextAccessor variationContextAccessor, IMacroService macroService, IContentService contentService)
        {
            _componentRenderer = componentRenderer;
            _variationContextAccessor = variationContextAccessor;
            _macroService = macroService;
            _contentService = contentService;
        }

        /// <summary>
        /// Gets the macro parameters to be filled in for a particular macro
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Note that ALL logged in users have access to this method because editors will need to insert macros into rte (content/media/members) and it's used for
        /// inserting into templates/views/etc... it doesn't expose any sensitive data.
        /// </remarks>
        public IEnumerable<MacroParameter> GetMacroParameters(int macroId)
        {
            var macro = _macroService.GetById(macroId);
            if (macro == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IEnumerable<MacroParameter>>(macro).OrderBy(x => x.SortOrder);
        }

        /// <summary>
        /// Gets a rendered macro as HTML for rendering in the rich text editor
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
        /// Gets a rendered macro as HTML for rendering in the rich text editor.
        /// Using HTTP POST instead of GET allows for more parameters to be passed as it's not dependent on URL-length limitations like GET.
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
            var m = _macroService.GetByAlias(macroAlias);
            if (m == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var publishedContent = UmbracoContext.Content.GetById(true, pageId);

            //if it isn't supposed to be rendered in the editor then return an empty string
            //currently we cannot render a macro if the page doesn't yet exist
            if (pageId == -1 || publishedContent == null || m.DontRender)
            {
                var response = Request.CreateResponse();
                //need to create a specific content result formatted as HTML since this controller has been configured
                //with only json formatters.
                response.Content = new StringContent(string.Empty, Encoding.UTF8, "text/html");

                return response;
            }


            // When rendering the macro in the backoffice the default setting would be to use the Culture of the logged in user.
            // Since a Macro might contain thing thats related to the culture of the "IPublishedContent" (ie Dictionary keys) we want
            // to set the current culture to the culture related to the content item. This is hacky but it works.

            // fixme
            // in a 1:1 situation we do not handle the language being edited
            // so the macro renders in the wrong language

            var culture = publishedContent.GetCultureFromDomains();

            if (culture != null)
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

            // must have an active variation context!
            _variationContextAccessor.VariationContext = new VariationContext(culture);

            using (UmbracoContext.ForcedPreview(true))
            {

                var result = Request.CreateResponse();
                //need to create a specific content result formatted as HTML since this controller has been configured
                //with only json formatters.
                result.Content = new StringContent(
                    _componentRenderer.RenderMacroForContent(publishedContent, m.Alias, macroParams).ToString(),
                    Encoding.UTF8,
                    "text/html");

                return result;
            }
        }

        [HttpPost]
        public HttpResponseMessage CreatePartialViewMacroWithFile(CreatePartialViewMacroWithFileModel model)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (string.IsNullOrWhiteSpace(model.Filename)) throw new ArgumentException("Filename cannot be null or whitespace", "model.Filename");
            if (string.IsNullOrWhiteSpace(model.VirtualPath)) throw new ArgumentException("VirtualPath cannot be null or whitespace", "model.VirtualPath");

            var macroName = model.Filename.TrimEnd(".cshtml");

            var macro = new Macro
            {
                Alias = macroName.ToSafeAlias(),
                Name = macroName,
                MacroSource = model.VirtualPath.EnsureStartsWith("~"),
                MacroType = MacroTypes.PartialView
            };

            _macroService.Save(macro); // may throw
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public class CreatePartialViewMacroWithFileModel
        {
            public string Filename { get; set; }
            public string VirtualPath { get; set; }
        }
    }
}
