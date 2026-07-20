namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// The message type to be used for syncing across servers.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Refresh all items of a specific cache type.
    /// </summary>
    RefreshAll,

    /// <summary>
    /// Refresh specific items by their identifiers.
    /// </summary>
    RefreshById,

    /// <summary>
    /// Refresh items using JSON payload data.
    /// </summary>
    RefreshByJson,

    /// <summary>
    /// Remove specific items by their identifiers.
    /// </summary>
    RemoveById,

    /// <summary>
    /// Refresh items by their object instances (local only, cannot be distributed).
    /// </summary>
    RefreshByInstance,

    /// <summary>
    /// Remove items by their object instances (local only, cannot be distributed).
    /// </summary>
    RemoveByInstance,

    /// <summary>
    /// Refresh items using a typed payload.
    /// </summary>
    RefreshByPayload,
}
