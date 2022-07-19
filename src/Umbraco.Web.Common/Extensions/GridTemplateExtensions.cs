using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class GridTemplateExtensions
{
    public static IHtmlContent GetGridHtml(this IHtmlHelper html, IPublishedProperty property, string framework = "bootstrap3")
    {
        if (property.GetValue() is string asString && string.IsNullOrEmpty(asString))
        {
            return new HtmlString(string.Empty);
        }

        var view = "grid/" + framework;
        return html.Partial(view, property.GetValue());
    }

    public static IHtmlContent GetGridHtml(this IHtmlHelper html, IPublishedContent contentItem) =>
        html.GetGridHtml(contentItem, "bodyText", "bootstrap3");

    public static IHtmlContent GetGridHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
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

        return html.GetGridHtml(contentItem, propertyAlias, "bootstrap3");
    }

    public static IHtmlContent GetGridHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string framework)
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

        var view = "grid/" + framework;
        IPublishedProperty? prop = contentItem.GetProperty(propertyAlias);
        if (prop == null)
        {
            throw new InvalidOperationException("No property type found with alias " + propertyAlias);
        }

        var model = prop.GetValue();

        if (model is string asString && string.IsNullOrEmpty(asString))
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(view, model);
    }

    public static IHtmlContent GetGridHtml(this IHtmlHelper html, IPublishedElement contentItem) =>
        html.GetGridHtml(contentItem, "bodyText", "bootstrap3");

    public static IHtmlContent GetGridHtml(this IHtmlHelper html, IPublishedElement contentItem, string propertyAlias)
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

        return html.GetGridHtml(contentItem, propertyAlias, "bootstrap3");
    }

    public static IHtmlContent GetGridHtml(this IHtmlHelper html, IPublishedElement contentItem, string propertyAlias, string framework)
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

        var view = "grid/" + framework;
        IPublishedProperty? prop = contentItem.GetProperty(propertyAlias);
        if (prop == null)
        {
            throw new InvalidOperationException("No property type found with alias " + propertyAlias);
        }

        var model = prop.GetValue();

        if (model is string asString && string.IsNullOrEmpty(asString))
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(view, model);
    }

    public static IHtmlContent GetGridHtml(this IPublishedProperty property, IHtmlHelper html, string framework = "bootstrap3")
    {
        if (property.GetValue() is string asString && string.IsNullOrEmpty(asString))
        {
            return new HtmlString(string.Empty);
        }

        var view = "grid/" + framework;
        return html.Partial(view, property.GetValue());
    }

    public static IHtmlContent GetGridHtml(this IPublishedContent contentItem, IHtmlHelper html) =>
        GetGridHtml(contentItem, html, "bodyText", "bootstrap3");

    public static IHtmlContent GetGridHtml(this IPublishedContent contentItem, IHtmlHelper html, string propertyAlias)
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

        return GetGridHtml(contentItem, html, propertyAlias, "bootstrap3");
    }

    public static IHtmlContent GetGridHtml(this IPublishedContent contentItem, IHtmlHelper html, string propertyAlias, string framework)
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

        var view = "grid/" + framework;
        IPublishedProperty? prop = contentItem.GetProperty(propertyAlias);
        if (prop == null)
        {
            throw new InvalidOperationException("No property type found with alias " + propertyAlias);
        }

        var model = prop.GetValue();

        if (model is string asString && string.IsNullOrEmpty(asString))
        {
            return new HtmlString(string.Empty);
        }

        return html.Partial(view, model);
    }
}
