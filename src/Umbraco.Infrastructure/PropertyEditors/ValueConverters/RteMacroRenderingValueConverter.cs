// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Blocks;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Infrastructure.Macros;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     A value converter for TinyMCE that will ensure any macro content is rendered properly even when
///     used dynamically.
/// </summary>
[DefaultPropertyValueConverter]
public class RteMacroRenderingValueConverter : SimpleTinyMceValueConverter, IDeliveryApiPropertyValueConverter
{
    private readonly HtmlImageSourceParser _imageSourceParser;
    private readonly HtmlLocalLinkParser _linkParser;
    private readonly IMacroRenderer _macroRenderer;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly HtmlUrlParser _urlParser;
    private readonly IApiRichTextElementParser _apiRichTextElementParser;
    private readonly IApiRichTextMarkupParser _apiRichTextMarkupParser;
    private readonly IPartialViewBlockEngine _partialViewBlockEngine;
    private readonly BlockEditorConverter _blockEditorConverter;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RteMacroRenderingValueConverter> _logger;
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly RichTextBlockPropertyValueConstructorCache _constructorCache;
    private DeliveryApiSettings _deliveryApiSettings;

    [Obsolete("Please use the constructor that takes all arguments. Will be removed in V14.")]
    public RteMacroRenderingValueConverter(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer,
        HtmlLocalLinkParser linkParser, HtmlUrlParser urlParser, HtmlImageSourceParser imageSourceParser)
        : this(
            umbracoContextAccessor,
            macroRenderer,
            linkParser,
            urlParser,
            imageSourceParser,
            StaticServiceProvider.Instance.GetRequiredService<IApiRichTextElementParser>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiRichTextMarkupParser>(),
            StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<DeliveryApiSettings>>())
    {
    }

    [Obsolete("Please use the constructor that takes all arguments. Will be removed in V15.")]
    public RteMacroRenderingValueConverter(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer,
        HtmlLocalLinkParser linkParser, HtmlUrlParser urlParser, HtmlImageSourceParser imageSourceParser,
        IApiRichTextElementParser apiRichTextElementParser, IApiRichTextMarkupParser apiRichTextMarkupParser, IOptionsMonitor<DeliveryApiSettings> deliveryApiSettingsMonitor)
        : this(
            umbracoContextAccessor,
            macroRenderer,
            linkParser,
            urlParser,
            imageSourceParser,
            apiRichTextElementParser,
            apiRichTextMarkupParser,
            StaticServiceProvider.Instance.GetRequiredService<IPartialViewBlockEngine>(),
            StaticServiceProvider.Instance.GetRequiredService<BlockEditorConverter>(),
            StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiElementBuilder>(),
            StaticServiceProvider.Instance.GetRequiredService<RichTextBlockPropertyValueConstructorCache>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogger<RteMacroRenderingValueConverter>>(),
            deliveryApiSettingsMonitor
        )
    {
    }

    public RteMacroRenderingValueConverter(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer,
        HtmlLocalLinkParser linkParser, HtmlUrlParser urlParser, HtmlImageSourceParser imageSourceParser,
        IApiRichTextElementParser apiRichTextElementParser, IApiRichTextMarkupParser apiRichTextMarkupParser,
        IPartialViewBlockEngine partialViewBlockEngine, BlockEditorConverter blockEditorConverter, IJsonSerializer jsonSerializer,
        IApiElementBuilder apiElementBuilder, RichTextBlockPropertyValueConstructorCache constructorCache, ILogger<RteMacroRenderingValueConverter> logger,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettingsMonitor)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _macroRenderer = macroRenderer;
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
        _deliveryApiSettings = deliveryApiSettingsMonitor.CurrentValue;
        deliveryApiSettingsMonitor.OnChange(settings => _deliveryApiSettings = settings);
    }

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>

        // because that version of RTE converter parses {locallink} and executes macros, its value has
        // to be cached at the published snapshot level, because we have no idea what the macros may depend on actually.
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
            ? ParseRichTextBlockModel(richTextEditorValue.Blocks, propertyType, PropertyCacheLevel.Element, preview)
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
        if (inter is not RichTextEditorIntermediateValue richTextEditorIntermediateValue
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

    // NOT thread-safe over a request because it modifies the
    // global UmbracoContext.Current.InPreviewMode status. So it
    // should never execute in // over the same UmbracoContext with
    // different preview modes.
    private string RenderRteMacros(string source, bool preview)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        using (umbracoContext.ForcedPreview(preview)) // force for macro rendering
        {
            var sb = new StringBuilder();

            MacroTagParser.ParseMacros(
                source,

                // callback for when text block is found
                textBlock => sb.Append(textBlock),

                // callback for when macro syntax is found
                (macroAlias, macroAttributes) => sb.Append(_macroRenderer.RenderAsync(
                    macroAlias,
                    umbracoContext.PublishedRequest?.PublishedContent,

                    // needs to be explicitly casted to Dictionary<string, object>
                    macroAttributes.ConvertTo(x => (string)x, x => x)!).GetAwaiter().GetResult().Text));

            return sb.ToString();
        }
    }

    private string? Convert(object? source, bool preview)
    {
        if (source is not RichTextEditorIntermediateValue intermediateValue)
        {
            return null;
        }

        var sourceString = intermediateValue.Markup;

        // ensures string is parsed for {localLink} and URLs and media are resolved correctly
        sourceString = _linkParser.EnsureInternalLinks(sourceString, preview);
        sourceString = _urlParser.EnsureUrls(sourceString);
        sourceString = _imageSourceParser.EnsureImageSources(sourceString);

        // ensure string is parsed for macros and macros are executed correctly
        sourceString = RenderRteMacros(sourceString, preview);

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

    private RichTextBlockModel? ParseRichTextBlockModel(BlockValue blocks, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, bool preview)
    {
        RichTextConfiguration? configuration = propertyType.DataType.ConfigurationAs<RichTextConfiguration>();
        if (configuration?.Blocks?.Any() is not true)
        {
            return null;
        }

        var creator = new RichTextBlockPropertyValueCreator(_blockEditorConverter, _constructorCache);
        return creator.CreateBlockModel(referenceCacheLevel, blocks, preview, configuration.Blocks);
    }

    private string RenderRichTextBlockModel(string source, RichTextBlockModel? richTextBlockModel)
    {
        if (richTextBlockModel is null || richTextBlockModel.Any() is false)
        {
            return source;
        }

        var blocksByUdi = richTextBlockModel.ToDictionary(block => block.ContentUdi);

        string RenderBlock(Match match) =>
            UdiParser.TryParse(match.Groups["udi"].Value, out Udi? udi) && blocksByUdi.TryGetValue(udi, out RichTextBlockItem? richTextBlockItem)
                ? _partialViewBlockEngine.ExecuteAsync(richTextBlockItem).GetAwaiter().GetResult()
                : string.Empty;

        return RichTextParsingRegexes.BlockRegex().Replace(source, RenderBlock);
    }

    private RichTextModel CreateRichTextModel(RichTextEditorIntermediateValue richTextEditorIntermediateValue)
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

    private class RichTextEditorIntermediateValue
    {
        public required string Markup { get; set; }

        public required RichTextBlockModel? RichTextBlockModel { get; set; }
    }
}
