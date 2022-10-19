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
        foreach ((var intId, GuidUdi? udi, var tagValue) in FindLocalLinkIds(text))
        {
            if (udi is not null)
            {
                yield return udi; // In v8, we only care abuot UDIs
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
    /// <param name="urlProvider"></param>
    /// <returns></returns>
    public string EnsureInternalLinks(string text)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out _))
        {
            throw new InvalidOperationException("Could not parse internal links, there is no current UmbracoContext");
        }

        foreach ((var intId, GuidUdi? udi, var tagValue) in FindLocalLinkIds(text))
        {
            if (udi is not null)
            {
                var newLink = "#";
                if (udi?.EntityType == Constants.UdiEntityType.Document)
                {
                    newLink = _publishedUrlProvider.GetUrl(udi.Guid);
                }
                else if (udi?.EntityType == Constants.UdiEntityType.Media)
                {
                    newLink = _publishedUrlProvider.GetMediaUrl(udi.Guid);
                }

                if (newLink == null)
                {
                    newLink = "#";
                }

                text = text.Replace(tagValue, "href=\"" + newLink);
            }
            else if (intId.HasValue)
            {
                var newLink = _publishedUrlProvider.GetUrl(intId.Value);
                text = text.Replace(tagValue, "href=\"" + newLink);
            }
        }

        return text;
    }

    private IEnumerable<(int? intId, GuidUdi? udi, string tagValue)> FindLocalLinkIds(string text)
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
                        yield return (null, guidUdi, tag.Value);
                    }
                }

                if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
                {
                    yield return (intId, null, tag.Value);
                }
            }
        }
    }
}
