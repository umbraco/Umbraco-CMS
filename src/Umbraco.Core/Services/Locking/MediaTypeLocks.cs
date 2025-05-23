namespace Umbraco.Cms.Core.Services.Locking;

internal static class MediaTypeLocks
{
    // beware! order is important to avoid deadlocks
    internal static int[] ReadLockIds { get; } = { Constants.Locks.MediaTypes };

    internal static int[] WriteLockIds { get; } = { Constants.Locks.MediaTree, Constants.Locks.MediaTypes };
}
