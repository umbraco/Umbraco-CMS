using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class SingleBlockTemplateExtensions
{
    public const string DefaultFolder = "singleblock/";
    public const string DefaultTemplate = "default";

    #region Async

    public static async Task<IHtmlContent> GetBlockHtmlAsync(this IHtmlHelper html, BlockListItem? model, string template = DefaultTemplate)
    {
        if (model is null)
        {
            return new HtmlString(string.Empty);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    public static async Task<IHtmlContent> GetBlockHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockHtmlAsync(html, property.GetValue() as BlockListItem, template);

    public static async Task<IHtmlContent> GetBlockHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockHtmlAsync(html, property.GetValue() as BlockListItem, template);
    }
    #endregion

    #region Sync

    public static IHtmlContent GetBlockHtml(this IHtmlHelper html, BlockListItem? model, string template = DefaultTemplate)
    {
        if (model is null)
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    public static IHtmlContent GetBlockHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockHtml(html, property.GetValue() as BlockListItem, template);

    public static IHtmlContent GetBlockHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockHtml(html, property.GetValue() as BlockListItem, template);
    }

    public static string SingleBlockPartialWithFallback(this IHtmlHelper html, string template, string fallbackTemplate)
    {
        IServiceProvider requestServices = html.ViewContext.HttpContext.RequestServices;
        ICompositeViewEngine? viewEngine = requestServices.GetService<ICompositeViewEngine>();
        if (viewEngine is null)
        {
            return template;
        }

        // .Getview, and likely .FindView, will be invoked when invoking html.Partial
        // the heavy lifting in the underlying logic seems to be cached so it should be ok to offer this logic
        // as a DX feature in the default block renderer.
        return
            viewEngine.GetView(html.ViewContext.ExecutingFilePath, template, isMainPage: false).Success
                ? template
                : viewEngine.FindView(html.ViewContext, template, isMainPage: false).Success
                ? template
                : fallbackTemplate;
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
