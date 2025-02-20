using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiRichTextMarkupParser : ApiRichTextParserBase, IApiRichTextMarkupParser
{
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IPublishedMediaCache _publishedMediaCache;
    private readonly ILogger<ApiRichTextMarkupParser> _logger;

    public ApiRichTextMarkupParser(
        IApiContentRouteBuilder apiContentRouteBuilder,
        IApiMediaUrlProvider mediaUrlProvider,
        IPublishedContentCache publishedContentCache,
        IPublishedMediaCache publishedMediaCache,
        ILogger<ApiRichTextMarkupParser> logger)
        : base(apiContentRouteBuilder, mediaUrlProvider)
    {
        _publishedContentCache = publishedContentCache;
        _publishedMediaCache = publishedMediaCache;
        _logger = logger;
    }

    public string Parse(string html)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            ReplaceLocalLinks(doc, _publishedContentCache, _publishedMediaCache);

            ReplaceLocalImages(doc, _publishedMediaCache);

            CleanUpBlocks(doc);

            return doc.DocumentNode.InnerHtml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not parse rich text HTML, see exception for details");
            return html;
        }
    }

    private void ReplaceLocalLinks(HtmlDocument doc, IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
    {
        HtmlNode[] links = doc.DocumentNode.SelectNodes("//a")?.ToArray() ?? Array.Empty<HtmlNode>();
        foreach (HtmlNode link in links)
        {
            ReplaceLocalLinks(
                    contentCache,
                    mediaCache,
                link.GetAttributeValue("href", string.Empty),
                link.GetAttributeValue("type", "unknown"),
                route =>
                {
                    link.SetAttributeValue("href", route.Path);
                    link.SetAttributeValue("data-start-item-path", route.StartItem.Path);
                    link.SetAttributeValue("data-start-item-id", route.StartItem.Id.ToString("D"));
                    link.Attributes["type"]?.Remove();
                },
                url =>
                {
                    link.SetAttributeValue("href", url);
                    link.Attributes["type"]?.Remove();
                },
                () =>
                {
                    link.Attributes.Remove("href");
                    link.Attributes["type"]?.Remove();
                });
        }
    }

    private void ReplaceLocalImages(HtmlDocument doc, IPublishedMediaCache mediaCache)
    {
        HtmlNode[] images = doc.DocumentNode.SelectNodes("//img")?.ToArray() ?? Array.Empty<HtmlNode>();
        foreach (HtmlNode image in images)
        {
            var dataUdi = image.GetAttributeValue("data-udi", string.Empty);
            if (dataUdi.IsNullOrWhiteSpace())
            {
                continue;
            }

            ReplaceLocalImages(mediaCache, dataUdi, mediaUrl =>
            {
                // the image source likely contains query string parameters for image cropping; we need to
                // preserve those, so let's extract the image query string (if present).
                var currentImageSource = image.GetAttributeValue("src", string.Empty);
                var currentImageQueryString = currentImageSource.Contains('?')
                    ? $"?{currentImageSource.Split('?').Last()}"
                    : null;

                image.SetAttributeValue("src", $"{mediaUrl}{currentImageQueryString}");
                image.Attributes.Remove("data-udi");

                // we don't want the "data-caption" attribute, it's already part of the output as <figcaption>
                image.Attributes.Remove("data-caption");
            });
        }
    }

    private void CleanUpBlocks(HtmlDocument doc)
    {
        HtmlNode[] blocks = doc.DocumentNode.SelectNodes("//*[starts-with(local-name(),'umb-rte-block')]")?.ToArray() ?? Array.Empty<HtmlNode>();
        foreach (HtmlNode block in blocks)
        {
            var dataKey = block.GetAttributeValue(BlockContentKeyAttribute, string.Empty);
            if (Guid.TryParse(dataKey, out Guid key) is false)
            {
                continue;
            }

            // swap the content UDI for the content ID
            block.Attributes.Remove(BlockContentKeyAttribute);
            block.SetAttributeValue("data-content-id", key.ToString("D"));

            // remove the inner comment placed by the RTE
            block.RemoveAllChildren();
        }
    }
}
