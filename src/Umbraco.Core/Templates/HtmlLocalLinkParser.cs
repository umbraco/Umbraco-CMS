using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Utility class used to parse internal links.
/// </summary>
/// <remarks>
/// Needs to support media and document links, order of attributes should not matter nor should other attributes mess with things:
/// <![CDATA[
/// <a type="media" href="/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}" title="media">media</a>
/// <a type="document" href="/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}" title="other page">other page</a>
/// ]]>
/// </remarks>
public sealed partial class HtmlLocalLinkParser
{
    /// <summary>
    ///     The name of the capture group holding the local link.
    /// </summary>
    /// <remarks>
    ///     Must match the group name used by every local link pattern below.
    /// </remarks>
    private const string LocalLinkGroupName = "locallink";

    [GeneratedRegex(@"<a\b[^>]*?href=['""](?<locallink>\/?(?:\{|\%7B)localLink:(?<guid>[a-fA-F0-9-]+)(?:\}|\%7D))[^>]*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-GB")]
    private static partial Regex GetLocalLinkTagPattern();

    [GeneratedRegex("""\s*(?<![\w-])type=['"](?<type>(?:media|document))['"]""", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-GB")]
    private static partial Regex GetTypePattern();

    [GeneratedRegex("""data-culture=['"](?<culture>[a-zA-Z0-9-_]+)['"]""", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]

    private static partial Regex GetCulturePattern();

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
        @"<a\b[^>]*?href=['""](?<locallink>\/?(?:\{|\%7B)localLink:(?<guid>[a-fA-F0-9-]+)(?:\}|\%7D))[^>]*?>",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    /// <summary>
    ///     Regex pattern to match the type attribute (media or document) in local link tags.
    /// </summary>
    internal static readonly Regex TypePattern = new(
        """\s*(?<![\w-])type=['"](?<type>(?:media|document))['"]""",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    /// <summary>
    ///     Regex pattern to match legacy local link href patterns.
    /// </summary>
    internal static readonly Regex LocalLinkPattern = new(
        @"href=['""](?<locallink>\/?(?:\{|\%7B)localLink:(?<guid>[a-zA-Z0-9-://]+)(?:\}|\%7D))",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    [GeneratedRegex(@"href=['""](?<locallink>\/?(?:\{|\%7B)localLink:(?<guid>[a-zA-Z0-9-://]+)(?:\}|\%7D))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-GB")]
    private static partial Regex GetLocalLinkPattern();

    private static readonly Regex _localLinkTagPattern = GetLocalLinkTagPattern();

    private static readonly Regex _typePattern = GetTypePattern();

    private static readonly Regex _localLinkPattern = GetLocalLinkPattern();

    private static readonly Regex _culturePattern = GetCulturePattern();

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
    public string EnsureInternalLinks(string text) => EnsureInternalLinks(text, UrlMode.Default);

    /// <summary>
    ///     Parses the string looking for the {localLink} syntax and updates them to their correct links.
    /// </summary>
    public string EnsureInternalLinks(string text, UrlMode urlMode)
    {
        text = _localLinkTagPattern.Replace(text, match => ReplaceLocalLinkTag(match, urlMode));

        return ReplaceLegacyLocalLinks(text, urlMode);
    }

    // Rewrites a single matched tag, replacing the local link with the resolved URL and removing the type
    // attribute that identified the entity. Every edit is confined to the matched tag, so a tag that
    // resolves to nothing keeps its original markup even when a neighbouring tag holds the same local link.
    private string ReplaceLocalLinkTag(Match tagMatch, UrlMode urlMode)
    {
        LocalLinkTagData? tagData = ReadLocalLinkTagData(tagMatch);
        if (tagData is null)
        {
            return tagMatch.Value;
        }

        var newLink = tagData.EntityType switch
        {
            Constants.UdiEntityType.Document => _publishedUrlProvider.GetUrl(tagData.Key, urlMode, tagData.Culture),
            Constants.UdiEntityType.Media => _publishedUrlProvider.GetMediaUrl(tagData.Key, urlMode),
            _ => string.Empty,
        };

        Group localLink = tagMatch.Groups[LocalLinkGroupName];
        var localLinkIndex = localLink.Index - tagMatch.Index;
        Match typeMatch = tagData.TypeMatch;

        // Both offsets are relative to the tag, so the later edit is applied first to keep the other valid.
        if (typeMatch.Index > localLinkIndex)
        {
            return tagMatch.Value
                .Remove(typeMatch.Index, typeMatch.Length)
                .Remove(localLinkIndex, localLink.Length)
                .Insert(localLinkIndex, newLink);
        }

        return tagMatch.Value
            .Remove(localLinkIndex, localLink.Length)
            .Insert(localLinkIndex, newLink)
            .Remove(typeMatch.Index, typeMatch.Length);
    }

    private string ReplaceLegacyLocalLinks(string text, UrlMode urlMode)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        foreach (LocalLinkTag tagData in FindLegacyLocalLinkIds(text))
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if (tagData.Udi is not null)
            {
                var newLink = tagData.Udi.EntityType switch
                {
                    Constants.UdiEntityType.Document => _publishedUrlProvider.GetUrl(tagData.Udi.Guid, urlMode, tagData.Culture),
                    Constants.UdiEntityType.Media => _publishedUrlProvider.GetMediaUrl(tagData.Udi.Guid, urlMode),
                    _ => string.Empty,
                };

                text = text.Replace(tagData.TagHref, newLink);
            }
            else if (tagData.IntId.HasValue)
            {
                text = text.Replace(tagData.TagHref, _publishedUrlProvider.GetUrl(tagData.IntId.Value, urlMode));
            }
        }

        return text;
    }

    private static LocalLinkTagData? ReadLocalLinkTagData(Match tagMatch)
    {
        if (Guid.TryParse(tagMatch.Groups["guid"].ValueSpan, out Guid key) is false)
        {
            return null;
        }

        // A tag that carries no type of its own cannot be resolved, as the entity type is what tells the
        // document and media URL providers apart.
        Match typeMatch = _typePattern.Match(tagMatch.Value);
        if (typeMatch.Success is false)
        {
            return null;
        }

        Match cultureMatch = _culturePattern.Match(tagMatch.Value);

        // Normalize the type to lower case to tolerate historic mis-cased values written by the (now fixed)
        // ConvertLocalLinks migration (see #22597). Constants.UdiEntityType.* values are lower case.
        return new LocalLinkTagData(
            key,
            typeMatch.Groups["type"].Value.ToLowerInvariant(),
            cultureMatch.Success ? cultureMatch.Groups["culture"].Value : null,
            typeMatch);
    }

    private IEnumerable<LocalLinkTag> FindLocalLinkIds(string text)
    {
        foreach (Match linkTag in _localLinkTagPattern.Matches(text))
        {
            LocalLinkTagData? tagData = ReadLocalLinkTagData(linkTag);
            if (tagData is null)
            {
                continue;
            }

            yield return new LocalLinkTag(
                null,
                new GuidUdi(tagData.EntityType, tagData.Key),
                linkTag.Groups[LocalLinkGroupName].Value,
                tagData.Culture);
        }

        // also return legacy results for values that have not been migrated
        foreach (LocalLinkTag legacyResult in FindLegacyLocalLinkIds(text))
        {
            yield return legacyResult;
        }
    }

    private sealed record LocalLinkTagData(Guid Key, string EntityType, string? Culture, Match TypeMatch);

    /// <summary>
    ///     Finds legacy local link identifiers in the specified text.
    /// </summary>
    /// <param name="text">The text to search for legacy local links.</param>
    /// <returns>An enumerable of <see cref="LocalLinkTag"/> objects representing legacy links.</returns>
    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migrated. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<LocalLinkTag> FindLegacyLocalLinkIds(string text)
    {
        // Parse internal links
        MatchCollection tags = _localLinkPattern.Matches(text);
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
                    yield return new LocalLinkTag(null, guidUdi, tag.Groups[LocalLinkGroupName].Value);
                }
            }

            if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
            {
                yield return new LocalLinkTag (intId, null, tag.Groups[LocalLinkGroupName].Value);
            }
        }
    }

    /// <summary>
    ///     Represents a local link tag extracted from HTML content.
    /// </summary>
    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migrated. Scheduled for removal in Umbraco 18.")]
    public class LocalLinkTag
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalLinkTag"/> class.
        /// </summary>
        /// <param name="intId">The integer identifier for legacy links.</param>
        /// <param name="udi">The <see cref="GuidUdi"/> for the linked content or media.</param>
        /// <param name="tagHref">The original href value from the tag.</param>
        public LocalLinkTag(int? intId, GuidUdi? udi, string tagHref)
        : this(intId, udi, tagHref, culture: null)
        {
        }

        public LocalLinkTag(int? intId, GuidUdi? udi, string tagHref, string? culture = null)
        {
            IntId = intId;
            Udi = udi;
            TagHref = tagHref;
            Culture = culture;
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

        public string? Culture { get; }
    }
}
