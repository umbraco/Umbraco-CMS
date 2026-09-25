// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Core.Extensions;

public static class IndexValueExtensions
{
    /// <summary>
    /// Merges two <see cref="IndexValue"/> instances, concatenating and de-duplicating their respective value collections.
    /// </summary>
    public static IndexValue Merge(this IndexValue original, IndexValue toMerge)
        => new()
        {
            TextsR1 = MergeValues(original.TextsR1, toMerge.TextsR1),
            TextsR2 = MergeValues(original.TextsR2, toMerge.TextsR2),
            TextsR3 = MergeValues(original.TextsR3, toMerge.TextsR3),
            Texts = MergeValues(original.Texts, toMerge.Texts),
            Keywords = MergeValues(original.Keywords, toMerge.Keywords),
            Integers = MergeValues(original.Integers, toMerge.Integers),
            Decimals = MergeValues(original.Decimals, toMerge.Decimals),
            DateTimeOffsets = MergeValues(original.DateTimeOffsets, toMerge.DateTimeOffsets),
        };

    private static IEnumerable<T>? MergeValues<T>(IEnumerable<T>? one, IEnumerable<T>? other)
    {
        if (one is null)
        {
            return other;
        }

        return other is null ? one : one.Concat(other).Distinct();
    }
}
