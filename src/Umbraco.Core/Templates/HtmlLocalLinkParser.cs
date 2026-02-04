using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Utility class used to parse internal links
/// </summary>
public sealed class HtmlLocalLinkParser
{
    /// <summary>
    ///     Regex pattern to match local link tags with type and href attributes.
    /// </summary>
    /// <remarks>
    ///     Needs to support media and document links. Order of attributes should not matter
    ///     nor should other attributes mess with things.
    ///     Examples:
    ///     &lt;a type="media" href="/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}" title="media"&gt;media&lt;/a&gt;
    ///     &lt;a type="document" href="/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}" title="other page"&gt;other page&lt;/a&gt;
    /// </remarks>
    internal static readonly Regex LocalLinkTagPattern = new(
        @"<a.+?href=['""](?<locallink>\/?{localLink:(?<guid>[a-fA-F0-9-]+)})[^>]*?>",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    ///     Regex pattern to match the type attribute (media or document) in local link tags.
    /// </summary>
    internal static readonly Regex TypePattern = new(
        """type=['"](?<type>(?:media|document))['"]""",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    /// <summary>
    ///     Regex pattern to match legacy local link href patterns.
    /// </summary>
    internal static readonly Regex LocalLinkPattern = new(
        @"href=['""](?<locallink>\/?(?:\{|\%7B)localLink:(?<guid>[a-zA-Z0-9-://]+)(?:\}|\%7D))",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    private readonly IPublishedUrlProvider _publishedUrlProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HtmlLocalLinkParser"/> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider for resolving content and media URLs.</param>
    public HtmlLocalLinkParser(IPublishedUrlProvider publishedUrlProvider)
    {
        _publishedUrlProvider = publishedUrlProvider;
    }

    /// <summary>
    ///     Finds all UDIs from local link references in the specified text.
    /// </summary>
    /// <param name="text">The text to search for local links.</param>
    /// <returns>An enumerable of <see cref="Udi"/> objects found in the text.</returns>
    public IEnumerable<Udi?> FindUdisFromLocalLinks(string text)
    {
        foreach (LocalLinkTag tagData in FindLocalLinkIds(text))
        {
            if (tagData.Udi is not null)
            {
                yield return tagData.Udi; // In v8, we only care about UDIs
            }
        }
    }

    /// <summary>
    ///     Parses the string looking for the {localLink} syntax and updates them to their correct links.
    /// </summary>
    [Obsolete("This method overload is no longer used in Umbraco and delegates to the overload without the preview parameter. Scheduled for removal in Umbraco 18.")]
    public string EnsureInternalLinks(string text, bool preview) => EnsureInternalLinks(text);

    /// <summary>
    ///     Parses the string looking for the {localLink} syntax and updates them to their correct links.
    /// </summary>
    public string EnsureInternalLinks(string text) => EnsureInternalLinks(text, UrlMode.Default);

    /// <summary>
    ///     Parses the string looking for the {localLink} syntax and updates them to their correct links.
    /// </summary>
    public string EnsureInternalLinks(string text, UrlMode urlMode)
    {
        foreach (LocalLinkTag tagData in FindLocalLinkIds(text))
        {
            if (tagData.Udi is not null)
            {
                var newLink = tagData.Udi?.EntityType switch
                {
                    Constants.UdiEntityType.Document => _publishedUrlProvider.GetUrl(tagData.Udi.Guid, urlMode),
                    Constants.UdiEntityType.Media => _publishedUrlProvider.GetMediaUrl(tagData.Udi.Guid, urlMode),
                    _ => string.Empty,
                };

                text = StripTypeAttributeFromTag(text, tagData.Udi!.EntityType);
                text = text.Replace(tagData.TagHref, newLink);
            }
            else if (tagData.IntId.HasValue)
            {
                var newLink = _publishedUrlProvider.GetUrl(tagData.IntId.Value, urlMode);
                text = text.Replace(tagData.TagHref, newLink);
            }
        }

        return text;
    }

    // under normal circumstances, the type attribute is preceded by a space
    // to cover the rare occasion where it isn't, we first replace with a space and then without.
    private static string StripTypeAttributeFromTag(string tag, string type) =>
        tag.Replace($" type=\"{type}\"", string.Empty)
            .Replace($"type=\"{type}\"", string.Empty);

    private IEnumerable<LocalLinkTag> FindLocalLinkIds(string text)
    {
        MatchCollection localLinkTagMatches = LocalLinkTagPattern.Matches(text);
        foreach (Match linkTag in localLinkTagMatches)
        {
            if (Guid.TryParse(linkTag.Groups["guid"].Value, out Guid guid) is false)
            {
                continue;
            }

            // Find the type attribute
            Match typeMatch = TypePattern.Match(linkTag.Value);
            if (typeMatch.Success is false)
            {
                continue;
            }

            yield return new LocalLinkTag(
                null,
                new GuidUdi(typeMatch.Groups["type"].Value, guid),
                linkTag.Groups["locallink"].Value);
        }

        // also return legacy results for values that have not been migrated
        foreach (LocalLinkTag legacyResult in FindLegacyLocalLinkIds(text))
        {
            yield return legacyResult;
        }
    }

    /// <summary>
    ///     Finds legacy local link identifiers in the specified text.
    /// </summary>
    /// <param name="text">The text to search for legacy local links.</param>
    /// <returns>An enumerable of <see cref="LocalLinkTag"/> objects representing legacy links.</returns>
    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migration. Scheduled for removal in v18")]
    public IEnumerable<LocalLinkTag> FindLegacyLocalLinkIds(string text)
    {
        // Parse internal links
        MatchCollection tags = LocalLinkPattern.Matches(text);
        foreach (Match tag in tags)
        {
            if (tag.Groups.Count <= 0)
            {
                continue;
            }

            var id = tag.Groups["guid"].Value;

            // The id could be an int or a UDI
            if (UdiParser.TryParse(id, out Udi? udi))
            {
                if (udi is GuidUdi guidUdi)
                {
                    yield return new LocalLinkTag(null, guidUdi, tag.Groups["locallink"].Value);
                }
            }

            if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
            {
                yield return new LocalLinkTag (intId, null, tag.Groups["locallink"].Value);
            }
        }
    }

    /// <summary>
    ///     Represents a local link tag extracted from HTML content.
    /// </summary>
    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migration. Scheduled for removal in v18")]
    public class LocalLinkTag
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalLinkTag"/> class.
        /// </summary>
        /// <param name="intId">The integer identifier for legacy links.</param>
        /// <param name="udi">The <see cref="GuidUdi"/> for the linked content or media.</param>
        /// <param name="tagHref">The original href value from the tag.</param>
        public LocalLinkTag(int? intId, GuidUdi? udi, string tagHref)
        {
            IntId = intId;
            Udi = udi;
            TagHref = tagHref;
        }

        /// <summary>
        ///     Gets the integer identifier for legacy local links.
        /// </summary>
        public int? IntId { get; }

        /// <summary>
        ///     Gets the <see cref="GuidUdi"/> for the linked content or media.
        /// </summary>
        public GuidUdi? Udi { get; }

        /// <summary>
        ///     Gets the original href value from the local link tag.
        /// </summary>
        public string TagHref { get; }
    }
}
