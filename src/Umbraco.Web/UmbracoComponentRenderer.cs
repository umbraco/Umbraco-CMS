using System;
using System.Linq;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Web.Templates;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Macros;

namespace Umbraco.Web
{

    /// <summary>
    /// Methods used to render umbraco components as HTML in templates
    /// </summary>
    /// <remarks>
    /// Used by UmbracoHelper
    /// </remarks>
    internal class UmbracoComponentRenderer : IUmbracoComponentRenderer
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMacroRenderer _macroRenderer;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly HtmlLocalLinkParser _linkParser;

        public UmbracoComponentRenderer(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer, ITemplateRenderer templateRenderer, HtmlLocalLinkParser linkParser)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _macroRenderer = macroRenderer;
            _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
            _linkParser = linkParser;
        }

        /// <summary>
        /// Renders the template for the specified pageId and an optional altTemplateId
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
        /// <returns></returns>
        public IHtmlString RenderTemplate(int contentId, int? altTemplateId = null)
        {
            using (var sw = new StringWriter())
            {
                try
                {
                    _templateRenderer.Render(contentId, altTemplateId, sw);
                }
                catch (Exception ex)
                {
                    sw.Write("<!-- Error rendering template with id {0}: '{1}' -->", contentId, ex);
                }
                return new HtmlString(sw.ToString());
            }
        }

        /// <summary>
        /// Renders the macro with the specified alias.
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(int contentId, string alias)
        {
            return RenderMacro(contentId, alias, new { });
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(int contentId, string alias, object parameters)
        {
            return RenderMacro(contentId, alias, parameters?.ToDictionary<object>());
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(int contentId, string alias, IDictionary<string, object> parameters)
        {
            if (contentId == default)
                throw new ArgumentException("Invalid content id " + contentId);

            var content = _umbracoContextAccessor.UmbracoContext.Content?.GetById(contentId);

            if (content == null)
                throw new InvalidOperationException("Cannot render a macro, no content found by id " + contentId);

            return RenderMacro(content, alias, parameters);
        }


        public IHtmlString RenderMacroForContent(IPublishedContent content, string alias, IDictionary<string, object> parameters)
        {
            if(content == null)
                throw new InvalidOperationException("Cannot render a macro, IPublishedContent is null");

            return RenderMacro(content, alias, parameters);
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The macro alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="content">The content used for macro rendering</param>
        /// <returns></returns>
        private IHtmlString RenderMacro(IPublishedContent content, string alias, IDictionary<string, object> parameters)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            // TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method looks for a lower case match. the whole macro concept needs to be rewritten.
            //NOTE: the value could have HTML encoded values, so we need to deal with that
            var macroProps = parameters?.ToDictionary(
                x => x.Key.ToLowerInvariant(),
                i => (i.Value is string) ? HttpUtility.HtmlDecode(i.Value.ToString()) : i.Value);
            
            string html = _macroRenderer.Render(alias, content, macroProps).GetAsText();

            return new HtmlString(html);
        }
    }
}
