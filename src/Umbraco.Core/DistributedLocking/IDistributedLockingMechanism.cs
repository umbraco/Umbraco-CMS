using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;

namespace Umbraco.Cms.Core.DistributedLocking;

/// <summary>
///     Represents a class responsible for managing distributed locks.
/// </summary>
/// <remarks>
///     In general the rules for distributed locks are as follows.
///     <list type="bullet">
///         <item>
///             <b>Cannot</b> obtain a write lock if a read lock exists for same lock id (except during an upgrade from
///             reader -> writer)
///         </item>
///         <item>
///             <b>Cannot</b> obtain a write lock if a write lock exists for same lock id.
///         </item>
///         <item>
///             <b>Cannot</b> obtain a read lock if a write lock exists for same lock id.
///         </item>
///         <item>
///             <b>Can</b> obtain a read lock if a read lock exists for same lock id.
///         </item>
///     </list>
/// </remarks>
public interface IDistributedLockingMechanism
{
    /// <summary>
    ///     Gets a value indicating whether this distributed locking mechanism can be used.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    ///     Obtains a distributed read lock.
    /// </summary>
    /// <remarks>
    ///     When timeout is null, implementations should use
    ///     <see cref="GlobalSettings.DistributedLockingReadLockDefaultTimeout" />.
    /// </remarks>
    /// <exception cref="DistributedReadLockTimeoutException">Failed to obtain distributed read lock in time.</exception>
    IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null);

    /// <summary>
    ///     Obtains a distributed read lock.
    /// </summary>
    /// <remarks>
    ///     When timeout is null, implementations should use
    ///     <see cref="GlobalSettings.DistributedLockingWriteLockDefaultTimeout" />.
    /// </remarks>
    /// <exception cref="DistributedWriteLockTimeoutException">Failed to obtain distributed write lock in time.</exception>
    IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null);
}
