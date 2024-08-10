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
        @"<a\s+(?:(?:(?:type=['""](?<type>document|media)['""].*?(?<locallink>href=[""']/{localLink:(?<guid>[a-fA-F0-9-]+)})[""'])|((?<locallink>href=[""']/{localLink:(?<guid>[a-fA-F0-9-]+)})[""'].*?type=(['""])(?<type>document|media)(?:['""])))|(?:(?:type=['""](?<type>document|media)['""])|(?:(?<locallink>href=[""']/{localLink:[a-fA-F0-9-]+})[""'])))[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    internal static readonly Regex LocalLinkPattern = new(
        @"href=""[/]?(?:\{|\%7B)localLink:([a-zA-Z0-9-://]+)(?:\}|\%7D)",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    private readonly IPublishedUrlProvider _publishedUrlProvider;

    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public HtmlLocalLinkParser(
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedUrlProvider publishedUrlProvider)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
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
    public string EnsureInternalLinks(string text, bool preview)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            throw new InvalidOperationException("Could not parse internal links, there is no current UmbracoContext");
        }

        if (!preview)
        {
            return EnsureInternalLinks(text);
        }

        using (umbracoContext.ForcedPreview(preview)) // force for URL provider
        {
            return EnsureInternalLinks(text);
        }
    }

    /// <summary>
    ///     Parses the string looking for the {localLink} syntax and updates them to their correct links.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string EnsureInternalLinks(string text)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out _))
        {
            throw new InvalidOperationException("Could not parse internal links, there is no current UmbracoContext");
        }

        foreach (LocalLinkTag tagData in FindLocalLinkIds(text))
        {
            if (tagData.Udi is not null)
            {
                var newLink = "#";
                if (tagData.Udi?.EntityType == Constants.UdiEntityType.Document)
                {
                    newLink = _publishedUrlProvider.GetUrl(tagData.Udi.Guid);
                }
                else if (tagData.Udi?.EntityType == Constants.UdiEntityType.Media)
                {
                    newLink = _publishedUrlProvider.GetMediaUrl(tagData.Udi.Guid);
                }


                text = StripTypeAttributeFromTag(text, tagData.Udi!.EntityType);
                text = text.Replace(tagData.TagHref, "href=\"" + newLink);
            }
            else if (tagData.IntId.HasValue)
            {
                var newLink = _publishedUrlProvider.GetUrl(tagData.IntId.Value);
                text = text.Replace(tagData.TagHref, "href=\"" + newLink);
            }
        }

        return text;
    }

    // under normal circumstances, the type attribute is preceded by a space
    // to cover the rare occasion where it isn't, we first replace with a a space and then without.
    private string StripTypeAttributeFromTag(string tag, string type) =>
        tag.Replace($" type=\"{type}\"", string.Empty)
            .Replace($"type=\"{type}\"", string.Empty);

    private IEnumerable<LocalLinkTag> FindLocalLinkIds(string text)
    {
        MatchCollection localLinkTagMatches = LocalLinkTagPattern.Matches(text);
        foreach (Match linkTag in localLinkTagMatches)
        {
            if (linkTag.Groups.Count < 1)
            {
                continue;
            }

            if (Guid.TryParse(linkTag.Groups["guid"].Value, out Guid guid) is false)
            {
                continue;
            }

            yield return new LocalLinkTag(
                null,
                new GuidUdi(linkTag.Groups["type"].Value, guid),
                linkTag.Groups["locallink"].Value,
                linkTag.Value);
        }

        // also return legacy results for values that have not been migrated
        foreach (LocalLinkTag legacyResult in FindLegacyLocalLinkIds(text))
        {
            yield return legacyResult;
        }
    }

    // todo remove at some point?
    private IEnumerable<LocalLinkTag> FindLegacyLocalLinkIds(string text)
    {
        // Parse internal links
        MatchCollection tags = LocalLinkPattern.Matches(text);
        foreach (Match tag in tags)
        {
            if (tag.Groups.Count > 0)
            {
                var id = tag.Groups[1].Value; // .Remove(tag.Groups[1].Value.Length - 1, 1);

                // The id could be an int or a UDI
                if (UdiParser.TryParse(id, out Udi? udi))
                {
                    var guidUdi = udi as GuidUdi;
                    if (guidUdi is not null)
                    {
                        yield return new LocalLinkTag(null, guidUdi, tag.Value, null);
                    }
                }

                if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
                {
                    yield return new LocalLinkTag (intId, null, tag.Value, null);
                }
            }
        }
    }

    private class LocalLinkTag
    {
        public LocalLinkTag(int? intId, GuidUdi? udi, string tagHref)
        {
            IntId = intId;
            Udi = udi;
            TagHref = tagHref;
        }

        public LocalLinkTag(int? intId, GuidUdi? udi, string tagHref, string? fullTag)
        {
            IntId = intId;
            Udi = udi;
            TagHref = tagHref;
            FullTag = fullTag;
        }

        public int? IntId { get; }

        public GuidUdi? Udi { get; }

        public string TagHref { get; }

        public string? FullTag { get; }
    }
}
