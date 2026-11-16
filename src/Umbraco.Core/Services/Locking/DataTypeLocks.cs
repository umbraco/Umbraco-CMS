namespace Umbraco.Cms.Core.Services.Locking;

/// <summary>
///     Provides lock identifiers for data type operations to ensure thread-safe access.
/// </summary>
/// <remarks>
///     This class defines the lock IDs required for read and write operations on data types.
///     The order of locks is critical to prevent deadlocks when multiple locks are acquired.
/// </remarks>
internal static class DataTypeLocks
{
    /// <summary>
    ///     Gets the lock identifiers required for read operations on data types.
    /// </summary>
    /// <value>
    ///     An empty array. Reads are not locked, as an unlocked read cannot observe a partially written data type
    ///     structure anyway - the writing transaction is not yet committed.
    /// </value>
    internal static int[] ReadLockIds { get; } = [];

    /// <summary>
    ///     Gets the lock identifiers required for write operations on data types.
    /// </summary>
    /// <value>
    ///     An array containing <see cref="Constants.Locks.DataTypes"/>.
    /// </value>
    // beware! order is important to avoid deadlocks
    internal static int[] WriteLockIds { get; } = { Constants.Locks.DataTypes };
}
