using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SQLitePCL;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Locking;

/// <summary>
/// Implements distributed locking for SQLite databases using EF Core.
/// </summary>
/// <typeparam name="T">The type of DbContext.</typeparam>
internal sealed class SqliteEFCoreDistributedLockingMechanism<T> : IDistributedLockingMechanism
    where T : DbContext
{
    private readonly ILogger<SqliteEFCoreDistributedLockingMechanism<T>> _logger;
    private readonly Lazy<IEFCoreScopeAccessor<T>> _efCoreScopeAccessor;
    private GlobalSettings _globalSettings;
    private ConnectionStrings _connectionStrings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteEFCoreDistributedLockingMechanism{T}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="efCoreScopeAccessor">The EF Core scope accessor.</param>
    /// <param name="globalSettings">The global settings monitor.</param>
    /// <param name="connectionStrings">The connection strings monitor.</param>
    public SqliteEFCoreDistributedLockingMechanism(
        ILogger<SqliteEFCoreDistributedLockingMechanism<T>> logger,
        Lazy<IEFCoreScopeAccessor<T>> efCoreScopeAccessor,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        _logger = logger;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _globalSettings = globalSettings.CurrentValue;
        _connectionStrings = connectionStrings.CurrentValue;

        globalSettings.OnChange(x => _globalSettings = x);
        connectionStrings.OnChange(x => _connectionStrings = x);
    }

    /// <inheritdoc />
    public bool HasActiveRelatedScope => _efCoreScopeAccessor.Value.AmbientScope is not null;

    /// <inheritdoc />
    public bool Enabled
        => _connectionStrings.IsConnectionStringConfigured() &&
        string.Equals(_connectionStrings.ProviderName, Constants.ProviderNames.SQLLite, StringComparison.InvariantCultureIgnoreCase) &&
        _efCoreScopeAccessor.Value.AmbientScope is not null;

    /// <inheritdoc />
    /// <remarks>With journal_mode=wal we can always read a snapshot.</remarks>
    public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null)
        => new SqliteDistributedLock(this, lockId, DistributedLockType.ReadLock, obtainLockTimeout ?? _globalSettings.DistributedLockingReadLockDefaultTimeout);

    /// <inheritdoc />
    /// <remarks>With journal_mode=wal only a single write transaction can exist at a time.</remarks>
    public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null)
        => new SqliteDistributedLock(this, lockId, DistributedLockType.WriteLock, obtainLockTimeout ?? _globalSettings.DistributedLockingWriteLockDefaultTimeout);

    /// <summary>
    /// Represents a distributed lock for SQLite databases.
    /// </summary>
    private sealed class SqliteDistributedLock : IDistributedLock
    {
        private readonly SqliteEFCoreDistributedLockingMechanism<T> _parent;
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDistributedLock"/> class.
        /// </summary>
        /// <param name="parent">The parent locking mechanism.</param>
        /// <param name="lockId">The lock identifier.</param>
        /// <param name="lockType">The type of lock.</param>
        /// <param name="timeout">The timeout for obtaining the lock.</param>
        public SqliteDistributedLock(
            SqliteEFCoreDistributedLockingMechanism<T> parent,
            int lockId,
            DistributedLockType lockType,
            TimeSpan timeout)
        {
            _parent = parent;
            _timeout = timeout;
            LockId = lockId;
            LockType = lockType;

            _parent._logger.LogDebug("Requesting {lockType} for id {id}", LockType, LockId);

            try
            {
                switch (lockType)
                {
                    case DistributedLockType.ReadLock:
                        ObtainReadLock();
                        break;
                    case DistributedLockType.WriteLock:
                        ObtainWriteLock();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(lockType), lockType, @"Unsupported lockType");
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                if (LockType == DistributedLockType.ReadLock)
                {
                    throw new DistributedReadLockTimeoutException(LockId);
                }

                throw new DistributedWriteLockTimeoutException(LockId);
            }

            _parent._logger.LogDebug("Acquired {lockType} for id {id}", LockType, LockId);
        }

        /// <inheritdoc />
        public int LockId { get; }

        /// <inheritdoc />
        public DistributedLockType LockType { get; }

        /// <inheritdoc />
        /// <remarks>Mostly no-op, cleaned up by completing transaction in scope.</remarks>
        public void Dispose()
            => _parent._logger.LogDebug("Dropped {lockType} for id {id}", LockType, LockId);

        /// <inheritdoc />
        public override string ToString()
            => $"SqliteDistributedLock({LockId})";

        // Can always obtain a read lock (snapshot isolation in wal mode)
        // Mostly no-op just check that we didn't end up ReadUncommitted for real.
        private void ObtainReadLock()
        {
            IEfCoreScope<T>? efCoreScope = _parent._efCoreScopeAccessor.Value.AmbientScope
                ?? throw new PanicException("No current ambient scope");

            efCoreScope.ExecuteWithContextAsync<Task>(database =>
            {
                if (database.Database.CurrentTransaction is null)
                {
                    throw new InvalidOperationException("SqliteDistributedLockingMechanism requires a transaction to function.");
                }

                return Task.CompletedTask;
            });
        }

        // Only one writer is possible at a time
        // lock occurs for entire database as opposed to row/table.
        private void ObtainWriteLock()
        {
            IEfCoreScope<T>? efCoreScope = _parent._efCoreScopeAccessor.Value.AmbientScope
                ?? throw new PanicException("No ambient scope");

            efCoreScope.ExecuteWithContextAsync<Task>(async database =>
            {
                if (database.Database.CurrentTransaction is null)
                {
                    throw new InvalidOperationException("SqliteDistributedLockingMechanism requires a transaction to function.");
                }

                var query = @$"UPDATE umbracoLock SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id = {LockId.ToString(CultureInfo.InvariantCulture)}";

                try
                {
                    // imagine there is an existing writer, whilst elapsed time is < command timeout sqlite will busy loop
                    // Important to note that if this value == 0 then Command.DefaultTimeout (30s) is used.
                    // Math.Ceiling such that (0 < totalseconds < 1) is rounded up to 1.
                    database.Database.SetCommandTimeout((int)Math.Ceiling(_timeout.TotalSeconds));
                    var i = await database.Database.ExecuteScalarAsync<int>(query);

                    if (i == 0)
                    {
                        // ensure we are actually locking!
                        throw new ArgumentException($"LockObject with id={LockId} does not exist.");
                    }
                }
                catch (SqliteException ex) when (IsBusyOrLocked(ex))
                {
                    throw new DistributedWriteLockTimeoutException(LockId);
                }
            });
        }

        private static bool IsBusyOrLocked(SqliteException ex) =>
            ex.SqliteErrorCode
                is raw.SQLITE_BUSY
                or raw.SQLITE_LOCKED
                or raw.SQLITE_LOCKED_SHAREDCACHE;
    }
}
