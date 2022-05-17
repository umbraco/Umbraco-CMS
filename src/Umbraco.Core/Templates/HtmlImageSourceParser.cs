using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Templates;

public sealed class HtmlImageSourceParser
{
    private static readonly Regex ResolveImgPattern = new(
        @"(<img[^>]*src="")([^""\?]*)((?:\?[^""]*)?""[^>]*data-udi="")([^""]*)(""[^>]*>)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex DataUdiAttributeRegex = new(
        @"data-udi=\\?(?:""|')(?<udi>umb://[A-z0-9\-]+/[A-z0-9]+)\\?(?:""|')",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly IPublishedUrlProvider? _publishedUrlProvider;

    private Func<Guid, string?>? _getMediaUrl;

    public HtmlImageSourceParser(Func<Guid, string> getMediaUrl) => _getMediaUrl = getMediaUrl;

    public HtmlImageSourceParser(IPublishedUrlProvider publishedUrlProvider) =>
        _publishedUrlProvider = publishedUrlProvider;

    /// <summary>
    ///     Parses out media UDIs from an html string based on 'data-udi' html attributes
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IEnumerable<Udi> FindUdisFromDataAttributes(string text)
    {
        MatchCollection matches = DataUdiAttributeRegex.Matches(text);
        if (matches.Count == 0)
        {
            yield break;
        }

        foreach (Match match in matches)
        {
            if (match.Groups.Count == 2 && UdiParser.TryParse(match.Groups[1].Value, out Udi? udi))
            {
                yield return udi;
            }
        }
    }

    /// <summary>
    ///     Parses the string looking for Umbraco image tags and updates them to their up-to-date image sources.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <remarks>Umbraco image tags are identified by their data-udi attributes</remarks>
    public string EnsureImageSources(string text)
    {
        if (_getMediaUrl == null)
        {
            _getMediaUrl = guid => _publishedUrlProvider?.GetMediaUrl(guid);
        }

        return ResolveImgPattern.Replace(text, match =>
        {
            // match groups:
            // - 1 = from the beginning of the image tag until src attribute value begins
            // - 2 = the src attribute value excluding the querystring (if present)
            // - 3 = anything after group 2 and before the data-udi attribute value begins
            // - 4 = the data-udi attribute value
            // - 5 = anything after group 4 until the image tag is closed
            var udi = match.Groups[4].Value;
            if (udi.IsNullOrWhiteSpace() || UdiParser.TryParse<GuidUdi>(udi, out GuidUdi? guidUdi) == false)
            {
                return match.Value;
            }

            var mediaUrl = _getMediaUrl(guidUdi.Guid);
            if (mediaUrl == null)
            {
                // image does not exist - we could choose to remove the image entirely here (return empty string),
                // but that would leave the editors completely in the dark as to why the image doesn't show
                return match.Value;
            }

            return $"{match.Groups[1].Value}{mediaUrl}{match.Groups[3].Value}{udi}{match.Groups[5].Value}";
        });
    }

    /// <summary>
    ///     Removes media URLs from &lt;img&gt; tags where a data-udi attribute is present
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string RemoveImageSources(string text)

        // see comment in ResolveMediaFromTextString for group reference
        => ResolveImgPattern.Replace(text, "$1$3$4$5");
}
