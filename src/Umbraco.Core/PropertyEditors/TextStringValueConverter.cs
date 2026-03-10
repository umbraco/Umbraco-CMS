using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Provides property value conversion for text string properties (TextBox and TextArea).
/// </summary>
[DefaultPropertyValueConverter]
public class TextStringValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private static readonly string[] PropertyTypeAliases =
    {
        Constants.PropertyEditors.Aliases.TextBox, Constants.PropertyEditors.Aliases.TextArea,
    };

    private readonly HtmlLocalLinkParser _linkParser;
    private readonly HtmlUrlParser _urlParser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextStringValueConverter" /> class.
    /// </summary>
    /// <param name="linkParser">The local link parser for resolving internal links.</param>
    /// <param name="urlParser">The URL parser for resolving URLs.</param>
    public TextStringValueConverter(HtmlLocalLinkParser linkParser, HtmlUrlParser urlParser)
    {
        _linkParser = linkParser;
        _urlParser = urlParser;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => PropertyTypeAliases.Contains(propertyType.EditorAlias);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        var sourceString = source.ToString();

        // ensures string is parsed for {localLink} and URLs are resolved correctly
        sourceString = _linkParser.EnsureInternalLinks(sourceString!);
        sourceString = _urlParser.EnsureUrls(sourceString);

        return sourceString;
    }

    /// <inheritdoc />
    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview) =>

        // source should come from ConvertSource and be a string (or null) already
        inter ?? string.Empty;

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => GetPropertyValueType(propertyType);

    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
}
