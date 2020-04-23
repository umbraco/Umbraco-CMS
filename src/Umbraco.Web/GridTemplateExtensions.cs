using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
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
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyAlias));

            return html.GetGridHtml(contentItem, propertyAlias, "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias, string framework)
        {
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyAlias));

            var view = "Grid/" + framework;
            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new InvalidOperationException("No property type found with alias " + propertyAlias);
            var model = prop.GetValue();

            var asString = model as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            return html.Partial(view, model);
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedElement contentItem)
        {
            return html.GetGridHtml(contentItem, "bodyText", "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedElement contentItem, string propertyAlias)
        {
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyAlias));

            return html.GetGridHtml(contentItem, propertyAlias, "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this HtmlHelper html, IPublishedElement contentItem, string propertyAlias, string framework)
        {
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyAlias));

            var view = "Grid/" + framework;
            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new InvalidOperationException("No property type found with alias " + propertyAlias);
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
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyAlias));

            return GetGridHtml(contentItem, html, propertyAlias, "bootstrap3");
        }

        public static MvcHtmlString GetGridHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias, string framework)
        {
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyAlias));

            var view = "Grid/" + framework;
            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new InvalidOperationException("No property type found with alias " + propertyAlias);
            var model = prop.GetValue();

            var asString = model as string;
            if (asString != null && string.IsNullOrEmpty(asString)) return new MvcHtmlString(string.Empty);

            return html.Partial(view, model);
        }
    }
}
