using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiRichTextElementParser : ApiRichTextParserBase, IApiRichTextElementParser
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly ILogger<ApiRichTextElementParser> _logger;

    private const string TextNodeName = "#text";

    public ApiRichTextElementParser(
        IApiContentRouteBuilder apiContentRouteBuilder,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        ILogger<ApiRichTextElementParser> logger)
        : base(apiContentRouteBuilder, publishedUrlProvider)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _logger = logger;
    }

    public IRichTextElement? Parse(string html)
    {
        try
        {
            IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return ParseRecursively(doc.DocumentNode, publishedSnapshot);
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
            : ParseElement(current, publishedSnapshot);

    private RichTextTextElement ParseTextElement(HtmlNode element)
    {
        if (element.Name != TextNodeName)
        {
            throw new ArgumentException($"Only {TextNodeName} elements are supported, got: {element.Name}");
        }

        return new RichTextTextElement(element.InnerText);
    }

    private RichTextGenericElement ParseElement(HtmlNode element, IPublishedSnapshot publishedSnapshot)
    {
        if (element.Name == TextNodeName)
        {
            throw new ArgumentException($"{TextNodeName} elements should be handled by {nameof(ParseTextElement)}");
        }

        // grab all non-#text nodes + all non-empty #text nodes as valid node children
        HtmlNode[] childNodes = element.ChildNodes
            .Where(c => c.Name != TextNodeName || string.IsNullOrWhiteSpace(c.InnerText) is false)
            .ToArray();

        var tag = TagName(element);
        var attributes = element.Attributes.ToDictionary(a => a.Name, a => a.Value as object);

        ReplaceLocalLinks(publishedSnapshot, attributes);

        ReplaceLocalImages(publishedSnapshot, tag, attributes);

        SanitizeAttributes(attributes);

        IRichTextElement[] childElements = childNodes.Any()
            ? childNodes.Select(child => ParseRecursively(child, publishedSnapshot)).ToArray()
            : Array.Empty<IRichTextElement>();

        return new RichTextGenericElement(tag, attributes, childElements);
    }

    private string TagName(HtmlNode htmlNode) => htmlNode.Name == "#document" ? "#root" : htmlNode.Name;

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
