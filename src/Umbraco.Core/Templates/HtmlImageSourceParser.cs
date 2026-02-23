using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Utility class used to parse and update image sources in HTML content based on Umbraco media references.
/// </summary>
public sealed class HtmlImageSourceParser
{
    private static readonly Regex ResolveImgPattern = new(
        @"<img[^>]*(data-udi=""([^""]*)"")[^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex SrcAttributeRegex = new(
        @"src=""([^""\?]*)(\?[^""]*)?""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex DataUdiAttributeRegex = new(
        @"data-udi=\\?(?:""|')(?<udi>umb://[A-z0-9\-]+/[A-z0-9]+)\\?(?:""|')",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly IPublishedUrlProvider? _publishedUrlProvider;

    private Func<Guid, string?>? _getMediaUrl;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HtmlImageSourceParser"/> class.
    /// </summary>
    /// <param name="getMediaUrl">A function that retrieves the media URL for a given GUID.</param>
    public HtmlImageSourceParser(Func<Guid, string> getMediaUrl) => _getMediaUrl = getMediaUrl;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HtmlImageSourceParser"/> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider for resolving media URLs.</param>
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
            // - 1 = the data-udi attribute
            // - 2 = the data-udi attribute value
            var udi = match.Groups[2].Value;
            if (udi.IsNullOrWhiteSpace() || UdiParser.TryParse<GuidUdi>(udi, out GuidUdi? guidUdi) == false)
            {
                return match.Value;
            }

            // Find the src attribute
            // src match groups:
            // - 1 = the src attribute value until the query string
            // - 2 = the src attribute query string including the '?'
            Match src = SrcAttributeRegex.Match(match.Value);

            if (src.Success == false)
            {
                // the src attribute isn't found, return the original value
                return match.Value;
            }

            var mediaUrl = _getMediaUrl(guidUdi.Guid);
            if (mediaUrl == null)
            {
                // image does not exist - we could choose to remove the image entirely here (return empty string),
                // but that would leave the editors completely in the dark as to why the image doesn't show
                return match.Value;
            }

            var newImgTag = match.Value.Replace(src.Value, $"src=\"{mediaUrl}{src.Groups[2].Value}\"");

            return newImgTag;
        });
    }

    /// <summary>
    ///     Removes media URLs from &lt;img&gt; tags where a data-udi attribute is present
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string RemoveImageSources(string text)

        // find each ResolveImgPattern match in the text, then find each
        // SrcAttributeRegex match in the match value, then replace the src
        // attribute value with an empty string
        // (see comment in ResolveMediaFromTextString for group reference)
        => ResolveImgPattern.Replace(text, match =>
        {
            // Find the src attribute
            Match src = SrcAttributeRegex.Match(match.Value);

            return src.Success == false || string.IsNullOrWhiteSpace(src.Groups[1].Value) ?
                match.Value : match.Value.Replace(src.Groups[1].Value, string.Empty);
        });
}
