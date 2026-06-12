using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class BlockListTemplateExtensions
{
    public const string DefaultFolder = "blocklist/";
    public const string DefaultTemplate = "default";

    #region Async

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return HtmlString.Empty;
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockListHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);
    }

    private static async Task<IHtmlContent> GetBlockListHtmlAsync(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null || model.Count == 0)
        {
            return BlockEmptyState.Container("umb-block-list", propertyAlias, editableInVisualEditor);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    #endregion

    #region Sync

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return HtmlString.Empty;
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockListHtml(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockListHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockListHtml(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);
    }

    private static IHtmlContent GetBlockListHtml(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null || model.Count == 0)
        {
            return BlockEmptyState.Container("umb-block-list", propertyAlias, editableInVisualEditor);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
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
