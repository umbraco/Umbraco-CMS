using System;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.DistributedLocking;

/// <summary>
/// Represents a class responsible for managing distributed locks.
/// </summary>
/// <remarks>
/// The rules for distributed locks are as follows.
/// <list type="bullet">
/// <item>
/// <b>Cannot</b> obtain a write lock if a read lock exists for same lock id.
/// </item>
/// <item>
/// <b>Cannot</b> obtain a write lock if a write lock exists for same lock id.
/// </item>
/// <item>
/// <b>Cannot</b> obtain a read lock if a write lock exists for same lock id.
/// </item>
/// <item>
/// <b>Can</b> obtain a read lock if a write lock exists for same lock id.
/// </item>
/// </list>
/// However please note these rules can be ignored at a higher level of abstraction e.g. IScope will allow upgrade / downgrade of locks within a single transaction.
/// </remarks>
public interface IDistributedLockingMechanism
{
    /// <summary>
    /// Obtains a distributed read lock.
    /// </summary>
    /// <remarks>
    /// When timeout is null, implementations should use <see cref="GlobalSettings.DistributedLockingReadLockDefaultTimeout"/>.
    /// </remarks>
    IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null);

    /// <summary>
    /// Obtains a distributed read lock.
    /// </summary>
    /// <remarks>
    /// When timeout is null, implementations should use <see cref="GlobalSettings.DistributedLockingWriteLockDefaultTimeout"/>.
    /// </remarks>
    IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null);
}
