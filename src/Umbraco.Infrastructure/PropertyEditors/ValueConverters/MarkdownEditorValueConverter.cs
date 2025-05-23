// Copyright (c) Umbraco.
// See LICENSE for more details.

using HeyRed.MarkdownSharp;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class MarkdownEditorValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly HtmlLocalLinkParser _localLinkParser;
    private readonly HtmlUrlParser _urlParser;

    public MarkdownEditorValueConverter(HtmlLocalLinkParser localLinkParser, HtmlUrlParser urlParser)
    {
        _localLinkParser = localLinkParser;
        _urlParser = urlParser;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.MarkdownEditor.Equals(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IHtmlEncodedString);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        var sourceString = source.ToString()!;

        // ensures string is parsed for {localLink} and URLs are resolved correctly
        sourceString = _localLinkParser.EnsureInternalLinks(sourceString, preview);
        sourceString = _urlParser.EnsureUrls(sourceString);

        return sourceString;
    }

    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // convert markup to HTML for frontend rendering.
        // source should come from ConvertSource and be a string (or null) already
        var mark = new Markdown();
        return new HtmlEncodedString(inter == null ? string.Empty : mark.Transform((string)inter));
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Element;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(string);

    public object ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        if (inter is not string markdownString || markdownString.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        return markdownString;
    }
}
