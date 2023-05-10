using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Infrastructure.Search;

internal interface IDeliveryApiIndexingHandler
{
    /// <summary>
    ///     Returns true if the indexing handler is enabled
    /// </summary>
    /// <remarks>
    ///     If this is false then there will be no data lookups executed to populate indexes
    ///     when service changes are made.
    /// </remarks>
    bool Enabled { get; }

    /// <summary>
    ///     Handles index updates for content changes
    /// </summary>
    /// <param name="changes">The list of changes by content ID</param>
    void HandleContentChanges(IList<KeyValuePair<int, TreeChangeTypes>> changes);

    /// <summary>
    ///     Handles index updates for content type changes
    /// </summary>
    /// <param name="changes">The list of changes by content type ID</param>
    void HandleContentTypeChanges(IList<KeyValuePair<int, ContentTypeChangeTypes>> changes);
}
