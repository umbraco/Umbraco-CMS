namespace Umbraco.Cms.Core.Services.Locking;

internal static class ContentTypeLocks
{
    // beware! order is important to avoid deadlocks
    internal static int[] ReadLockIds { get; } = { Constants.Locks.ContentTypes };

    internal static int[] WriteLockIds { get; } = { Constants.Locks.ContentTree, Constants.Locks.ContentTypes };
}
