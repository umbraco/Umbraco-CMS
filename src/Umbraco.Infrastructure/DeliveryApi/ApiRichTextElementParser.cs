using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiRichTextElementParser : ApiRichTextParserBase, IApiRichTextElementParser
{
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IPublishedMediaCache _publishedMediaCache;
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly ILogger<ApiRichTextElementParser> _logger;

    private const string TextNodeName = "#text";
    private const string CommentNodeName = "#comment";

    public ApiRichTextElementParser(
        IApiContentRouteBuilder apiContentRouteBuilder,
        IApiMediaUrlProvider mediaUrlProvider,
        IPublishedContentCache publishedContentCache,
        IPublishedMediaCache publishedMediaCache,
        IApiElementBuilder apiElementBuilder,
        ILogger<ApiRichTextElementParser> logger)
        : base(apiContentRouteBuilder, mediaUrlProvider)
    {
        _publishedContentCache = publishedContentCache;
        _publishedMediaCache = publishedMediaCache;
        _apiElementBuilder = apiElementBuilder;
        _logger = logger;
    }

    [Obsolete($"Please use the overload that accepts {nameof(RichTextBlockModel)}. Will be removed in V15.")]
    public IRichTextElement? Parse(string html) => Parse(html, null);

    public IRichTextElement? Parse(string html, RichTextBlockModel? richTextBlockModel)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return ParseRootElement(doc.DocumentNode, _publishedContentCache, _publishedMediaCache, richTextBlockModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not parse rich text HTML, see exception for details");
            return null;
        }
    }

    private IRichTextElement ParseRecursively(HtmlNode current, IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
        => current.Name == TextNodeName
                ? ParseTextElement(current)
                : ParseGenericElement(current, contentCache, mediaCache);

    private RichTextTextElement ParseTextElement(HtmlNode element)
    {
        if (element.Name != TextNodeName)
        {
            throw new ArgumentException($"Only {TextNodeName} elements are supported, got: {element.Name}");
        }

        return new RichTextTextElement(element.InnerText);
    }

    private RichTextRootElement ParseRootElement(HtmlNode element, IPublishedContentCache contentCache, IPublishedMediaCache mediaCache, RichTextBlockModel? richTextBlockModel)
    {
        ApiBlockItem[] blocks = richTextBlockModel is not null
            ? richTextBlockModel.Select(item => item.CreateApiBlockItem(_apiElementBuilder)).ToArray()
            : Array.Empty<ApiBlockItem>();

        return ParseElement(
            element,
            contentCache,
            mediaCache,
            (_, attributes, childElements) => new RichTextRootElement(attributes, childElements, blocks));
    }

    private RichTextGenericElement ParseGenericElement(HtmlNode element, IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
    {
        if (element.Name == TextNodeName)
        {
            throw new ArgumentException($"{TextNodeName} elements should be handled by {nameof(ParseTextElement)}");
        }

        return ParseElement(
            element,
            contentCache,
            mediaCache,
            (tag, attributes, childElements) => new RichTextGenericElement(tag, attributes, childElements));
    }

    private T ParseElement<T>(HtmlNode element, IPublishedContentCache contentCache, IPublishedMediaCache mediaCache, Func<string, Dictionary<string, object>, IRichTextElement[], T> createElement)
        where T : IRichTextElement
    {
        // grab all valid node children:
        // - non-#comment nodes
        // - non-#text nodes
        // - non-empty #text nodes
        // - empty #text between inline elements (see #17037)
        HtmlNode[] childNodes = element.ChildNodes
            .Where(c => c.Name != CommentNodeName && (c.Name != TextNodeName || c.NextSibling is not null || string.IsNullOrWhiteSpace(c.InnerText) is false))
            .ToArray();

        var tag = TagName(element);
        var attributes = element.Attributes.ToDictionary(a => a.Name, a => a.Value as object);

        ReplaceLocalLinks(contentCache, mediaCache, attributes);

        ReplaceLocalImages(mediaCache, tag, attributes);

        CleanUpBlocks(tag, attributes);

        SanitizeAttributes(attributes);

        IRichTextElement[] childElements = childNodes.Any()
            ? childNodes.Select(child => ParseRecursively(child, contentCache, mediaCache)).ToArray()
            : Array.Empty<IRichTextElement>();

        return createElement(tag, attributes, childElements);
    }

    private static string TagName(HtmlNode htmlNode) => htmlNode.Name;

    private void ReplaceLocalLinks(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache, Dictionary<string, object> attributes)
    {
        if (attributes.TryGetValue("href", out object? hrefAttribute) is false || hrefAttribute is not string href)
        {
            return;
        }

        if (attributes.TryGetValue("type", out object? typeAttribute) is false || typeAttribute is not string type)
        {
            type = "unknown";
        }

        ReplaceLocalLinks(
            contentCache,
            mediaCache,
            href,
            type,
            route =>
            {
                attributes["route"] = route;
                attributes.Remove("href");
            },
            url => attributes["href"] = url,
            () => attributes.Remove("href"));
    }

    private void ReplaceLocalImages(IPublishedMediaCache mediaCache, string tag, Dictionary<string, object> attributes)
    {
        if (tag is not "img" || attributes.TryGetValue("data-udi", out object? dataUdiAttribute) is false || dataUdiAttribute is not string dataUdi)
        {
            return;
        }

        ReplaceLocalImages(mediaCache, dataUdi, mediaUrl =>
        {
            attributes["src"] = mediaUrl;
            attributes.Remove("data-udi");
        });
    }

    private static void CleanUpBlocks(string tag, Dictionary<string, object> attributes)
    {
        if (tag.StartsWith("umb-rte-block") is false || attributes.TryGetValue(BlockContentKeyAttribute, out object? blockContentKeyAttribute) is false || blockContentKeyAttribute is not string dataKey)
        {
            return;
        }

        if (Guid.TryParse(dataKey, out Guid key) is false)
        {
            return;
        }

        attributes["content-id"] = key;
        attributes.Remove(BlockContentKeyAttribute);
    }

    private static void SanitizeAttributes(Dictionary<string, object> attributes)
    {
        KeyValuePair<string, object>[] dataAttributes = attributes
            .Where(kvp => kvp.Key.StartsWith("data-"))
            .ToArray();

        foreach (KeyValuePair<string, object> dataAttribute in dataAttributes)
        {
            var actualKey = dataAttribute.Key.TrimStart("data-");
            attributes.TryAdd(actualKey, dataAttribute.Value);

            attributes.Remove(dataAttribute.Key);
        }
    }
}
