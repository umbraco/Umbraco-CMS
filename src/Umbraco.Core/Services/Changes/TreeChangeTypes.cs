namespace Umbraco.Cms.Core.Services.Changes;

[Flags]
public enum TreeChangeTypes : byte
{
    None = 0,

    // all items have been refreshed
    RefreshAll = 1,

    // an item node has been refreshed
    // with only local impact
    RefreshNode = 2,

    // an item node has been refreshed
    // with branch impact
    RefreshBranch = 4,

    // an item node has been removed
    // never to return
    Remove = 8,
}
