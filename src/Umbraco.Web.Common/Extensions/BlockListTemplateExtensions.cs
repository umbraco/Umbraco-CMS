using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class BlockListTemplateExtensions
{
    public const string DefaultFolder = "blocklist/";
    public const string DefaultTemplate = "default";

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        var view = DefaultFolder + template;
        return html.Partial(view, model);
    }

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockListHtml(html, property.GetValue() as BlockListModel, template);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockListHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        if (propertyAlias == null)
        {
            throw new ArgumentNullException(nameof(propertyAlias));
        }

        if (string.IsNullOrWhiteSpace(propertyAlias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(propertyAlias));
        }

        IPublishedProperty? prop = contentItem.GetProperty(propertyAlias);
        if (prop == null)
        {
            throw new InvalidOperationException("No property type found with alias " + propertyAlias);
        }

        return GetBlockListHtml(html, prop.GetValue() as BlockListModel, template);
    }
}
