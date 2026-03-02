namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Describes <see cref="RefreshInstruction"/> refresh action type.
/// </summary>
/// <remarks>
/// <para>
/// This enum is exposed in CacheRefresher web service through <see cref="RefreshInstruction"/>,
/// so it is kept as-is for backward compatibility reasons.
/// </para>
/// </remarks>
[Serializable]
public enum RefreshMethodType
{
    /// <summary>
    /// Refresh all cached items of the specified type.
    /// </summary>
    RefreshAll = 0,

    /// <summary>
    /// Refresh a single item by its GUID identifier.
    /// </summary>
    RefreshByGuid = 1,

    /// <summary>
    /// Refresh a single item by its integer identifier.
    /// </summary>
    RefreshById = 2,

    /// <summary>
    /// Refresh multiple items by their integer identifiers.
    /// </summary>
    RefreshByIds = 3,

    /// <summary>
    /// Refresh items using JSON payload data.
    /// </summary>
    RefreshByJson = 4,

    /// <summary>
    /// Remove an item by its integer identifier.
    /// </summary>
    RemoveById = 5,
}
