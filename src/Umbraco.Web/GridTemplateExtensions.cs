using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.GridFrameworks;

namespace Umbraco.Web
{

    public static class GridTemplateExtensions
    {
        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedProperty property, string framework = "bootstrap3")
        {
            var asString = property.Value as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            var view = "Grid/" + framework;
            return html.Partial(view, property.Value);
        }

        /// <summary>
        /// Gets the grid HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="contentItem">The content item.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException">No property type found with alias " + propertyAlias</exception>
        public static IHtmlString GetGridHtml<TGridFramework>(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias, bool recursive = false)
            where TGridFramework : IGridFramework, new()
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");
            if (contentItem.HasProperty(propertyAlias) == false)
            {
                throw new NullReferenceException("No property found with alias " + propertyAlias);
            }
            var gridJson = contentItem.GetPropertyValue<string>(propertyAlias, recursive);
            if (gridJson == null)
            {
                throw new NullReferenceException("The property with alias " + propertyAlias + ", was null");
            }
            return html.GetGridHtml<TGridFramework>(JsonConvert.DeserializeObject<GridValue>(gridJson));
        }

        /// <summary>
        /// Gets the grid HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="gridToken">The grid as a JToken.</param>
        /// <returns></returns>
        public static IHtmlString GetGridHtml<TGridFramework>(this HtmlHelper html, JToken gridToken)
            where TGridFramework : IGridFramework, new()
        {
            return html.GetGridHtml<TGridFramework>(gridToken.ToObject<GridValue>());
        }

        /// <summary>
        /// Renders the grid.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="grid">The grid.</param>
        /// <returns></returns>
        public static IHtmlString GetGridHtml<TGridFramework>(this HtmlHelper html, GridValue grid)
            where TGridFramework : IGridFramework, new()
        {
            var framework = new TGridFramework { GridValue = grid };

            var gridDiv = GridHelper.GetDivWrapper(framework.GridCssClass);

            if (grid.Sections.Count() == 1)
            {
                gridDiv.AddChild(framework.GetSectionHtml(html, 0).ToHtmlString());
            }
            else if (grid.Sections.Count() > 1)
            {
                gridDiv.AddChild(framework.GetSectionsHtml(html).ToHtmlString());
            }

            return gridDiv.ToHtml();
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem)
        {
            return html.GetGridHtml(contentItem, "bodyText", "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            return html.GetGridHtml(contentItem, propertyAlias, "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias, string framework)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            var view = "Grid/" + framework;
            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new NullReferenceException("No property type found with alias " + propertyAlias);
            var model = prop.Value;

            var asString = model as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            return html.Partial(view, model);
        }

        public static MvcHtmlString GetGridHtml(this IPublishedProperty property, HtmlHelper html, string framework = "bootstrap3")
        {
            var asString = property.Value as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            var view = "Grid/" + framework;
            return html.Partial(view, property.Value);
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html)
        {
            return GetGridHtml(contentItem, html, "bodyText", "bootstrap3");
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            return GetGridHtml(contentItem, html, propertyAlias, "bootstrap3");
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias, string framework)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            var view = "Grid/" + framework;
            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new NullReferenceException("No property type found with alias " + propertyAlias);
            var model = prop.Value;

            var asString = model as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            return html.Partial(view, model);
        }


        [Obsolete("This should not be used, GetGridHtml methods accepting HtmlHelper as a parameter or GetGridHtml extensions on HtmlHelper should be used instead")]
        public static MvcHtmlString GetGridHtml(this IPublishedProperty property, string framework = "bootstrap3")
        {
            var asString = property.Value as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            var htmlHelper = CreateHtmlHelper(property.Value);
            return htmlHelper.GetGridHtml(property, framework);
        }

        [Obsolete("This should not be used, GetGridHtml methods accepting HtmlHelper as a parameter or GetGridHtml extensions on HtmlHelper should be used instead")]
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem)
        {
            return GetGridHtml(contentItem, "bodyText", "bootstrap3");
        }

        [Obsolete("This should not be used, GetGridHtml methods accepting HtmlHelper as a parameter or GetGridHtml extensions on HtmlHelper should be used instead")]
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, string propertyAlias)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            return GetGridHtml(contentItem, propertyAlias, "bootstrap3");
        }

        [Obsolete("This should not be used, GetGridHtml methods accepting HtmlHelper as a parameter or GetGridHtml extensions on HtmlHelper should be used instead")]
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, string propertyAlias, string framework)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new NullReferenceException("No property type found with alias " + propertyAlias);
            var model = prop.Value;

            var asString = model as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            var htmlHelper = CreateHtmlHelper(model);
            return htmlHelper.GetGridHtml(contentItem, propertyAlias, framework);
        }

        [Obsolete("This shouldn't need to be used but because the obsolete extension methods above don't have access to the current HtmlHelper, we need to create a fake one, unfortunately however this will not pertain the current views viewdata, tempdata or model state so should not be used")]
        private static HtmlHelper CreateHtmlHelper(object model)
        {
            var cc = new ControllerContext
            {
                RequestContext = UmbracoContext.Current.HttpContext.Request.RequestContext
            };
            var viewContext = new ViewContext(cc, new FakeView(), new ViewDataDictionary(model), new TempDataDictionary(), new StringWriter());
            var htmlHelper = new HtmlHelper(viewContext, new ViewPage());
            return htmlHelper;
        }

        private class FakeView : IView
        {
            public void Render(ViewContext viewContext, TextWriter writer)
            {
            }
        }
    }
}
