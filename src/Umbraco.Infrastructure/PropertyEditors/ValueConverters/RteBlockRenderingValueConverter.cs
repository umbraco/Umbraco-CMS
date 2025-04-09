// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Blocks;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     A value converter for TinyMCE that will ensure any blocks content are rendered properly even when
///     used dynamically.
/// </summary>
[DefaultPropertyValueConverter]
public class RteBlockRenderingValueConverter : SimpleRichTextValueConverter, IDeliveryApiPropertyValueConverter
{
    private readonly HtmlImageSourceParser _imageSourceParser;
    private readonly HtmlLocalLinkParser _linkParser;
    private readonly HtmlUrlParser _urlParser;
    private readonly IApiRichTextElementParser _apiRichTextElementParser;
    private readonly IApiRichTextMarkupParser _apiRichTextMarkupParser;
    private readonly IPartialViewBlockEngine _partialViewBlockEngine;
    private readonly BlockEditorConverter _blockEditorConverter;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RteBlockRenderingValueConverter> _logger;
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly RichTextBlockPropertyValueConstructorCache _constructorCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;
    private DeliveryApiSettings _deliveryApiSettings;

    public RteBlockRenderingValueConverter(HtmlLocalLinkParser linkParser, HtmlUrlParser urlParser, HtmlImageSourceParser imageSourceParser,
        IApiRichTextElementParser apiRichTextElementParser, IApiRichTextMarkupParser apiRichTextMarkupParser,
        IPartialViewBlockEngine partialViewBlockEngine, BlockEditorConverter blockEditorConverter, IJsonSerializer jsonSerializer,
        IApiElementBuilder apiElementBuilder, RichTextBlockPropertyValueConstructorCache constructorCache, ILogger<RteBlockRenderingValueConverter> logger,
        IVariationContextAccessor variationContextAccessor, BlockEditorVarianceHandler blockEditorVarianceHandler, IOptionsMonitor<DeliveryApiSettings> deliveryApiSettingsMonitor)
    {
        _linkParser = linkParser;
        _urlParser = urlParser;
        _imageSourceParser = imageSourceParser;
        _apiRichTextElementParser = apiRichTextElementParser;
        _apiRichTextMarkupParser = apiRichTextMarkupParser;
        _partialViewBlockEngine = partialViewBlockEngine;
        _blockEditorConverter = blockEditorConverter;
        _jsonSerializer = jsonSerializer;
        _apiElementBuilder = apiElementBuilder;
        _constructorCache = constructorCache;
        _logger = logger;
        _variationContextAccessor = variationContextAccessor;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
        _deliveryApiSettings = deliveryApiSettingsMonitor.CurrentValue;
        deliveryApiSettingsMonitor.OnChange(settings => _deliveryApiSettings = settings);
    }

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>

        // because that version of RTE converter parses {locallink} and renders blocks, its value has
        // to be cached at the published snapshot level, because we have no idea what the block renderings may depend on actually.
        PropertyCacheLevel.Snapshot;

    // to counterweigh the cache level, we're going to do as much of the heavy lifting as we can while converting source to intermediate
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (RichTextPropertyEditorHelper.TryParseRichTextEditorValue(source, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue) is false)
        {
            return null;
        }

        // the reference cache level is .Element here, as is also the case when rendering at property level.
        RichTextBlockModel? richTextBlockModel = richTextEditorValue.Blocks is not null
            ? ParseRichTextBlockModel(owner, richTextEditorValue.Blocks, propertyType, PropertyCacheLevel.Element, preview)
            : null;

        return new RichTextEditorIntermediateValue
        {
            Markup = richTextEditorValue.Markup,
            RichTextBlockModel = richTextBlockModel
        };
    }

    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var converted = Convert(inter, preview);

        return new HtmlEncodedString(converted ?? string.Empty);
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => _deliveryApiSettings.RichTextOutputAsJson
            ? typeof(IRichTextElement)
            : typeof(RichTextModel);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        if (inter is not IRichTextEditorIntermediateValue richTextEditorIntermediateValue
            || richTextEditorIntermediateValue.Markup.IsNullOrWhiteSpace())
        {
            // different return types for the JSON configuration forces us to have different return values for empty properties
            return _deliveryApiSettings.RichTextOutputAsJson is false
                ? RichTextModel.Empty()
                : null;
        }

        return _deliveryApiSettings.RichTextOutputAsJson is false
            ? CreateRichTextModel(richTextEditorIntermediateValue)
            : _apiRichTextElementParser.Parse(richTextEditorIntermediateValue.Markup, richTextEditorIntermediateValue.RichTextBlockModel);
    }

    private string? Convert(object? source, bool preview)
    {
        if (source is not IRichTextEditorIntermediateValue intermediateValue)
        {
            return null;
        }

        var sourceString = intermediateValue.Markup;

        // ensures string is parsed for {localLink} and URLs and media are resolved correctly
        sourceString = _linkParser.EnsureInternalLinks(sourceString, preview);
        sourceString = _urlParser.EnsureUrls(sourceString);
        sourceString = _imageSourceParser.EnsureImageSources(sourceString);

        // render blocks
        sourceString = RenderRichTextBlockModel(sourceString, intermediateValue.RichTextBlockModel);

        // find and remove the rel attributes used in the Umbraco UI from img tags
        var doc = new HtmlDocument();
        doc.LoadHtml(sourceString);

        if (doc.ParseErrors.Any() == false && doc.DocumentNode != null)
        {
            // Find all images with rel attribute
            HtmlNodeCollection? imgNodes = doc.DocumentNode.SelectNodes("//img[@rel]");

            var modified = false;
            if (imgNodes != null)
            {
                foreach (HtmlNode? img in imgNodes)
                {
                    var nodeId = img.GetAttributeValue("rel", string.Empty);
                    if (int.TryParse(nodeId, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        img.Attributes.Remove("rel");
                        modified = true;
                    }
                }
            }

            // Find all a and img tags with a data-udi attribute
            HtmlNodeCollection? dataUdiNodes = doc.DocumentNode.SelectNodes("(//a|//img)[@data-udi]");
            if (dataUdiNodes != null)
            {
                foreach (HtmlNode? node in dataUdiNodes)
                {
                    node.Attributes.Remove("data-udi");
                    modified = true;
                }
            }

            if (modified)
            {
                return doc.DocumentNode.OuterHtml;
            }
        }

        return sourceString;
    }

    private RichTextBlockModel? ParseRichTextBlockModel(IPublishedElement owner, RichTextBlockValue blocks, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, bool preview)
    {
        RichTextConfiguration? configuration = propertyType.DataType.ConfigurationAs<RichTextConfiguration>();
        if (configuration?.Blocks?.Any() is not true)
        {
            return null;
        }

        var creator = new RichTextBlockPropertyValueCreator(_blockEditorConverter, _variationContextAccessor, _blockEditorVarianceHandler, _jsonSerializer, _constructorCache);
        return creator.CreateBlockModel(owner, referenceCacheLevel, blocks, preview, configuration.Blocks);
    }

    private string RenderRichTextBlockModel(string source, RichTextBlockModel? richTextBlockModel)
    {
        if (richTextBlockModel is null || richTextBlockModel.Any() is false)
        {
            return source;
        }

        var blocksByKey = richTextBlockModel.ToDictionary(block => block.ContentKey);

        string RenderBlock(Match match) =>
            Guid.TryParse(match.Groups["key"].Value, out Guid key) && blocksByKey.TryGetValue(key, out RichTextBlockItem? richTextBlockItem)
                ? _partialViewBlockEngine.ExecuteAsync(richTextBlockItem).GetAwaiter().GetResult()
                : string.Empty;

        return RichTextParsingRegexes.BlockRegex().Replace(source, RenderBlock);
    }

    private RichTextModel CreateRichTextModel(IRichTextEditorIntermediateValue richTextEditorIntermediateValue)
    {
        var markup = _apiRichTextMarkupParser.Parse(richTextEditorIntermediateValue.Markup);

        ApiBlockItem[] blocks = richTextEditorIntermediateValue.RichTextBlockModel is not null
            ? richTextEditorIntermediateValue.RichTextBlockModel
                .Select(item => item.CreateApiBlockItem(_apiElementBuilder))
                .ToArray()
            : Array.Empty<ApiBlockItem>();

        return new RichTextModel
        {
            Markup = markup,
            Blocks = blocks
        };
    }

    private class RichTextEditorIntermediateValue : IRichTextEditorIntermediateValue
    {
        public required string Markup { get; set; }

        public required RichTextBlockModel? RichTextBlockModel { get; set; }
    }
}
