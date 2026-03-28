// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Determines whether a block element is exposed for a given culture, considering language fallback policies.
/// </summary>
internal static class BlockExposeFallbackHelper
{
    /// <summary>
    /// Checks whether a block element is exposed for the expected culture and segment, optionally walking
    /// the language fallback chain when <see cref="Fallback.Language"/> or <see cref="Fallback.DefaultLanguage"/>
    /// policies are specified.
    /// </summary>
    /// <param name="expose">The expose entries from the block value.</param>
    /// <param name="elementKey">The key of the block element to check.</param>
    /// <param name="expectedCulture">The expected culture, or <c>null</c> for invariant blocks.</param>
    /// <param name="expectedSegment">The expected segment, or <c>null</c> for the default segment.</param>
    /// <param name="fallback">The fallback policy from the current variation context.</param>
    /// <param name="languagesByIsoCode">All configured languages keyed by ISO code, used for walking fallback chains.</param>
    /// <param name="defaultIsoCode">The default language ISO code, used for <see cref="Fallback.DefaultLanguage"/> checks.</param>
    /// <returns><c>true</c> if the block is exposed for the expected culture/segment or reachable via the specified fallback policy; otherwise <c>false</c>.</returns>
    public static bool IsBlockExposed(
        IEnumerable<BlockItemVariation> expose,
        Guid elementKey,
        string? expectedCulture,
        string? expectedSegment,
        Fallback fallback,
        Dictionary<string, ILanguage> languagesByIsoCode,
        string defaultIsoCode)
    {
        IList<BlockItemVariation> exposeList = expose as IList<BlockItemVariation> ?? expose.ToList();

        // Direct match check.
        if (IsExposedForCulture(exposeList, elementKey, expectedCulture, expectedSegment))
        {
            return true;
        }

        // Only apply language fallback for culture-variant blocks.
        if (expectedCulture is null)
        {
            return false;
        }

        // Check fallback policies in order.
        foreach (var policy in fallback)
        {
            switch (policy)
            {
                case Fallback.Language:
                    if (IsExposedViaLanguageFallback(exposeList, elementKey, expectedCulture, expectedSegment, languagesByIsoCode))
                    {
                        return true;
                    }

                    break;
                case Fallback.DefaultLanguage:
                    if (IsExposedForCulture(exposeList, elementKey, defaultIsoCode, expectedSegment))
                    {
                        return true;
                    }

                    break;
            }
        }

        return false;
    }

    private static bool IsExposedForCulture(
        IList<BlockItemVariation> expose,
        Guid elementKey,
        string? culture,
        string? segment)
        => expose.Any(v =>
            v.ContentKey == elementKey &&
            string.Equals(v.Culture, culture, StringComparison.OrdinalIgnoreCase) &&
            v.Segment == segment);

    private static bool IsExposedViaLanguageFallback(
        IList<BlockItemVariation> expose,
        Guid elementKey,
        string expectedCulture,
        string? expectedSegment,
        Dictionary<string, ILanguage> languagesByIsoCode)
    {
        if (languagesByIsoCode.TryGetValue(expectedCulture, out ILanguage? language) is false)
        {
            return false;
        }

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ILanguage? current = language;

        while (current?.FallbackIsoCode is not null)
        {
            if (visited.Add(current.FallbackIsoCode) is false)
            {
                break; // Circular fallback protection
            }

            if (IsExposedForCulture(expose, elementKey, current.FallbackIsoCode, expectedSegment))
            {
                return true;
            }

            languagesByIsoCode.TryGetValue(current.FallbackIsoCode, out current);
        }

        return false;
    }
}
