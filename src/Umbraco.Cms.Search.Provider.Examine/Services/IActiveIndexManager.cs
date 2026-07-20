namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// Manages the active/shadow index slots for zero-downtime reindexing.
/// Each logical index alias maps to two physical Examine indexes (suffixed _a and _b).
/// During rebuild, writes go to the shadow index while the active continues serving queries.
/// </summary>
public interface IActiveIndexManager
{
    /// <summary>
    /// Resolves the physical index name that is currently serving queries.
    /// </summary>
    string ResolveActiveIndexName(string indexAlias);

    /// <summary>
    /// Resolves the physical index name that is available for rebuilding.
    /// </summary>
    string ResolveShadowIndexName(string indexAlias);

    /// <summary>
    /// Returns true if a rebuild is currently in progress for the given index alias.
    /// </summary>
    bool IsRebuilding(string indexAlias);

    /// <summary>
    /// Marks the given index alias as rebuilding. No-op if already rebuilding.
    /// </summary>
    void StartRebuilding(string indexAlias);

    /// <summary>
    /// Swaps the active and shadow indexes, then clears the rebuilding flag.
    /// </summary>
    void CompleteRebuilding(string indexAlias);

    /// <summary>
    /// Clears the rebuilding flag without swapping. Used when a rebuild is cancelled or fails.
    /// </summary>
    void CancelRebuilding(string indexAlias);
}
