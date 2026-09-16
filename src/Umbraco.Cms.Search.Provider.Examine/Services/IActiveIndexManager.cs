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
    /// <param name="indexAlias">The logical index alias to resolve.</param>
    /// <returns>The name of the physical index currently active for the alias.</returns>
    string ResolveActiveIndexName(string indexAlias);

    /// <summary>
    /// Resolves the physical index name that is available for rebuilding.
    /// </summary>
    /// <param name="indexAlias">The logical index alias to resolve.</param>
    /// <returns>The name of the physical index currently available as the shadow for the alias.</returns>
    string ResolveShadowIndexName(string indexAlias);

    /// <summary>
    /// Returns true if a rebuild is currently in progress for the given index alias.
    /// </summary>
    /// <param name="indexAlias">The logical index alias to check.</param>
    /// <returns>True if a rebuild is in progress; otherwise false.</returns>
    bool IsRebuilding(string indexAlias);

    /// <summary>
    /// Marks the given index alias as rebuilding. No-op if already rebuilding.
    /// </summary>
    /// <param name="indexAlias">The logical index alias to mark as rebuilding.</param>
    void StartRebuilding(string indexAlias);

    /// <summary>
    /// Swaps the active and shadow indexes, then clears the rebuilding flag.
    /// </summary>
    /// <param name="indexAlias">The logical index alias to complete the rebuild for.</param>
    void CompleteRebuilding(string indexAlias);

    /// <summary>
    /// Clears the rebuilding flag without swapping. Used when a rebuild is cancelled or fails.
    /// </summary>
    /// <param name="indexAlias">The logical index alias to cancel the rebuild for.</param>
    void CancelRebuilding(string indexAlias);
}
