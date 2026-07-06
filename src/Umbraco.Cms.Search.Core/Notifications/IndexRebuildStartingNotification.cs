using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Search.Core.Notifications;

/// <summary>
/// Notification published when an index rebuild is starting.
/// </summary>
/// <remarks>
/// This notification allows indexer implementations to prepare for a rebuild operation,
/// such as creating a temporary/staging index for zero-downtime reindexing.
/// </remarks>
public sealed class IndexRebuildStartingNotification : INotification
{
    public IndexRebuildStartingNotification(string indexAlias)
    {
        IndexAlias = indexAlias;
    }

    /// <summary>
    /// Gets the alias of the index being rebuilt.
    /// </summary>
    public string IndexAlias { get; }
}
