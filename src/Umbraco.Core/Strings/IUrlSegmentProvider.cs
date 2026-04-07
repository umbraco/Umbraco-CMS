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
    ///     Gets the URL segment for a specified content, published status and culture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="published">Whether to get the published URL segment.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The URL segment.</returns>
    /// <remarks>
    ///     This is for when Umbraco is capable of managing more than one URL
    ///     per content, in 1-to-1 multilingual configurations. Then there would be one
    ///     URL per culture.
    /// </remarks>
    string? GetUrlSegment(IContentBase content, bool published, string? culture = null) => GetUrlSegment(content, culture);

    /// <summary>
    /// Determines whether the URL segment for the given content has changed compared to the
    /// currently published segment. Used by redirect tracking to avoid unnecessary descendant
    /// traversal when URL segments haven't changed.
    /// </summary>
    /// <param name="content">The content being published (carries new property values).</param>
    /// <param name="currentPublishedSegment">The currently published URL segment (from IDocumentUrlService).</param>
    /// <param name="culture">The culture.</param>
    /// <returns>True if the segment has changed, false otherwise.</returns>
    /// <remarks>
    /// The default implementation computes the new URL segment via <see cref="GetUrlSegment(IContentBase, bool, string?)"/>
    /// using draft values (<c>published: false</c>) and compares it to <paramref name="currentPublishedSegment"/>.
    /// Draft values are used because this method is called during publishing, before the new values are committed
    /// as published — so the draft values represent what the segment <em>will</em> be after publishing.
    /// This is intentionally a permanent default so that custom providers automatically get correct change detection
    /// without additional implementation.
    /// Override only if you need custom change detection logic (e.g., URL segments derived from external state).
    /// </remarks>
    bool HasUrlSegmentChanged(IContentBase content, string? currentPublishedSegment, string? culture)
        => !string.Equals(
            GetUrlSegment(content, published: false, culture),
            currentPublishedSegment,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether changes to the given content item may affect URL segments of its
    /// descendant content items. Used by redirect tracking to decide whether descendant
    /// traversal can be skipped when the content item's own URL segment is unchanged.
    /// </summary>
    /// <param name="content">The content item being published.</param>
    /// <returns>
    /// <c>true</c> if this provider may compute descendant segments based on data from this
    /// content item; <c>false</c> if this provider only uses each content item's own data.
    /// </returns>
    /// <remarks>
    /// The default is <c>false</c>, meaning this provider derives segments solely from the
    /// content item itself (e.g. its Name or properties). Custom providers that read ancestor
    /// properties to compute descendant segments should override this — either returning
    /// <c>true</c> unconditionally, or using logic (e.g. checking the content type or whether
    /// relevant properties have changed) to limit the impact to affected subtrees.
    /// </remarks>
    bool MayAffectDescendantSegments(IContentBase content) => false;
}
