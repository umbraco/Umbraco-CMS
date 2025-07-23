using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Provides URL segments for content.
/// </summary>
/// <remarks>Url segments should comply with IETF RFCs regarding content, encoding, etc.</remarks>
public interface IUrlSegmentProvider
{
    /// <summary>
    /// Gets a value indicating whether the URL segment provider allows additional segments after providing one.
    /// </summary>
    /// <remarks>
    /// If set to true, when more than one URL segment provider is available, futher providers after this one in the collection will be called
    /// even if the current provider provides a segment.
    /// If false, the provider will terminate the chain of URL segment providers if it provides a segment.
    /// </remarks>
    bool AllowAdditionalSegments => false;

    /// <summary>
    ///     Gets the URL segment for a specified content and culture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The URL segment.</returns>
    /// <remarks>
    ///     This is for when Umbraco is capable of managing more than one URL
    ///     per content, in 1-to-1 multilingual configurations. Then there would be one
    ///     URL per culture.
    /// </remarks>
    string? GetUrlSegment(IContentBase content, string? culture = null);

    /// <summary>
    ///     Gets the URL segment for a specified content, published status and  and culture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The URL segment.</returns>
    /// <remarks>
    ///     This is for when Umbraco is capable of managing more than one URL
    ///     per content, in 1-to-1 multilingual configurations. Then there would be one
    ///     URL per culture.
    /// </remarks>
    string? GetUrlSegment(IContentBase content, bool published, string? culture = null) => GetUrlSegment(content, culture);

    // TODO: For the 301 tracking, we need to add another extended interface to this so that
    // the RedirectTrackingEventHandler can ask the IUrlSegmentProvider if the URL is changing.
    // Currently the way it works is very hacky, see notes in: RedirectTrackingEventHandler.ContentService_Publishing
}
