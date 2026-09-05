namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Controls how content type structural changes trigger cache rebuilds.
/// </summary>
public enum ContentTypeRebuildMode
{
    /// <summary>
    ///     Rebuilds the database cache immediately during the save operation (default).
    /// </summary>
    Immediate = 0,

    /// <summary>
    ///     Defers the database cache rebuild to a background task with de-duplication,
    ///     allowing the save operation to return faster. Content may be temporarily stale
    ///     until the deferred rebuild completes.
    /// </summary>
    Deferred = 1,
}
