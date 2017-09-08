using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Models;
using System.Web.Mvc;
using Umbraco.Web.Templates;
using System.IO;
using System.Web.Routing;
using Umbraco.Web.Mvc;
using Newtonsoft.Json.Linq;

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
        
        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, bool recurse)
        {
            return html.GetGridHtml(contentItem, "bodyText", recurse, "bootstrap3");
        }
        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias, bool recurse)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            return html.GetGridHtml(contentItem, propertyAlias, recurse, "bootstrap3");
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html, bool recurse)
        {
            return html.GetGridHtml(contentItem, "bodyText", recurse, "bootstrap3");
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias, bool recurse)
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");

            return html.GetGridHtml(contentItem, propertyAlias, recurse, "bootstrap3");
        }
        /// <summary>
        /// Is this the best place to put this?
        /// Essentially it's a work around for the fact that HasValue doesn't work consistently with the Grid
        /// If a Grid property exists and hasn't been used HasValue functions as expected and returns false
        /// However if controls have been added to the grid, and then deleted, along with any rows, then some 'Json' remains 
        /// and HasValue returns true when to the editor it is completely empty
        /// so here we use JObject SelectTokens to look for controls to determine if there is any content in the remaining Json
        /// Is that the best approach?
        ///  it may make more sense to investigate cleaning up a grid property when it has had all controls removed so it doesn't store anything and HasValue would work again
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static bool HasGridValue(IPublishedProperty property)
        {
            if (property == null) { return false; }
            if (!property.HasValue) { return false; }
            JObject grid = property.GetValue<JObject>();
            if (grid == null) { return false; }
            return grid.SelectTokens("sections[*].rows[*].areas[*].controls").Any();
        }
        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias, bool recurse, string framework = "bootstrap3")
        {
            Mandate.ParameterNotNullOrEmpty(propertyAlias, "propertyAlias");
            if (recurse == false) return html.GetGridHtml(contentItem, propertyAlias, framework);
            var property = contentItem.GetProperty(propertyAlias);
            var firstNonNullProperty = property;
            while (contentItem != null && (property == null || property.HasValue == false || (property.HasValue == true && HasGridValue(property) == false)))
            {
                contentItem = contentItem.Parent;
                property = contentItem == null ? null : contentItem.GetProperty(propertyAlias);
                if (firstNonNullProperty == null && property != null) firstNonNullProperty = property;
            }

            var view = "Grid/" + framework;

            if (property == null) throw new NullReferenceException("No property type found with alias using recurse " + propertyAlias);
            var model = property.Value;

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
