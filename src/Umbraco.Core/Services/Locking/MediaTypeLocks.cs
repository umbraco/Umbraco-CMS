namespace Umbraco.Cms.Core.Services.Locking;

/// <summary>
///     Provides lock identifiers for media type operations to ensure thread-safe access.
/// </summary>
/// <remarks>
///     This class defines the lock IDs required for read and write operations on media types.
///     The order of locks is critical to prevent deadlocks when multiple locks are acquired.
/// </remarks>
internal static class MediaTypeLocks
{
    /// <summary>
    ///     Gets the lock identifiers required for read operations on media types.
    /// </summary>
    /// <value>
    ///     An array containing <see cref="Constants.Locks.MediaTypes"/>.
    /// </value>
    // beware! order is important to avoid deadlocks
    internal static int[] ReadLockIds { get; } = { Constants.Locks.MediaTypes };

    /// <summary>
    ///     Gets the lock identifiers required for write operations on media types.
    /// </summary>
    /// <value>
    ///     An array containing <see cref="Constants.Locks.MediaTree"/> and <see cref="Constants.Locks.MediaTypes"/>
    ///     in the order they must be acquired.
    /// </value>
    // beware! order is important to avoid deadlocks
    internal static int[] WriteLockIds { get; } = { Constants.Locks.MediaTree, Constants.Locks.MediaTypes };
}
