using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiRichTextElementParser : ApiRichTextParserBase, IApiRichTextElementParser
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly ILogger<ApiRichTextElementParser> _logger;

    private const string TextNodeName = "#text";
    private const string CommentNodeName = "#comment";

    [Obsolete($"Please use the constructor that accepts {nameof(IApiElementBuilder)}. Will be removed in V15.")]
    public ApiRichTextElementParser(
        IApiContentRouteBuilder apiContentRouteBuilder,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        ILogger<ApiRichTextElementParser> logger)
        : this(
            apiContentRouteBuilder,
            publishedUrlProvider,
            publishedSnapshotAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IApiElementBuilder>(),
            logger)
    {
    }

    public ApiRichTextElementParser(
        IApiContentRouteBuilder apiContentRouteBuilder,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiElementBuilder apiElementBuilder,
        ILogger<ApiRichTextElementParser> logger)
        : base(apiContentRouteBuilder, publishedUrlProvider)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _apiElementBuilder = apiElementBuilder;
        _logger = logger;
    }

    [Obsolete($"Please use the overload that accepts {nameof(RichTextBlockModel)}. Will be removed in V15.")]
    public IRichTextElement? Parse(string html) => Parse(html, null);

    public IRichTextElement? Parse(string html, RichTextBlockModel? richTextBlockModel)
    {
        try
        {
            IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return ParseRootElement(doc.DocumentNode, publishedSnapshot, richTextBlockModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not parse rich text HTML, see exception for details");
            return null;
        }
    }

    private IRichTextElement ParseRecursively(HtmlNode current, IPublishedSnapshot publishedSnapshot)
        => current.Name == TextNodeName
                ? ParseTextElement(current)
                : ParseGenericElement(current, publishedSnapshot);

    private RichTextTextElement ParseTextElement(HtmlNode element)
    {
        if (element.Name != TextNodeName)
        {
            throw new ArgumentException($"Only {TextNodeName} elements are supported, got: {element.Name}");
        }

        return new RichTextTextElement(element.InnerText);
    }

    private RichTextRootElement ParseRootElement(HtmlNode element, IPublishedSnapshot publishedSnapshot, RichTextBlockModel? richTextBlockModel)
    {
        ApiBlockItem[] blocks = richTextBlockModel is not null
            ? richTextBlockModel.Select(item => item.CreateApiBlockItem(_apiElementBuilder)).ToArray()
            : Array.Empty<ApiBlockItem>();

        return ParseElement(
            element,
            publishedSnapshot,
            (_, attributes, childElements) => new RichTextRootElement(attributes, childElements, blocks));
    }

    private RichTextGenericElement ParseGenericElement(HtmlNode element, IPublishedSnapshot publishedSnapshot)
    {
        if (element.Name == TextNodeName)
        {
            throw new ArgumentException($"{TextNodeName} elements should be handled by {nameof(ParseTextElement)}");
        }

        return ParseElement(
            element,
            publishedSnapshot,
            (tag, attributes, childElements) => new RichTextGenericElement(tag, attributes, childElements));
    }

    private T ParseElement<T>(HtmlNode element, IPublishedSnapshot publishedSnapshot, Func<string, Dictionary<string, object>, IRichTextElement[], T> createElement)
        where T : IRichTextElement
    {
        // grab all valid node children:
        // - non-#comment nodes
        // - non-#text nodes
        // - non-empty #text nodes
        HtmlNode[] childNodes = element.ChildNodes
            .Where(c => c.Name != CommentNodeName && (c.Name != TextNodeName || string.IsNullOrWhiteSpace(c.InnerText) is false))
            .ToArray();

        var tag = TagName(element);
        var attributes = element.Attributes.ToDictionary(a => a.Name, a => a.Value as object);

        ReplaceLocalLinks(publishedSnapshot, attributes);

        ReplaceLocalImages(publishedSnapshot, tag, attributes);

        CleanUpBlocks(tag, attributes);

        SanitizeAttributes(attributes);

        IRichTextElement[] childElements = childNodes.Any()
            ? childNodes.Select(child => ParseRecursively(child, publishedSnapshot)).ToArray()
            : Array.Empty<IRichTextElement>();

        return createElement(tag, attributes, childElements);
    }

    private string TagName(HtmlNode htmlNode) => htmlNode.Name;

    private void ReplaceLocalLinks(IPublishedSnapshot publishedSnapshot, Dictionary<string, object> attributes)
    {
        if (attributes.ContainsKey("href") is false || attributes["href"] is not string href)
        {
            return;
        }

        ReplaceLocalLinks(
            publishedSnapshot,
            href,
            route =>
            {
                attributes["route"] = route;
                attributes.Remove("href");
            },
            url => attributes["href"] = url,
            () => attributes.Remove("href"));
    }

    private void ReplaceLocalImages(IPublishedSnapshot publishedSnapshot, string tag, Dictionary<string, object> attributes)
    {
        if (tag is not "img" || attributes.ContainsKey("data-udi") is false || attributes["data-udi"] is not string dataUdi)
        {
            return;
        }

        ReplaceLocalImages(publishedSnapshot, dataUdi, mediaUrl =>
        {
            attributes["src"] = mediaUrl;
            attributes.Remove("data-udi");
        });
    }

    private void CleanUpBlocks(string tag, Dictionary<string, object> attributes)
    {
        if (tag.StartsWith("umb-rte-block") is false || attributes.ContainsKey("data-content-udi") is false || attributes["data-content-udi"] is not string dataUdi)
        {
            return;
        }

        if (UdiParser.TryParse<GuidUdi>(dataUdi, out GuidUdi? guidUdi) is false)
        {
            return;
        }

        attributes["content-id"] = guidUdi.Guid;
        attributes.Remove("data-content-udi");
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
