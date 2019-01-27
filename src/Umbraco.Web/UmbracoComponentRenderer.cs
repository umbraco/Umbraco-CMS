using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Web.Templates;
using umbraco;
using System.Collections.Generic;
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
        private readonly UmbracoContext _umbracoContext;

        public UmbracoComponentRenderer(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        /// <summary>
        /// Renders the template for the specified pageId and an optional altTemplateId
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
        /// <returns></returns>
        public IHtmlString RenderTemplate(int pageId, int? altTemplateId = null)
        {
            var templateRenderer = new TemplateRenderer(_umbracoContext, pageId, altTemplateId);
            using (var sw = new StringWriter())
            {
                try
                {
                    templateRenderer.Render(sw);
                }
                catch (Exception ex)
                {
                    sw.Write("<!-- Error rendering template with id {0}: '{1}' -->", pageId, ex);
                }
                return new HtmlString(sw.ToString());
            }
        }

        /// <summary>
        /// Renders the macro with the specified alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias)
        {
            return RenderMacro(alias, new { });
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias, object parameters)
        {
            return RenderMacro(alias, parameters.ToDictionary<object>());
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters)
        {

            if (_umbracoContext.PublishedRequest == null)
            {
                throw new InvalidOperationException("Cannot render a macro when there is no current PublishedContentRequest.");
            }

            return RenderMacro(alias, parameters, _umbracoContext.PublishedRequest.UmbracoPage);
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="umbracoPage">The legacy umbraco page object that is required for some macros</param>
        /// <returns></returns>
        internal IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters, page umbracoPage)
        {
            if (alias == null) throw new ArgumentNullException("alias");
            if (umbracoPage == null) throw new ArgumentNullException("umbracoPage");

            var m = Current.Services.MacroService.GetByAlias(alias);
            if (m == null)
                throw new KeyNotFoundException("Could not find macro with alias " + alias);

            return RenderMacro(new MacroModel(m), parameters, umbracoPage);
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="m">The macro.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="umbracoPage">The legacy umbraco page object that is required for some macros</param>
        /// <returns></returns>
        internal IHtmlString RenderMacro(MacroModel m, IDictionary<string, object> parameters, page umbracoPage)
        {
            if (umbracoPage == null) throw new ArgumentNullException(nameof(umbracoPage));
            if (m == null) throw new ArgumentNullException(nameof(m));

            if (_umbracoContext.PageId == null)
            {
                throw new InvalidOperationException("Cannot render a macro when UmbracoContext.PageId is null.");
            }

            var macroProps = new Hashtable();
            foreach (var i in parameters)
            {
                // TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method of macro.cs
                // looks for a lower case match. WTF. the whole macro concept needs to be rewritten.


                //NOTE: the value could have HTML encoded values, so we need to deal with that
                macroProps.Add(i.Key.ToLowerInvariant(), (i.Value is string) ? HttpUtility.HtmlDecode(i.Value.ToString()) : i.Value);
            }
            var renderer = new MacroRenderer(Current.ProfilingLogger);
            var macroControl = renderer.Render(m, umbracoPage.Elements, _umbracoContext.PageId.Value, macroProps).GetAsControl();

            string html;
            if (macroControl is LiteralControl)
            {
                // no need to execute, we already have text
                html = (macroControl as LiteralControl).Text;
            }
            else
            {
                var containerPage = new FormlessPage();
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

                    var contentType = _umbracoContext.HttpContext.Response.ContentType;
                    var traceIsEnabled = containerPage.Trace.IsEnabled;
                    containerPage.Trace.IsEnabled = false;
                    _umbracoContext.HttpContext.Server.Execute(containerPage, output, true);
                    containerPage.Trace.IsEnabled = traceIsEnabled;
                    //reset the content type
                    _umbracoContext.HttpContext.Response.ContentType = contentType;

                    //Now, we need to ensure that local links are parsed
                    html = TemplateUtilities.ParseInternalLinks(output.ToString(), _umbracoContext.UrlProvider);
                }
            }

            return new HtmlString(html);
        }
    }
}
