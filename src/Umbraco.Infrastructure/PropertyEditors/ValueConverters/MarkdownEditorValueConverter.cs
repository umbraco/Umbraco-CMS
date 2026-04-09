// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Converts values stored by the Markdown editor property editor into HTML or other displayable formats for use within Umbraco.
/// </summary>
[DefaultPropertyValueConverter]
public class MarkdownEditorValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly HtmlLocalLinkParser _localLinkParser;
    private readonly HtmlUrlParser _urlParser;
    private readonly IMarkdownToHtmlConverter _markdownToHtmlConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownEditorValueConverter"/> class.
    /// </summary>
    /// <param name="localLinkParser">An instance used to parse local links within HTML content.</param>
    /// <param name="urlParser">An instance used to parse URLs within HTML content.</param>
    /// <param name="markdownToHtmlConverter">An instance responsible for converting markdown to HTML.</param>
    public MarkdownEditorValueConverter(HtmlLocalLinkParser localLinkParser, HtmlUrlParser urlParser, IMarkdownToHtmlConverter markdownToHtmlConverter)
    {
        _localLinkParser = localLinkParser;
        _urlParser = urlParser;
        _markdownToHtmlConverter = markdownToHtmlConverter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownEditorValueConverter"/> class.
    /// </summary>
    /// <param name="localLinkParser">An instance of <see cref="HtmlLocalLinkParser"/> used to parse local links in HTML content.</param>
    /// <param name="urlParser">An instance of <see cref="HtmlUrlParser"/> used to parse URLs in HTML content.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public MarkdownEditorValueConverter(HtmlLocalLinkParser localLinkParser, HtmlUrlParser urlParser)
        : this(
              localLinkParser,
              urlParser,
              StaticServiceProvider.Instance.GetRequiredService<IMarkdownToHtmlConverter>())
    {
    }

    /// <summary>
    /// Returns a value indicating whether this converter is applicable to the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type to evaluate.</param>
    /// <returns><c>true</c> if the converter can handle the specified property type; otherwise, <c>false</c>.</returns>
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.MarkdownEditor.Equals(propertyType.EditorAlias);

    /// <summary>
    /// Gets the .NET type of the property value returned by the Markdown editor for the specified property type.
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The <see cref="IHtmlEncodedString"/> type representing the property value.</returns>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IHtmlEncodedString);

    /// <summary>
    /// Gets the cache level for the specified property type.
    /// For the Markdown editor, this always returns <see cref="PropertyCacheLevel.Snapshot"/>.
    /// </summary>
    /// <param name="propertyType">The property type for which to determine the cache level.</param>
    /// <returns>The cache level, which is always <see cref="PropertyCacheLevel.Snapshot"/> for this value converter.</returns>
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    /// <summary>
    /// Converts the source value of a Markdown editor property to an intermediate string format by parsing and resolving internal links and URLs.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The metadata describing the property type.</param>
    /// <param name="source">The source value to convert, expected to be a string or convertible to string.</param>
    /// <param name="preview">A value indicating whether the conversion is for preview mode.</param>
    /// <returns>
    /// The processed string with internal links and URLs resolved, or <c>null</c> if <paramref name="source"/> is <c>null</c>.
    /// </returns>
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        var sourceString = source.ToString()!;

        // ensures string is parsed for {localLink} and URLs are resolved correctly
        sourceString = _localLinkParser.EnsureInternalLinks(sourceString);
        sourceString = _urlParser.EnsureUrls(sourceString);

        return sourceString;
    }

    /// <summary>
    /// Converts the intermediate value, expected to be a Markdown string, into an HTML-encoded string suitable for frontend rendering.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The metadata describing the property type.</param>
    /// <param name="referenceCacheLevel">The cache level at which the property value is stored.</param>
    /// <param name="inter">The intermediate value to convert, typically a Markdown string or <c>null</c>.</param>
    /// <param name="preview">A value indicating whether the conversion is for preview purposes.</param>
    /// <returns>
    /// An <see cref="HtmlEncodedString"/> containing the HTML representation of the Markdown content, or an empty <see cref="HtmlEncodedString"/> if the input is <c>null</c>.
    /// </returns>
    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // Convert markup to HTML for frontend rendering.
        // Source should come from ConvertSource and be a string (or null) already.
        if (inter is null)
        {
            return new HtmlEncodedString(string.Empty);
        }

        var htmlString = _markdownToHtmlConverter.ToHtml((string)inter);
        return new HtmlEncodedString(htmlString);
    }

    /// <summary>
    /// Returns the cache level to use for the Delivery API for a given Markdown property type.
    /// </summary>
    /// <param name="propertyType">The <see cref="IPublishedPropertyType"/> representing the Markdown property.</param>
    /// <returns>The <see cref="PropertyCacheLevel"/> to use for the Delivery API.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Element;

    /// <summary>
    /// Gets the delivery API property value type for the specified published property type.
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The type of the value returned by the delivery API.</returns>
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(string);

    /// <summary>
    /// Converts the intermediate value of a Markdown editor property to an object suitable for the Delivery API.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="referenceCacheLevel">The cache level for property references.</param>
    /// <param name="inter">The intermediate value to convert.</param>
    /// <param name="preview">True if the conversion is for preview mode; otherwise, false.</param>
    /// <param name="expanding">True if the value is being expanded; otherwise, false.</param>
    /// <returns>The Markdown string if present; otherwise, an empty string.</returns>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        if (inter is not string markdownString || markdownString.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        return markdownString;
    }
}
