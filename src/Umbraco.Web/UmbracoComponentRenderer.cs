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

        public UmbracoComponentRenderer(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer, ITemplateRenderer templateRenderer)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _macroRenderer = macroRenderer;
            _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
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
            return RenderMacro(contentId, alias, parameters.ToDictionary<object>());
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

            var content = _umbracoContextAccessor.UmbracoContext.ContentCache?.GetById(true, contentId);

            if (content == null)
                throw new InvalidOperationException("Cannot render a macro, no content found by id " + contentId);

            return RenderMacro(alias, parameters, content);
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The macro alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="content">The content used for macro rendering</param>
        /// <returns></returns>
        private IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters, IPublishedContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            // TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method looks for a lower case match. the whole macro concept needs to be rewritten.
            //NOTE: the value could have HTML encoded values, so we need to deal with that
            var macroProps = parameters.ToDictionary(
                x => x.Key.ToLowerInvariant(),
                i => (i.Value is string) ? HttpUtility.HtmlDecode(i.Value.ToString()) : i.Value);
            
            var macroControl = _macroRenderer.Render(alias, content, macroProps).GetAsControl();

            string html;
            if (macroControl is LiteralControl control)
            {
                // no need to execute, we already have text
                html = control.Text;
            }
            else
            {
                using (var containerPage = new FormlessPage())
                {
                    containerPage.Controls.Add(macroControl);

                    using (var output = new StringWriter())
                    {
                        // .Execute() does a PushTraceContext/PopTraceContext and writes trace output straight into 'output'
                        // and I do not see how we could wire the trace context to the current context... so it creates dirty
                        // trace output right in the middle of the page.
                        //
                        // The only thing we can do is fully disable trace output while .Execute() runs and restore afterwards
                        // which means trace output is lost if the macro is a control (.ascx or user control) that is invoked
                        // from within Razor -- which makes sense anyway because the control can _not_ run correctly from
                        // within Razor since it will never be inserted into the page pipeline (which may even not exist at all
                        // if we're running MVC).
                        //
                        // I'm sure there's more things that will get lost with this context changing but I guess we'll figure
                        // those out as we go along. One thing we lose is the content type response output.
                        // http://issues.umbraco.org/issue/U4-1599 if it is setup during the macro execution. So
                        // here we'll save the content type response and reset it after execute is called.

                        var contentType = _umbracoContextAccessor.UmbracoContext.HttpContext.Response.ContentType;
                        var traceIsEnabled = containerPage.Trace.IsEnabled;
                        containerPage.Trace.IsEnabled = false;
                        _umbracoContextAccessor.UmbracoContext.HttpContext.Server.Execute(containerPage, output, true);
                        containerPage.Trace.IsEnabled = traceIsEnabled;
                        //reset the content type
                        _umbracoContextAccessor.UmbracoContext.HttpContext.Response.ContentType = contentType;

                        //Now, we need to ensure that local links are parsed
                        html = TemplateUtilities.ParseInternalLinks(output.ToString(), _umbracoContextAccessor.UmbracoContext.UrlProvider);
                    }
                }
                    
            }

            return new HtmlString(html);
        }
    }
}
