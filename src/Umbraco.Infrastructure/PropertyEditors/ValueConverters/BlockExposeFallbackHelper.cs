// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

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
    /// <param name="resolvedCulture">When the method returns <c>true</c>, the culture the block is actually exposed for.
    /// This equals <paramref name="expectedCulture"/> for direct matches, or the fallback culture that was resolved.
    /// When the method returns <c>false</c>, this is <c>null</c>.</param>
    /// <returns><c>true</c> if the block is exposed for the expected culture/segment or reachable via the specified fallback policy; otherwise <c>false</c>.</returns>
    public static bool IsBlockExposed(
        IEnumerable<BlockItemVariation> expose,
        Guid elementKey,
        string? expectedCulture,
        string? expectedSegment,
        Fallback fallback,
        Dictionary<string, ILanguage> languagesByIsoCode,
        string defaultIsoCode,
        out string? resolvedCulture)
    {
        IList<BlockItemVariation> exposeList = expose as IList<BlockItemVariation> ?? expose.ToList();

        // Direct match check.
        if (IsExposedForCulture(exposeList, elementKey, expectedCulture, expectedSegment))
        {
            resolvedCulture = expectedCulture;
            return true;
        }

        // Only apply language fallback for culture-variant blocks.
        if (expectedCulture is null)
        {
            resolvedCulture = null;
            return false;
        }

        // Check fallback policies in order.
        foreach (var policy in fallback)
        {
            switch (policy)
            {
                case Fallback.Language:
                    var languageFallbackCulture = FindLanguageFallbackCulture(exposeList, elementKey, expectedCulture, expectedSegment, languagesByIsoCode);
                    if (languageFallbackCulture is not null)
                    {
                        resolvedCulture = languageFallbackCulture;
                        return true;
                    }

                    break;
                case Fallback.DefaultLanguage:
                    if (IsExposedForCulture(exposeList, elementKey, defaultIsoCode, expectedSegment))
                    {
                        resolvedCulture = defaultIsoCode;
                        return true;
                    }

                    break;
            }
        }

        resolvedCulture = null;
        return false;
    }

    private static bool IsExposedForCulture(
        IList<BlockItemVariation> expose,
        Guid elementKey,
        string? culture,
        string? segment)
        => expose.Any(v =>
            v.ContentKey == elementKey &&
            v.Culture.InvariantEquals(culture) &&
            v.Segment == segment);

    /// <summary>
    /// Walks the language fallback chain and returns the culture that the block is exposed for,
    /// or <c>null</c> if no fallback culture has an expose entry.
    /// </summary>
    private static string? FindLanguageFallbackCulture(
        IList<BlockItemVariation> expose,
        Guid elementKey,
        string expectedCulture,
        string? expectedSegment,
        Dictionary<string, ILanguage> languagesByIsoCode)
    {
        if (languagesByIsoCode.TryGetValue(expectedCulture, out ILanguage? language) is false)
        {
            return null;
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
                return current.FallbackIsoCode;
            }

            languagesByIsoCode.TryGetValue(current.FallbackIsoCode, out current);
        }

        return null;
    }
}
