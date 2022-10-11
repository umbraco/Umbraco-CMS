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

    #region Async

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, BlockGridModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockGridHtmlAsync(html, property.GetValue() as BlockGridModel, template);

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockGridHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty prop = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockGridHtmlAsync(html, prop.GetValue() as BlockGridModel, template);
    }

    public static async Task<IHtmlContent> GetBlockGridItemsHtmlAsync(this IHtmlHelper html, IEnumerable<BlockGridItem> items, string template = DefaultItemsTemplate)
        => await html.PartialAsync(DefaultFolderTemplate(template), items);

    public static async Task<IHtmlContent> GetBlockGridItemAreasHtmlAsync(this IHtmlHelper html, BlockGridItem item, string template = DefaultItemAreasTemplate)
        => await html.PartialAsync(DefaultFolderTemplate(template), item);

    #endregion

    #region Sync

    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, BlockGridModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockGridHtml(html, property.GetValue() as BlockGridModel, template);

    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockGridHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty prop = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockGridHtml(html, prop.GetValue() as BlockGridModel, template);
    }

    public static IHtmlContent GetBlockGridItemsHtml(this IHtmlHelper html, IEnumerable<BlockGridItem> items, string template = DefaultItemsTemplate)
        => html.Partial(DefaultFolderTemplate(template), items);

    public static IHtmlContent GetBlockGridItemAreasHtml(this IHtmlHelper html, BlockGridItem item, string template = DefaultItemAreasTemplate)
        => html.Partial(DefaultFolderTemplate(template), item);

    #endregion

    private static string DefaultFolderTemplate(string template) => $"{DefaultFolder}{template}";

    private static IPublishedProperty GetRequiredProperty(IPublishedContent contentItem, string propertyAlias)
    {
        ArgumentNullException.ThrowIfNull(propertyAlias);

        if (string.IsNullOrWhiteSpace(propertyAlias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(propertyAlias));
        }

        IPublishedProperty? property = contentItem.GetProperty(propertyAlias);
        if (property == null)
        {
            throw new InvalidOperationException("No property type found with alias " + propertyAlias);
        }

        return property;
    }
}
