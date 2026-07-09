using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Utility class used to parse and update image sources in HTML content based on Umbraco media references.
/// </summary>
public sealed partial class HtmlImageSourceParser
{
    private static readonly Regex _resolveImgRegex = ResolveImgRegex();

    private static readonly Regex _srcAttributeRegex = SrcAttributeRegex();

    private static readonly Regex _dataUdiAttributeRegex = DataUdiAttributeRegex();

    private readonly IPublishedUrlProvider? _publishedUrlProvider;
    private readonly IImageUrlTokenGenerator _imageUrlTokenGenerator;

    private Func<Guid, string?>? _getMediaUrl;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HtmlImageSourceParser"/> class.
    /// </summary>
    /// <param name="getMediaUrl">A function that retrieves the media URL for a given GUID.</param>
    /// <param name="imageUrlTokenGenerator">Used to re-sign rendered image URLs against the current HMAC secret key.</param>
    public HtmlImageSourceParser(Func<Guid, string> getMediaUrl, IImageUrlTokenGenerator imageUrlTokenGenerator)
    {
        _getMediaUrl = getMediaUrl;
        _imageUrlTokenGenerator = imageUrlTokenGenerator;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HtmlImageSourceParser"/> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider for resolving media URLs.</param>
    /// <param name="imageUrlTokenGenerator">Used to re-sign rendered image URLs against the current HMAC secret key.</param>
    public HtmlImageSourceParser(IPublishedUrlProvider publishedUrlProvider, IImageUrlTokenGenerator imageUrlTokenGenerator)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _imageUrlTokenGenerator = imageUrlTokenGenerator;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HtmlImageSourceParser"/> class.
    /// </summary>
    /// <param name="getMediaUrl">A function that retrieves the media URL for a given GUID.</param>
    [Obsolete("Please use the constructor that accepts IImageUrlTokenGenerator. Scheduled for removal in Umbraco 19.")]
    public HtmlImageSourceParser(Func<Guid, string> getMediaUrl)
        : this(getMediaUrl, StaticServiceProvider.Instance.GetRequiredService<IImageUrlTokenGenerator>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HtmlImageSourceParser"/> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider for resolving media URLs.</param>
    [Obsolete("Please use the constructor that accepts IImageUrlTokenGenerator. Scheduled for removal in Umbraco 19.")]
    public HtmlImageSourceParser(IPublishedUrlProvider publishedUrlProvider)
        : this(publishedUrlProvider, StaticServiceProvider.Instance.GetRequiredService<IImageUrlTokenGenerator>())
    {
    }

    /// <summary>
    ///     Parses media UDIs out of an HTML string by reading <c>data-udi</c> attributes on
    ///     <c>&lt;a&gt;</c> and <c>&lt;img&gt;</c> tags.
    /// </summary>
    /// <param name="text">The HTML text to scan.</param>
    /// <returns>The parseable UDIs found on <c>data-udi</c> attributes, in document order.</returns>
    public IEnumerable<Udi> FindUdisFromDataAttributes(string text)
    {
        MatchCollection matches = _dataUdiAttributeRegex.Matches(text);
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
    ///     Refreshes the <c>src</c> attribute of every Umbraco <c>&lt;img&gt;</c> tag in the supplied HTML
    ///     so the rendered URL points at the current media path and carries an up-to-date HMAC signature.
    /// </summary>
    /// <param name="text">The HTML text to process.</param>
    /// <returns>
    ///     The HTML with each <c>&lt;img data-udi="..."&gt;</c> rewritten: the path is replaced with the
    ///     current media URL (preserving the persisted query string), and any HMAC token in the query
    ///     is re-signed against the currently configured secret key. Other tags are returned unchanged.
    /// </returns>
    /// <remarks>Umbraco image tags are identified by their <c>data-udi</c> attributes.</remarks>
    public string EnsureImageSources(string text)
    {
        _getMediaUrl ??= guid => _publishedUrlProvider?.GetMediaUrl(guid);

        return _resolveImgRegex.Replace(text, match =>
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
            Match src = _srcAttributeRegex.Match(match.Value);

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

            // Re-sign the URL so a rotated HMAC secret key doesn't break previously-authored images.
            // No-op when HMAC isn't configured.
            var refreshedSrc = _imageUrlTokenGenerator.RefreshSignature($"{mediaUrl}{src.Groups[2].Value}");

            return match.Value.Replace(src.Value, $"src=\"{refreshedSrc}\"");
        });
    }

    /// <summary>
    ///     Clears the media path from the <c>src</c> attribute of every <c>&lt;img&gt;</c> tag that has a
    ///     <c>data-udi</c> attribute, preserving any query string the URL carried.
    /// </summary>
    /// <param name="text">The HTML text to process.</param>
    /// <returns>
    ///     The HTML with the path portion of each Umbraco-managed image <c>src</c> emptied (the query
    ///     string, if present, is left in place). Tags without a <c>data-udi</c> attribute are unchanged.
    /// </returns>
    public string RemoveImageSources(string text)

        // find each ResolveImgPattern match in the text, then find each
        // SrcAttributeRegex match in the match value, then replace the src
        // attribute value with an empty string
        // (see comment in ResolveMediaFromTextString for group reference)
        => _resolveImgRegex.Replace(text, match =>
        {
            // Find the src attribute
            Match src = _srcAttributeRegex.Match(match.Value);

            return src.Success == false || string.IsNullOrWhiteSpace(src.Groups[1].Value) ?
                match.Value : match.Value.Replace(src.Groups[1].Value, string.Empty);
        });

    [GeneratedRegex(@"<img[^>]*(data-udi=""([^""]*)"")[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-GB")]
    private static partial Regex ResolveImgRegex();

    [GeneratedRegex(@"src=""([^""\?]*)(\?[^""]*)?""", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-GB")]
    private static partial Regex SrcAttributeRegex();

    [GeneratedRegex(@"data-udi=\\?(?:""|')(?<udi>umb://[A-z0-9\-]+/[A-z0-9]+)\\?(?:""|')", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex DataUdiAttributeRegex();
}
