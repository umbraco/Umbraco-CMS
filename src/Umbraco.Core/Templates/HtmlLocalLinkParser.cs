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
    // needs to support media and document links, order of attributes should not matter nor should other attributes mess with things
    // <a type="media" href="/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}" title="media">media</a>
    // <a type="document" href="/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}" title="other page">other page</a>
    private static readonly Regex _localLinkTagPattern = new(
        @"<a.+?href=['""](?<locallink>\/?{localLink:(?<guid>[a-fA-F0-9-]+)})[^>]*?>",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex _typePattern = new(
        """type=['"](?<type>(?:media|document))['"]""",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    private static readonly Regex _localLinkPattern = new(
        @"href=['""](?<locallink>\/?(?:\{|\%7B)localLink:(?<guid>[a-zA-Z0-9-://]+)(?:\}|\%7D))",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    private static readonly Regex _culturePattern = new( """culture=['"](?<culture>[a-zA-Z0-9-_]+)['"]""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _linkPattern = new(@"(<a\b(?=[^>]*data-culture=['""](?<culture>[a-zA-Z0-9-_]+)['""])(?=[^>]*href=['""])[^>]*href=['""])[^""']*[""']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public HtmlLocalLinkParser(IPublishedUrlProvider publishedUrlProvider)
    {
        _publishedUrlProvider = publishedUrlProvider;
    }

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
                    Constants.UdiEntityType.Document => _publishedUrlProvider.GetUrl(tagData.Udi.Guid, urlMode, tagData.Culture),
                    Constants.UdiEntityType.Media => _publishedUrlProvider.GetMediaUrl(tagData.Udi.Guid, urlMode),
                    _ => string.Empty,
                };

                text = StripTypeAttributeFromTag(text, tagData.Udi!.EntityType);
                text = ReplaceLink(text, tagData.TagHref, newLink, tagData.Culture);
            }
            else if (tagData.IntId.HasValue)
            {
                var newLink = _publishedUrlProvider.GetUrl(tagData.IntId.Value, urlMode);
                text = text.Replace(tagData.TagHref, newLink);
            }
        }

        return text;
    }

    private string ReplaceLink(string text, string tagHref, string newLink, string? culture = null)
    {
        if (string.IsNullOrEmpty(culture))
        {
            return text.Replace(tagHref, newLink);
        }
        else
        {
            return _linkPattern.Replace(text, match =>
            {
                if (match.Groups["culture"].Value.Equals(culture, StringComparison.OrdinalIgnoreCase))
                {
                    return match.Groups[1].Value + newLink + "\"";
                }
                return match.Value;
            });
        }
    }

    // under normal circumstances, the type attribute is preceded by a space
    // to cover the rare occasion where it isn't, we first replace with a space and then without.
    private static string StripTypeAttributeFromTag(string tag, string type) =>
        tag.Replace($" type=\"{type}\"", string.Empty)
            .Replace($"type=\"{type}\"", string.Empty);

    private IEnumerable<LocalLinkTag> FindLocalLinkIds(string text)
    {
        MatchCollection localLinkTagMatches = _localLinkTagPattern.Matches(text);
        foreach (Match linkTag in localLinkTagMatches)
        {
            if (Guid.TryParse(linkTag.Groups["guid"].Value, out Guid guid) is false)
            {
                continue;
            }

            // Find the type attribute
            Match typeMatch = _typePattern.Match(linkTag.Value);
            if (typeMatch.Success is false)
            {
                continue;
            }

            // Find the culture attribute if it exists
            Match cultureMatch = _culturePattern.Match(linkTag.Value);

            yield return new LocalLinkTag(
                null,
                new GuidUdi(typeMatch.Groups["type"].Value, guid),
                linkTag.Groups["locallink"].Value,
                cultureMatch.Success ? cultureMatch.Groups["culture"].Value : null
            );
        }

        // also return legacy results for values that have not been migrated
        foreach (LocalLinkTag legacyResult in FindLegacyLocalLinkIds(text))
        {
            yield return legacyResult;
        }
    }

    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migration. Scheduled for removal in v18")]
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
                    yield return new LocalLinkTag(null, guidUdi, tag.Groups["locallink"].Value);
                }
            }

            if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
            {
                yield return new LocalLinkTag (intId, null, tag.Groups["locallink"].Value);
            }
        }
    }

    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migration. Scheduled for removal in v18")]
    public class LocalLinkTag
    {
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

        public int? IntId { get; }

        public GuidUdi? Udi { get; }

        public string TagHref { get; }

        public string? Culture { get; }
    }
}
