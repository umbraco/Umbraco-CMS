using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed partial class ApiRichTextParser : IApiRichTextParser
{
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly ILogger<ApiRichTextParser> _logger;

    private const string TextNodeName = "#text";

    public ApiRichTextParser(
        IApiContentRouteBuilder apiContentRouteBuilder,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedUrlProvider publishedUrlProvider,
        ILogger<ApiRichTextParser> logger)
    {
        _apiContentRouteBuilder = apiContentRouteBuilder;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _publishedUrlProvider = publishedUrlProvider;
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

        Match match = LocalLinkRegex().Match(href);
        if (match.Success is false)
        {
            return;
        }

        attributes.Remove("href");

        if (UdiParser.TryParse(match.Groups["udi"].Value, out Udi? udi) is false)
        {
            return;
        }

        switch (udi.EntityType)
        {
            case Constants.UdiEntityType.Document:
                IPublishedContent? content = publishedSnapshot.Content?.GetById(udi);
                IApiContentRoute? route = content != null
                    ? _apiContentRouteBuilder.Build(content)
                    : null;
                if (route != null)
                {
                    attributes["route"] = route;
                }

                break;
            case Constants.UdiEntityType.Media:
                IPublishedContent? media = publishedSnapshot.Media?.GetById(udi);
                if (media != null)
                {
                    attributes["href"] = _publishedUrlProvider.GetMediaUrl(media, UrlMode.Absolute);
                }

                break;
        }
    }

    private void ReplaceLocalImages(IPublishedSnapshot publishedSnapshot, string tag, Dictionary<string, object> attributes)
    {
        if (tag is not "img" || attributes.ContainsKey("data-udi") is false)
        {
            return;
        }

        var dataUdiValue = attributes["data-udi"];
        attributes.Remove("data-udi");

        if (dataUdiValue is not string dataUdi || UdiParser.TryParse(dataUdi, out Udi? udi) is false)
        {
            return;
        }

        IPublishedContent? media = publishedSnapshot.Media?.GetById(udi);
        if (media is not null)
        {
            attributes["src"] = _publishedUrlProvider.GetMediaUrl(media, UrlMode.Absolute);

            // this may be relevant if we can't find width and height in the attributes ... for now we seem quite able to, though
            // if (currentSrc != null)
            // {
            //     NameValueCollection queryString = HttpUtility.ParseQueryString(HttpUtility.HtmlDecode(currentSrc));
            //     attributes["params"] = queryString.AllKeys.WhereNotNull().ToDictionary(key => key, key => queryString[key]);
            // }
        }
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

    [GeneratedRegex("{localLink:(?<udi>umb:.+)}")]
    private static partial Regex LocalLinkRegex();
}
