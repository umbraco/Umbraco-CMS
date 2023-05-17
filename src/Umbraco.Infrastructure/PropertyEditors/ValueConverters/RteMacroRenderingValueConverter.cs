// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Infrastructure.Macros;
using Umbraco.Cms.Core.Models.DeliveryApi;
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
    private readonly IApiRichTextParser _apiRichTextParser;
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
            StaticServiceProvider.Instance.GetRequiredService<IApiRichTextParser>(),
            StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<DeliveryApiSettings>>())
    {
    }

    public RteMacroRenderingValueConverter(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer,
        HtmlLocalLinkParser linkParser, HtmlUrlParser urlParser, HtmlImageSourceParser imageSourceParser, IApiRichTextParser apiRichTextParser, IOptionsMonitor<DeliveryApiSettings> deliveryApiSettingsMonitor)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _macroRenderer = macroRenderer;
        _linkParser = linkParser;
        _urlParser = urlParser;
        _imageSourceParser = imageSourceParser;
        _apiRichTextParser = apiRichTextParser;
        _deliveryApiSettings = deliveryApiSettingsMonitor.CurrentValue;
        deliveryApiSettingsMonitor.OnChange(settings => _deliveryApiSettings = settings);
    }

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>

        // because that version of RTE converter parses {locallink} and executes macros, its value has
        // to be cached at the published snapshot level, because we have no idea what the macros may depend on actually.
        PropertyCacheLevel.Snapshot;

    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var converted = Convert(inter, preview);

        return new HtmlEncodedString(converted ?? string.Empty);
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => _deliveryApiSettings.RichTextOutputAsJson
            ? typeof(IRichTextElement)
            : typeof(RichTextModel);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (_deliveryApiSettings.RichTextOutputAsJson is false)
        {
            return new RichTextModel
            {
                Markup = Convert(inter, preview, false) ?? string.Empty
            };
        }

        var sourceString = inter?.ToString();
        return sourceString != null
            ? _apiRichTextParser.Parse(sourceString)
            : null;
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

    private string? Convert(object? source, bool preview, bool handleMacros = true)
    {
        if (source == null)
        {
            return null;
        }

        var sourceString = source.ToString()!;

        // ensures string is parsed for {localLink} and URLs and media are resolved correctly
        sourceString = _linkParser.EnsureInternalLinks(sourceString, preview);
        sourceString = _urlParser.EnsureUrls(sourceString);
        sourceString = _imageSourceParser.EnsureImageSources(sourceString);

        // ensure string is parsed for macros and macros are executed correctly
        if (handleMacros)
        {
            sourceString = RenderRteMacros(sourceString, preview);
        }

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
}
