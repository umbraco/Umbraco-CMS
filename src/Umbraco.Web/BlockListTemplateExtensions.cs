using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    public static class BlockListTemplateExtensions
    {
        public const string DefaultFolder = "BlockList/";
        public const string DefaultTemplate = "Default";

        public static MvcHtmlString GetBlockListHtml(this HtmlHelper html, BlockListModel model, string template = DefaultTemplate)
        {
            if (model?.Layout == null || !model.Layout.Any()) return new MvcHtmlString(string.Empty);

            var view = DefaultFolder + template;
            return html.Partial(view, model);
        }

        public static MvcHtmlString GetBlockListHtml(this HtmlHelper html, IPublishedProperty property, string template = DefaultTemplate) => GetBlockListHtml(html, property?.GetValue() as BlockListModel, template);

        public static MvcHtmlString GetBlockListHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias) => GetBlockListHtml(html, contentItem, propertyAlias, DefaultTemplate);

        public static MvcHtmlString GetBlockListHtml(this HtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
        {
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));
            if (string.IsNullOrWhiteSpace(propertyAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyAlias));

            var prop = contentItem.GetProperty(propertyAlias);
            if (prop == null) throw new InvalidOperationException("No property type found with alias " + propertyAlias);

            return GetBlockListHtml(html, prop?.GetValue() as BlockListModel, template);
        }

        public static MvcHtmlString GetBlockListHtml(this IPublishedProperty property, HtmlHelper html, string template = DefaultTemplate) => GetBlockListHtml(html, property?.GetValue() as BlockListModel, template);

        public static MvcHtmlString GetBlockListHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias) => GetBlockListHtml(html, contentItem, propertyAlias, DefaultTemplate);

        public static MvcHtmlString GetBlockListHtml(this IPublishedContent contentItem, HtmlHelper html, string propertyAlias, string template) => GetBlockListHtml(html, contentItem, propertyAlias, template);
    }
}
