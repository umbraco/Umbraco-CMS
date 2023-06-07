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
    public const string DefaultItemAreaTemplate = "area";

    #region Async

    /// <summary>
    /// Renders a block grid model into a grid layout
    /// </summary>
    /// <remarks>
    /// By default this method uses a set of built-in partial views for rendering the blocks and areas in the grid model.
    /// These partial views are embedded in the static assets (Umbraco.Cms.StaticAssets), so they won't show up in the
    /// Views folder on your local disk.
    ///
    /// If you need to tweak the grid rendering output, you can copy the partial views from GitHub to your local disk.
    /// The partial views are found in "/src/Umbraco.Cms.StaticAssets/Views/Partials/blockgrid/" on GitHub and should
    /// be copied to "Views/Partials/BlockGrid/" on your local disk.
    /// </remarks>
    /// <seealso href="https://docs.umbraco.com/umbraco-cms/fundamentals/backoffice/property-editors/built-in-umbraco-property-editors/block-editor/block-grid-editor#1.-default-rendering"/>
    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, BlockGridModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockGridHtmlAsync(html, property.GetValue() as BlockGridModel, template);

    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
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

    public static async Task<IHtmlContent> GetBlockGridItemAreaHtmlAsync(this IHtmlHelper html, BlockGridArea area, string template = DefaultItemAreaTemplate)
        => await html.PartialAsync(DefaultFolderTemplate(template), area);

    public static async Task<IHtmlContent> GetBlockGridItemAreaHtmlAsync(this IHtmlHelper html, BlockGridItem item, string areaAlias, string template = DefaultItemAreaTemplate)
    {
        BlockGridArea? area = item.Areas.FirstOrDefault(a => a.Alias == areaAlias);
        if (area == null)
        {
            return new HtmlString(string.Empty);
        }

        return await GetBlockGridItemAreaHtmlAsync(html, area, template);
    }

    #endregion

    #region Sync

    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, BlockGridModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockGridHtml(html, property.GetValue() as BlockGridModel, template);

    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
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

    public static IHtmlContent GetBlockGridItemAreaHtml(this IHtmlHelper html, BlockGridArea area, string template = DefaultItemAreaTemplate)
        => html.Partial(DefaultFolderTemplate(template), area);

    public static IHtmlContent GetBlockGridItemAreaHtml(this IHtmlHelper html, BlockGridItem item, string areaAlias, string template = DefaultItemAreaTemplate)
    {
        BlockGridArea? area = item.Areas.FirstOrDefault(a => a.Alias == areaAlias);
        return area != null
            ? GetBlockGridItemAreaHtml(html, area, template)
            : new HtmlString(string.Empty);
    }

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
