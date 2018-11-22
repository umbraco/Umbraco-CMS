using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Templates;
using umbraco;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.templateControls;
using Umbraco.Core.Cache;

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

            if (_umbracoContext.PublishedContentRequest == null)
            {
                throw new InvalidOperationException("Cannot render a macro when there is no current PublishedContentRequest.");
            }

            return RenderMacro(alias, parameters, _umbracoContext.PublishedContentRequest.UmbracoPage);
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

            var m = macro.GetMacro(alias);
            if (m == null)
            {
                throw new KeyNotFoundException("Could not find macro with alias " + alias);
            }

            return RenderMacro(m, parameters, umbracoPage);
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="m">The macro.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="umbracoPage">The legacy umbraco page object that is required for some macros</param>
        /// <returns></returns>
        internal IHtmlString RenderMacro(macro m, IDictionary<string, object> parameters, page umbracoPage)
        {
            if (umbracoPage == null) throw new ArgumentNullException("umbracoPage");
            if (m == null) throw new ArgumentNullException("m");

            if (_umbracoContext.PageId == null)
            {
                throw new InvalidOperationException("Cannot render a macro when UmbracoContext.PageId is null.");
            }

            var macroProps = new Hashtable();
            foreach (var i in parameters)
            {
                //TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method of macro.cs 
                // looks for a lower case match. WTF. the whole macro concept needs to be rewritten.


                //NOTE: the value could have html encoded values, so we need to deal with that
                macroProps.Add(i.Key.ToLowerInvariant(), (i.Value is string) ? HttpUtility.HtmlDecode(i.Value.ToString()) : i.Value);
            }
            var macroControl = m.renderMacro(macroProps,
                umbracoPage.Elements,
                _umbracoContext.PageId.Value);

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
                    html = TemplateUtilities.ParseInternalLinks(output.ToString());
                }
            }

            return new HtmlString(html);
        }

        /// <summary>
        /// Renders an field to the template
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="fieldAlias"></param>
        /// <param name="altFieldAlias"></param>
        /// <param name="altText"></param>
        /// <param name="insertBefore"></param>
        /// <param name="insertAfter"></param>
        /// <param name="recursive"></param>
        /// <param name="convertLineBreaks"></param>
        /// <param name="removeParagraphTags"></param>
        /// <param name="casing"></param>
        /// <param name="encoding"></param>
        /// <param name="formatAsDate"></param>
        /// <param name="formatAsDateWithTime"></param>
        /// <param name="formatAsDateWithTimeSeparator"></param>
        //// <param name="formatString"></param>
        /// <returns></returns>
        public IHtmlString Field(IPublishedContent currentPage, string fieldAlias,
            string altFieldAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            RenderFieldCaseType casing = RenderFieldCaseType.Unchanged,
            RenderFieldEncodingType encoding = RenderFieldEncodingType.Unchanged,
            bool formatAsDate = false,
            bool formatAsDateWithTime = false,
            string formatAsDateWithTimeSeparator = "")

            //TODO: commented out until as it is not implemented by umbraco:item yet
        //,string formatString = "")
        {
            Mandate.ParameterNotNull(currentPage, "currentPage");
            Mandate.ParameterNotNullOrEmpty(fieldAlias, "fieldAlias");

            //TODO: This is real nasty and we should re-write the 'item' and 'ItemRenderer' class but si fine for now

            var attributes = new Dictionary<string, string>
				{
					{"field", fieldAlias},
					{"recursive", recursive.ToString().ToLowerInvariant()},
					{"useifempty", altFieldAlias},
					{"textifempty", altText},
					{"stripparagraph", removeParagraphTags.ToString().ToLowerInvariant()},
					{
						"case", casing == RenderFieldCaseType.Lower ? "lower"
						        	: casing == RenderFieldCaseType.Upper ? "upper"
						        	  	: casing == RenderFieldCaseType.Title ? "title"
						        	  	  	: string.Empty
						},
					{"inserttextbefore", insertBefore},
					{"inserttextafter", insertAfter},
					{"convertlinebreaks", convertLineBreaks.ToString().ToLowerInvariant()},
                    {"formatasdate", formatAsDate.ToString().ToLowerInvariant()},
                    {"formatasdatewithtime", formatAsDateWithTime.ToString().ToLowerInvariant()},
                    {"formatasdatewithtimeseparator", formatAsDateWithTimeSeparator}
				};
            switch (encoding)
            {
                case RenderFieldEncodingType.Url:
                    attributes.Add("urlencode", "true");
                    break;
                case RenderFieldEncodingType.Html:
                    attributes.Add("htmlencode", "true");
                    break;
                case RenderFieldEncodingType.Unchanged:
                default:
                    break;
            }

            //need to convert our dictionary over to this weird dictionary type
            var attributesForItem = new AttributeCollectionAdapter(
                new AttributeCollection(
                    new StateBag()));
            foreach (var i in attributes)
            {
                attributesForItem.Add(i.Key, i.Value);
            }



            var item = new Item(currentPage)
            {
                Field = fieldAlias,
                TextIfEmpty = altText,
                LegacyAttributes = attributesForItem
            };

            //here we are going to check if we are in the context of an Umbraco routed page, if we are we 
            //will leave the NodeId empty since the underlying ItemRenderer will work ever so slightly faster
            //since it already knows about the current page. Otherwise, we'll assign the id based on our
            //currently assigned node. The PublishedContentRequest will be null if:
            // * we are rendering a partial view or child action
            // * we are rendering a view from a custom route
            if ((_umbracoContext.PublishedContentRequest == null
                || _umbracoContext.PublishedContentRequest.PublishedContent.Id != currentPage.Id)
                && currentPage.Id > 0) // in case we're rendering a detached content (id == 0)
            {
                item.NodeId = currentPage.Id.ToString(CultureInfo.InvariantCulture);
            }


            var containerPage = new FormlessPage();
            containerPage.Controls.Add(item);

            using (var output = new StringWriter())
            using (var htmlWriter = new HtmlTextWriter(output))
            {
                ItemRenderer.Instance.Init(item);
                ItemRenderer.Instance.Load(item);
                ItemRenderer.Instance.Render(item, htmlWriter);

                //because we are rendering the output through the legacy Item (webforms) stuff, the {localLinks} will already be replaced.
                return new HtmlString(output.ToString());
            }
        }
    }
}