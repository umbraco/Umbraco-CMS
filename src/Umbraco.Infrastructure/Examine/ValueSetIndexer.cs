using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Writes a batch of <see cref="ValueSet" />s to one or more indexes.
/// </summary>
/// <remarks>
///     When a single index is registered the value sets are streamed straight through, so a lazily-built
///     sequence is enumerated once and never fully materialised in memory — keeping the common single-index
///     rebuild's peak memory down. When multiple indexes are registered the sequence is materialised once and
///     reused, so the (potentially expensive) value sets are not rebuilt per index.
/// </remarks>
internal static class ValueSetIndexer
{
    public static void IndexItems(IReadOnlyList<IIndex> indexes, IEnumerable<ValueSet> valueSets)
    {
        switch (indexes.Count)
        {
            case 0:
                return;
            case 1:
                indexes[0].IndexItems(valueSets);
                return;
            default:
                ValueSet[] materialized = valueSets as ValueSet[] ?? valueSets.ToArray();
                foreach (IIndex index in indexes)
                {
                    index.IndexItems(materialized);
                }

                return;
        }
    }
}
