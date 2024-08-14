namespace Umbraco.Cms.Core.Services.Locking;

internal static class MemberTypeLocks
{
    // beware! order is important to avoid deadlocks
    internal static int[] ReadLockIds { get; } = { Constants.Locks.MemberTypes };

    internal static int[] WriteLockIds { get; } = { Constants.Locks.MemberTree, Constants.Locks.MemberTypes };
}
