namespace Umbraco.Cms.Core.Services.Locking;

/// <summary>
///     Provides lock identifiers for content type operations to ensure thread-safe access.
/// </summary>
/// <remarks>
///     This class defines the lock IDs required for read and write operations on content types.
///     The order of locks is critical to prevent deadlocks when multiple locks are acquired.
/// </remarks>
internal static class ContentTypeLocks
{
    /// <summary>
    ///     Gets the lock identifiers required for read operations on content types.
    /// </summary>
    /// <value>
    ///     An array containing <see cref="Constants.Locks.ContentTypes"/>.
    /// </value>
    // beware! order is important to avoid deadlocks
    internal static int[] ReadLockIds { get; } = { Constants.Locks.ContentTypes };

    /// <summary>
    ///     Gets the lock identifiers required for write operations on content types.
    /// </summary>
    /// <value>
    ///     An array containing <see cref="Constants.Locks.ContentTree"/> and <see cref="Constants.Locks.ContentTypes"/>
    ///     in the order they must be acquired.
    /// </value>
    // beware! order is important to avoid deadlocks
    internal static int[] WriteLockIds { get; } = { Constants.Locks.ContentTree, Constants.Locks.ContentTypes };
}
