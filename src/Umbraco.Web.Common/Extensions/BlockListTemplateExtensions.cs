using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class BlockListTemplateExtensions
{
    public const string DefaultFolder = "blocklist/";
    public const string DefaultTemplate = "default";

    /// <summary>
    /// ViewData key under which the block list property alias is passed to the partial view,
    /// enabling the visual editor to annotate (and offer block creation on) empty block lists.
    /// </summary>
    public const string PropertyAliasViewDataKey = "umbBlockListPropertyAlias";

    #region Async

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, property.Alias);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockListHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, propertyAlias);
    }

    private static async Task<IHtmlContent> GetBlockListHtmlAsync(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias)
        => await html.PartialAsync(DefaultFolderTemplate(template), model, WithPropertyAlias(html, propertyAlias));

    #endregion

    #region Sync

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockListHtml(html, property.GetValue() as BlockListModel, template, property.Alias);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockListHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockListHtml(html, property.GetValue() as BlockListModel, template, propertyAlias);
    }

    private static IHtmlContent GetBlockListHtml(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias)
        => html.Partial(DefaultFolderTemplate(template), model, WithPropertyAlias(html, propertyAlias));

    #endregion

    private static ViewDataDictionary WithPropertyAlias(IHtmlHelper html, string propertyAlias)
        => new(html.ViewData) { [PropertyAliasViewDataKey] = propertyAlias };

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
