using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to IContentBase to get URL segments.
/// </summary>
public static class ContentBaseExtensions
{
    private static DefaultUrlSegmentProvider? _defaultUrlSegmentProvider;

    /// <summary>
    /// Gets a single URL segment for a specified content and culture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="shortStringHelper"></param>
    /// <param name="urlSegmentProviders"></param>
    /// <param name="culture">The culture.</param>
    /// <param name="published">Whether to get the published or draft.</param>
    /// <returns>The URL segment.</returns>
    /// <remarks>
    /// If more than one URL segment provider is available, the first one that returns a non-null value will be returned.
    /// </remarks>
    public static string? GetUrlSegment(this IContentBase content, IShortStringHelper shortStringHelper, IEnumerable<IUrlSegmentProvider> urlSegmentProviders, string? culture = null, bool published = true)
    {
        var urlSegment = GetUrlSegments(content, urlSegmentProviders, culture, published).FirstOrDefault();

        // Ensure we have at least the segment from the default URL provider returned.
        urlSegment ??= GetDefaultUrlSegment(shortStringHelper, content, culture, published);

        return urlSegment;
    }

    /// <summary>
    /// Gets all URL segments for a specified content and culture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="shortStringHelper"></param>
    /// <param name="urlSegmentProviders"></param>
    /// <param name="culture">The culture.</param>
    /// <param name="published">Whether to get the published or draft.</param>
    /// <returns>The collection of URL segments.</returns>
    public static IEnumerable<string> GetUrlSegments(this IContentBase content, IShortStringHelper shortStringHelper, IEnumerable<IUrlSegmentProvider> urlSegmentProviders, string? culture = null, bool published = true)
    {
        var urlSegments = GetUrlSegments(content, urlSegmentProviders, culture, published).Distinct().ToList();

        // Ensure we have at least the segment from the default URL provider returned.
        if (urlSegments.Count == 0)
        {
            var defaultUrlSegment = GetDefaultUrlSegment(shortStringHelper, content, culture, published);
            if (defaultUrlSegment is not null)
            {
                urlSegments.Add(defaultUrlSegment);
            }
        }

        return urlSegments;
    }

    private static IEnumerable<string> GetUrlSegments(
        IContentBase content,
        IEnumerable<IUrlSegmentProvider> urlSegmentProviders,
        string? culture,
        bool published)
    {
        foreach (IUrlSegmentProvider urlSegmentProvider in urlSegmentProviders)
        {
            var segment = urlSegmentProvider.GetUrlSegment(content, published, culture);
            if (string.IsNullOrEmpty(segment) == false)
            {
                yield return segment;

                if (urlSegmentProvider.AllowAdditionalSegments is false)
                {
                    yield break;
                }
            }
        }
    }

    private static string? GetDefaultUrlSegment(
        IShortStringHelper shortStringHelper,
        IContentBase content,
        string? culture,
        bool published)
    {
        _defaultUrlSegmentProvider ??= new DefaultUrlSegmentProvider(shortStringHelper);
        return _defaultUrlSegmentProvider.GetUrlSegment(content, published, culture);
    }
}
