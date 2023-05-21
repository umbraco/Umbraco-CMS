using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Provides URL segments for content.
/// </summary>
/// <remarks>Url segments should comply with IETF RFCs regarding content, encoding, etc.</remarks>
public interface IUrlSegmentProvider
{
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

    // TODO: For the 301 tracking, we need to add another extended interface to this so that
    // the RedirectTrackingEventHandler can ask the IUrlSegmentProvider if the URL is changing.
    // Currently the way it works is very hacky, see notes in: RedirectTrackingEventHandler.ContentService_Publishing
}
