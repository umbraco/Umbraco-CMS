namespace Umbraco.Cms.Core.Services.Changes;

/// <summary>
///     Represents the types of changes that can occur on a tree item.
/// </summary>
[Flags]
public enum TreeChangeTypes : byte
{
    /// <summary>
    ///     No change has occurred.
    /// </summary>
    None = 0,

    /// <summary>
    ///     All items have been refreshed.
    /// </summary>
    // all items have been refreshed
    RefreshAll = 1,

    /// <summary>
    ///     An item node has been refreshed with only local impact.
    /// </summary>
    // an item node has been refreshed
    // with only local impact
    RefreshNode = 2,

    /// <summary>
    ///     An item node has been refreshed with branch impact.
    /// </summary>
    // an item node has been refreshed
    // with branch impact
    RefreshBranch = 4,

    /// <summary>
    ///     An item node has been removed permanently.
    /// </summary>
    // an item node has been removed
    // never to return
    Remove = 8,
}
