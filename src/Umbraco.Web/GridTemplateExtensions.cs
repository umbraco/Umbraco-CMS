using System;
using System.Web.Mvc.Html;
using System.Web.Mvc;
using System.IO;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{

    public static class GridTemplateExtensions
    {
        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedProperty property, string framework = "bootstrap3")
        {
            var asString = property.GetValue() as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            var view = "Grid/" + framework;
            return html.Partial(view, property.GetValue());
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem)
        {
            return html.GetGridHtml(contentItem, "bodyText", "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        {
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentNullOrEmptyException(nameof(propertyAlias));

            return html.GetGridHtml(contentItem, propertyAlias, "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias, string framework)
        {
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentNullOrEmptyException(nameof(propertyAlias));

            var view = "Grid/" + framework;
            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new NullReferenceException("No property type found with alias " + propertyAlias);
            var model = prop.GetValue();

            var asString = model as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            return html.Partial(view, model);
        }

        public static MvcHtmlString GetGridHtml(this IPublishedProperty property, HtmlHelper html, string framework = "bootstrap3")
        {
            var asString = property.GetValue() as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            var view = "Grid/" + framework;
            return html.Partial(view, property.GetValue());
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html)
        {
            return GetGridHtml(contentItem, html, "bodyText", "bootstrap3");
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias)
        {
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentNullOrEmptyException(nameof(propertyAlias));

            return GetGridHtml(contentItem, html, propertyAlias, "bootstrap3");
        }
        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias, string framework)
        {
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentNullOrEmptyException(nameof(propertyAlias));

            var view = "Grid/" + framework;
            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new NullReferenceException("No property type found with alias " + propertyAlias);
            var model = prop.GetValue();

            var asString = model as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            return html.Partial(view, model);
        }

        private class FakeView : IView
        {
            public void Render(ViewContext viewContext, TextWriter writer)
            {
            }
        }
    }
}
