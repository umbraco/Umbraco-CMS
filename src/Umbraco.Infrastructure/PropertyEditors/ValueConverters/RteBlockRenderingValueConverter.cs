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
public class RteBlockRenderingValueConverter : SimpleRichTextValueConverter, IDeliveryApiPropertyValueConverter, IDisposable
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
    private readonly IDisposable? _deliveryApiSettingsChangeSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="RteBlockRenderingValueConverter"/> class.
    /// </summary>
    /// <param name="linkParser">Parses local links within HTML content.</param>
    /// <param name="urlParser">Parses URLs within HTML content.</param>
    /// <param name="imageSourceParser">Parses image sources within HTML content.</param>
    /// <param name="apiRichTextElementParser">Parses rich text elements for the Delivery API.</param>
    /// <param name="apiRichTextMarkupParser">Parses rich text markup for the Delivery API.</param>
    /// <param name="partialViewBlockEngine">Renders partial view blocks within rich text content.</param>
    /// <param name="blockEditorConverter">Converts block editor values.</param>
    /// <param name="jsonSerializer">Serializes and deserializes JSON data.</param>
    /// <param name="apiElementBuilder">Builds API elements for rich text content.</param>
    /// <param name="constructorCache">Caches constructors for rich text block property values.</param>
    /// <param name="logger">The logger used for diagnostic and error messages.</param>
    /// <param name="variationContextAccessor">Provides access to the current variation context.</param>
    /// <param name="blockEditorVarianceHandler">Handles variance for block editors.</param>
    /// <param name="deliveryApiSettingsMonitor">Monitors settings for the Delivery API.</param>
    public RteBlockRenderingValueConverter(
        HtmlLocalLinkParser linkParser,
        HtmlUrlParser urlParser,
        HtmlImageSourceParser imageSourceParser,
        IApiRichTextElementParser apiRichTextElementParser,
        IApiRichTextMarkupParser apiRichTextMarkupParser,
        IPartialViewBlockEngine partialViewBlockEngine,
        BlockEditorConverter blockEditorConverter,
        IJsonSerializer jsonSerializer,
        IApiElementBuilder apiElementBuilder,
        RichTextBlockPropertyValueConstructorCache constructorCache,
        ILogger<RteBlockRenderingValueConverter> logger,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettingsMonitor)
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
        _deliveryApiSettingsChangeSubscription = deliveryApiSettingsMonitor.OnChange(settings => _deliveryApiSettings = settings);
    }

    /// <summary>
    /// Gets the cache level for the property.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    /// The cache level for the property, which is <see cref="PropertyCacheLevel.None"/> because the value must be re-rendered at request time.
    /// This is necessary as the RTE converter parses <c>{locallink}</c> and renders blocks, and the dependencies of block renderings are not known in advance.
    /// </returns>
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>

        // because that version of RTE converter parses {locallink} and renders blocks, its value has
        // to be re-rendered at request time, because we have no idea what the block renderings may depend on actually.
        PropertyCacheLevel.None;

    /// <inheritdoc />
    public override bool? IsValue(object? value, PropertyValueLevel level)
        => level switch
        {
            // we cannot determine if an RTE has a value at source level, because some RTEs might
            // be saved with an "empty" representation like {"markup":"","blocks":null}.
            PropertyValueLevel.Source => null,
            // we assume the RTE has a value if the intermediate value has markup beyond an empty paragraph tag.
            PropertyValueLevel.Inter => value is IRichTextEditorIntermediateValue { Markup.Length: > 0 } intermediateValue
                                        && intermediateValue.Markup != "<p></p>",
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

    /// <summary>
    /// Converts the source value of a rich text editor property to an intermediate <see cref="RichTextEditorIntermediateValue"/> for further processing and caching.
    /// </summary>
    /// <remarks>to counterweigh the cache level, we're going to do as much of the heavy lifting as we can while converting source to intermediate</remarks>
    /// <param name="owner">The <see cref="IPublishedElement"/> that owns the property being converted.</param>
    /// <param name="propertyType">The <see cref="IPublishedPropertyType"/> metadata describing the property.</param>
    /// <param name="source">The source value to convert, typically a JSON string or object representing the rich text editor value.</param>
    /// <param name="preview">A value indicating whether the conversion is being performed in preview mode.</param>
    /// <returns>
    /// A <see cref="RichTextEditorIntermediateValue"/> representing the parsed and processed rich text content, or <c>null</c> if the source value cannot be parsed.
    /// </returns>
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

    /// <summary>
    /// Converts the intermediate value produced by the property editor into the final object representation for use in templates.
    /// </summary>
    /// <param name="owner">The <see cref="IPublishedElement"/> that owns the property being converted.</param>
    /// <param name="propertyType">The <see cref="IPublishedPropertyType"/> describing the property.</param>
    /// <param name="referenceCacheLevel">The <see cref="PropertyCacheLevel"/> indicating the cache level for the property value.</param>
    /// <param name="inter">The intermediate value to convert, typically a string or null.</param>
    /// <param name="preview">True if the conversion is for preview mode; otherwise, false.</param>
    /// <returns>
    /// An <see cref="HtmlEncodedString"/> containing the converted value, or an empty encoded string if the intermediate value is null.
    /// </returns>
    public override object ConvertIntermediateToObject(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel,
        object? inter,
        bool preview)
    {
        var converted = Convert(inter, preview);

        return new HtmlEncodedString(converted ?? string.Empty);
    }

    /// <summary>
    /// Gets the cache level for the delivery API for the specified property type.
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The property cache level.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    /// <summary>
    /// Determines the <see cref="PropertyCacheLevel"/> to use when expanding the property for the Delivery API.
    /// </summary>
    /// <param name="propertyType">The <see cref="IPublishedPropertyType"/> representing the property being expanded.</param>
    /// <returns>The appropriate <see cref="PropertyCacheLevel"/> for Delivery API expansion.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.None;

    /// <summary>
    /// Returns the .NET <see cref="Type"/> that represents the value type delivered by the Delivery API for a given property type.
    /// </summary>
    /// <param name="propertyType">The published property type to evaluate.</param>
    /// <returns>The <see cref="Type"/> of the value returned by the Delivery API.</returns>
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => _deliveryApiSettings.RichTextOutputAsJson
            ? typeof(IRichTextElement)
            : typeof(RichTextModel);

    /// <summary>
    /// Converts the intermediate value of a rich text editor property to an object suitable for the Delivery API.
    /// </summary>
    /// <param name="owner">The published element that owns the property being converted.</param>
    /// <param name="propertyType">The type information for the published property.</param>
    /// <param name="referenceCacheLevel">The cache level to use for property value references during conversion.</param>
    /// <param name="inter">The intermediate value to convert, typically an <see cref="IRichTextEditorIntermediateValue"/>.</param>
    /// <param name="preview">True if the conversion is for preview mode; otherwise, false.</param>
    /// <param name="expanding">True to expand nested values during conversion; otherwise, false.</param>
    /// <returns>
    /// An object representing the converted value for the Delivery API, or <c>null</c> if the intermediate value is empty and JSON output is enabled in settings; otherwise, a <see cref="RichTextModel"/> or parsed API object.
    /// </returns>
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
        sourceString = _linkParser.EnsureInternalLinks(sourceString);
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

    private sealed class RichTextEditorIntermediateValue : IRichTextEditorIntermediateValue
    {
        /// <summary>
        /// Gets or sets the HTML markup content for this rich text editor value.
        /// </summary>
        public required string Markup { get; set; }

        /// <summary>
        /// Gets or sets the model representing the rich text block associated with this intermediate value.
        /// </summary>
        public required RichTextBlockModel? RichTextBlockModel { get; set; }
    }

    /// <summary>
    /// Releases the managed resources used by the <see cref="RteBlockRenderingValueConverter"/>, including disposing the settings change subscription.
    /// </summary>
    public void Dispose() => _deliveryApiSettingsChangeSubscription?.Dispose();
}
