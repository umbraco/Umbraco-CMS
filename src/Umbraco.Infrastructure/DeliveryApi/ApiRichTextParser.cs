using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed partial class ApiRichTextParser : IApiRichTextParser
{
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly ILogger<ApiRichTextParser> _logger;

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

    public RichTextElement? Parse(string html)
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

    private RichTextElement ParseRecursively(HtmlNode current, IPublishedSnapshot publishedSnapshot)
    {
        // if a HtmlNode contains only #text elements, the entire node contents will be contained
        // within the innerText declaration later in this method; otherwise accept all non-#text
        // nodes + all non-empty #text nodes as valid node children
        HtmlNode[]? childNodes = current.ChildNodes.All(c => c.Name == "#text")
            ? null
            : current.ChildNodes
                .Where(c => c.Name != "#text" || string.IsNullOrWhiteSpace(c.InnerText) is false)
                .ToArray();

        // the resulting element can only have an inner text value if the node has no (valid) children
        var innerText = childNodes is null ? current.InnerText : string.Empty;

        var tag = TagName(current);
        var attributes = current.Attributes.ToDictionary(a => a.Name, a => a.Value as object);

        ReplaceLocalLinks(publishedSnapshot, attributes);

        ReplaceLocalImages(publishedSnapshot, tag, attributes);

        SanitizeAttributes(attributes);

        RichTextElement[] childElements = childNodes?.Any() is true
            ? childNodes.Select(child => ParseRecursively(child, publishedSnapshot)).ToArray()
            : Array.Empty<RichTextElement>();

        return new RichTextElement(tag, innerText, attributes, childElements);
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
                if (content != null)
                {
                    attributes["route"] = _apiContentRouteBuilder.Build(content);
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
            if (attributes.ContainsKey(actualKey) is false)
            {
                attributes[actualKey] = dataAttribute.Value;
            }

            attributes.Remove(dataAttribute.Key);
        }
    }

    [GeneratedRegex("{localLink:(?<udi>umb:.+)}")]
    private static partial Regex LocalLinkRegex();
}
