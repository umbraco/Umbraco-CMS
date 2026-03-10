namespace Umbraco.Cms.Core.Services.Locking;

/// <summary>
///     Provides lock identifiers for member type operations to ensure thread-safe access.
/// </summary>
/// <remarks>
///     This class defines the lock IDs required for read and write operations on member types.
///     The order of locks is critical to prevent deadlocks when multiple locks are acquired.
/// </remarks>
internal static class MemberTypeLocks
{
    /// <summary>
    ///     Gets the lock identifiers required for read operations on member types.
    /// </summary>
    /// <value>
    ///     An array containing <see cref="Constants.Locks.MemberTypes"/>.
    /// </value>
    // beware! order is important to avoid deadlocks
    internal static int[] ReadLockIds { get; } = { Constants.Locks.MemberTypes };

    /// <summary>
    ///     Gets the lock identifiers required for write operations on member types.
    /// </summary>
    /// <value>
    ///     An array containing <see cref="Constants.Locks.MemberTree"/> and <see cref="Constants.Locks.MemberTypes"/>
    ///     in the order they must be acquired.
    /// </value>
    // beware! order is important to avoid deadlocks
    internal static int[] WriteLockIds { get; } = { Constants.Locks.MemberTree, Constants.Locks.MemberTypes };
}
