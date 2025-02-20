using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Utility class used to parse internal links
/// </summary>
public sealed class HtmlLocalLinkParser
{
    // needs to support media and document links, order of attributes should not matter nor should other attributes mess with things
    // <a type="media" href="/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}" title="media">media</a>
    // <a type="document" href="/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}" title="other page">other page</a>
    internal static readonly Regex LocalLinkTagPattern = new(
        @"<a.+?href=['""](?<locallink>\/?{localLink:(?<guid>[a-fA-F0-9-]+)})[^>]*?>",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

    internal static readonly Regex TypePattern = new(
        """type=['"](?<type>(?:media|document))['"]""",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    internal static readonly Regex LocalLinkPattern = new(
        @"href=['""](?<locallink>\/?(?:\{|\%7B)localLink:(?<guid>[a-zA-Z0-9-://]+)(?:\}|\%7D))",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

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
    /// <param name="text"></param>
    /// <param name="preview"></param>
    /// <returns></returns>
    public string EnsureInternalLinks(string text, bool preview) => EnsureInternalLinks(text);

    /// <summary>
    ///     Parses the string looking for the {localLink} syntax and updates them to their correct links.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string EnsureInternalLinks(string text)
    {
        foreach (LocalLinkTag tagData in FindLocalLinkIds(text))
        {
            if (tagData.Udi is not null)
            {
                var newLink = tagData.Udi?.EntityType switch
                {
                    Constants.UdiEntityType.Document => _publishedUrlProvider.GetUrl(tagData.Udi.Guid),
                    Constants.UdiEntityType.Media => _publishedUrlProvider.GetMediaUrl(tagData.Udi.Guid),
                    _ => string.Empty,
                };

                text = StripTypeAttributeFromTag(text, tagData.Udi!.EntityType);
                text = text.Replace(tagData.TagHref, newLink);
            }
            else if (tagData.IntId.HasValue)
            {
                var newLink = _publishedUrlProvider.GetUrl(tagData.IntId.Value);
                text = text.Replace(tagData.TagHref, newLink);
            }
        }

        return text;
    }

    // under normal circumstances, the type attribute is preceded by a space
    // to cover the rare occasion where it isn't, we first replace with a space and then without.
    private string StripTypeAttributeFromTag(string tag, string type) =>
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

    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migration. Scheduled for removal in v17")]
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

    [Obsolete("This is a temporary method to support legacy formats until we are sure all data has been migration. Scheduled for removal in v17")]
    public class LocalLinkTag
    {
        public LocalLinkTag(int? intId, GuidUdi? udi, string tagHref)
        {
            IntId = intId;
            Udi = udi;
            TagHref = tagHref;
        }

        public int? IntId { get; }

        public GuidUdi? Udi { get; }

        public string TagHref { get; }
    }
}
