namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// Notification published while a content index is being rebuilt, after its content change strategy has
/// repopulated it and before <see cref="IndexRebuildCompletedNotification"/> is published.
/// </summary>
/// <remarks>
/// This notification allows handlers to add documents that are not tracked by the content change strategy of the
/// index - for example, entities that are not content - so they survive the rebuild.
/// </remarks>
public sealed class IndexRebuildingNotification : INotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexRebuildingNotification"/> class.
    /// </summary>
    /// <param name="indexAlias">The alias of the index being rebuilt.</param>
    public IndexRebuildingNotification(string indexAlias)
    {
        IndexAlias = indexAlias;
    }

    /// <summary>
    /// Gets the alias of the index being rebuilt.
    /// </summary>
    public string IndexAlias { get; }
}
