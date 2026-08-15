using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Search.Core.Notifications;

/// <summary>
/// Notification published when an index rebuild has completed.
/// </summary>
/// <remarks>
/// This notification allows indexer implementations to finalize a rebuild operation,
/// such as swapping a temporary/staging index with the live index for zero-downtime reindexing.
/// </remarks>
public sealed class IndexRebuildCompletedNotification : INotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexRebuildCompletedNotification"/> class.
    /// </summary>
    /// <param name="indexAlias">The alias of the index that was rebuilt.</param>
    public IndexRebuildCompletedNotification(string indexAlias)
    {
        IndexAlias = indexAlias;
    }

    /// <summary>
    /// Gets the alias of the index that was rebuilt.
    /// </summary>
    public string IndexAlias { get; }
}
