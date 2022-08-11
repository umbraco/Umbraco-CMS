// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class BlockGridTemplateExtensions
{
    public const string DefaultFolder = "blockgrid/";
    public const string DefaultTemplate = "default";
    public const string DefaultItemsTemplate = "items";
    public const string DefaultItemAreasTemplate = "areas";

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, BlockGridModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        var view = $"{DefaultFolder}{template}";
        return await html.PartialAsync(view, model);
    }

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockGridHtmlAsync(html, property.GetValue() as BlockGridModel, template);

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockGridHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
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

        return await GetBlockGridHtmlAsync(html, prop.GetValue() as BlockGridModel, template);
    }

    public static async Task<IHtmlContent> GetBlockGridItemsHtmlAsync(this IHtmlHelper html, IEnumerable<BlockGridItem> items, string template = DefaultItemsTemplate)
        => await html.PartialAsync($"{DefaultFolder}{template}", items);

    public static async Task<IHtmlContent> GetBlockGridItemAreasHtmlAsync(this IHtmlHelper html, BlockGridItem item, string template = DefaultItemAreasTemplate)
        => await html.PartialAsync($"{DefaultFolder}{template}", item);
}
