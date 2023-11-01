using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal sealed class ApiRichTextMarkupParser : ApiRichTextParserBase, IApiRichTextMarkupParser
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly ILogger<ApiRichTextMarkupParser> _logger;

    public ApiRichTextMarkupParser(
        IApiContentRouteBuilder apiContentRouteBuilder,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        ILogger<ApiRichTextMarkupParser> logger)
        : base(apiContentRouteBuilder, publishedUrlProvider)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _logger = logger;
    }

    public string Parse(string html)
    {
        try
        {
            IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            ReplaceLocalLinks(doc, publishedSnapshot);

            ReplaceLocalImages(doc, publishedSnapshot);

            CleanUpBlocks(doc);

            return doc.DocumentNode.InnerHtml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not parse rich text HTML, see exception for details");
            return html;
        }
    }

    private void ReplaceLocalLinks(HtmlDocument doc, IPublishedSnapshot publishedSnapshot)
    {
        HtmlNode[] links = doc.DocumentNode.SelectNodes("//a")?.ToArray() ?? Array.Empty<HtmlNode>();
        foreach (HtmlNode link in links)
        {
            ReplaceLocalLinks(
                publishedSnapshot,
                link.GetAttributeValue("href", string.Empty),
                route =>
                {
                    link.SetAttributeValue("href", route.Path);
                    link.SetAttributeValue("data-start-item-path", route.StartItem.Path);
                    link.SetAttributeValue("data-start-item-id", route.StartItem.Id.ToString("D"));
                },
                url => link.SetAttributeValue("href", url),
                () => link.Attributes.Remove("href"));
        }
    }

    private void ReplaceLocalImages(HtmlDocument doc, IPublishedSnapshot publishedSnapshot)
    {
        HtmlNode[] images = doc.DocumentNode.SelectNodes("//img")?.ToArray() ?? Array.Empty<HtmlNode>();
        foreach (HtmlNode image in images)
        {
            var dataUdi = image.GetAttributeValue("data-udi", string.Empty);
            if (dataUdi.IsNullOrWhiteSpace())
            {
                continue;
            }

            ReplaceLocalImages(publishedSnapshot, dataUdi, mediaUrl =>
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
            var dataUdi = block.GetAttributeValue("data-content-udi", string.Empty);
            if (UdiParser.TryParse<GuidUdi>(dataUdi, out GuidUdi? guidUdi) is false)
            {
                continue;
            }

            // swap the content UDI for the content ID
            block.Attributes.Remove("data-content-udi");
            block.SetAttributeValue("data-content-id", guidUdi.Guid.ToString("D"));

            // remove the inner comment placed by the RTE
            block.RemoveAllChildren();
        }
    }
}
