using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Default implementation of IUrlSegmentProvider.
/// </summary>
public class DefaultUrlSegmentProvider : IUrlSegmentProvider
{
    private readonly IShortStringHelper _shortStringHelper;

    public DefaultUrlSegmentProvider(IShortStringHelper shortStringHelper) => _shortStringHelper = shortStringHelper;


    public virtual string? GetUrlSegment(IContentBase content, bool published, string? culture = null) =>
        GetUrlSegmentSource(content, culture, published)?.ToUrlSegment(_shortStringHelper, culture);

    /// <summary>
    ///     Gets the URL segment for a specified content and culture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The URL segment.</returns>
    public virtual string? GetUrlSegment(IContentBase content, string? culture = null) =>
        GetUrlSegmentSource(content, culture, true)?.ToUrlSegment(_shortStringHelper, culture);

    private static string? GetUrlSegmentSource(IContentBase content, string? culture, bool published)
    {
        string? source = null;
        if (content.HasProperty(Constants.Conventions.Content.UrlName))
        {
            source = (content.GetValue<string>(Constants.Conventions.Content.UrlName, culture, published: published) ?? string.Empty).Trim();
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            // If the name of a node has been updated, but it has not been published, the url should use the published name, not the current node name
            // If this node has never been published (GetPublishName is null), use the unpublished name
            source = content is IContent document && document.Edited && document.GetPublishName(culture) != null
                ? document.GetPublishName(culture)
                : content.GetCultureName(culture);
        }

        return source;
    }
}
