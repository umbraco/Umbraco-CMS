using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Locking;

/// <summary>
/// Implements distributed locking for SQL Server databases using EF Core.
/// </summary>
/// <typeparam name="T">The type of DbContext.</typeparam>
internal sealed class SqlServerEFCoreDistributedLockingMechanism<T> : IDistributedLockingMechanism
    where T : DbContext
{
    private ConnectionStrings _connectionStrings;
    private GlobalSettings _globalSettings;
    private readonly ILogger<SqlServerEFCoreDistributedLockingMechanism<T>> _logger;
    private readonly Lazy<IEFCoreScopeAccessor<T>> _scopeAccessor; // Hooray it's a circular dependency.

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerEFCoreDistributedLockingMechanism{T}"/> class.
    /// </summary>
    public SqlServerEFCoreDistributedLockingMechanism(
        ILogger<SqlServerEFCoreDistributedLockingMechanism<T>> logger,
        Lazy<IEFCoreScopeAccessor<T>> scopeAccessor,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        _logger = logger;
        _scopeAccessor = scopeAccessor;
        _globalSettings = globalSettings.CurrentValue;
        _connectionStrings = connectionStrings.CurrentValue;
        globalSettings.OnChange(x=>_globalSettings = x);
        connectionStrings.OnChange(x=>_connectionStrings = x);
    }

    /// <inheritdoc />
    public bool HasActiveRelatedScope => _scopeAccessor.Value.AmbientScope is not null;

    /// <inheritdoc />
    public bool Enabled => _connectionStrings.IsConnectionStringConfigured() &&
                           string.Equals(_connectionStrings.ProviderName, "Microsoft.Data.SqlClient", StringComparison.InvariantCultureIgnoreCase) && _scopeAccessor.Value.AmbientScope is not null;

    /// <inheritdoc />
    public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null)
    {
        obtainLockTimeout ??= _globalSettings.DistributedLockingReadLockDefaultTimeout;
        return new SqlServerDistributedLock(this, lockId, DistributedLockType.ReadLock, obtainLockTimeout.Value);
    }

    /// <inheritdoc />
    public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null)
    {
        obtainLockTimeout ??= _globalSettings.DistributedLockingWriteLockDefaultTimeout;
        return new SqlServerDistributedLock(this, lockId, DistributedLockType.WriteLock, obtainLockTimeout.Value);
    }

    /// <summary>
    /// Represents a distributed lock for SQL Server databases.
    /// </summary>
    private sealed class SqlServerDistributedLock : IDistributedLock
    {
        /// <summary>
        ///     The SQL Server error number reported when the server gives up waiting for a lock, having
        ///     waited for the period set by <c>SET LOCK_TIMEOUT</c>.
        /// </summary>
        private const int LockRequestTimeoutError = 1222;

        /// <summary>
        ///     The SQL Server error number reported when the client gives up waiting for the command,
        ///     having waited for its command timeout.
        /// </summary>
        /// <remarks>
        ///     The lock statement runs under a command timeout derived from the lock timeout, so a command
        ///     that times out obtaining the lock is reported as a lock timeout too, rather than escaping
        ///     the mechanism untranslated.
        /// </remarks>
        private const int CommandTimeoutError = -2;

        private readonly SqlServerEFCoreDistributedLockingMechanism<T> _parent;
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDistributedLock"/> class.
        /// </summary>
        /// <param name="parent">The parent locking mechanism.</param>
        /// <param name="lockId">The lock identifier.</param>
        /// <param name="lockType">The type of lock.</param>
        /// <param name="timeout">The timeout for obtaining the lock.</param>
        public SqlServerDistributedLock(
            SqlServerEFCoreDistributedLockingMechanism<T> parent,
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
            catch (SqlException ex) when (ex.Number is LockRequestTimeoutError or CommandTimeoutError)
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
        public void Dispose() =>
            _parent._logger.LogDebug("Dropped {lockType} for id {id}", LockType, LockId);

        /// <inheritdoc />
        public override string ToString()
            => $"SqlServerDistributedLock({LockId}, {LockType}";

        private void ObtainReadLock()
        {
            IEfCoreScope<T>? scope = _parent._scopeAccessor.Value.AmbientScope;

            if (scope is null)
            {
                throw new PanicException("No ambient scope");
            }

            scope.ExecuteWithContextAsync<Task>(async dbContext =>
            {
                if (dbContext.Database.CurrentTransaction is null)
                {
                    throw new InvalidOperationException(
                        "SqlServerDistributedLockingMechanism requires a transaction to function.");
                }

                if (dbContext.Database.CurrentTransaction.GetDbTransaction().IsolationLevel <
                    IsolationLevel.ReadCommitted)
                {
                    throw new InvalidOperationException(
                        "A transaction with minimum ReadCommitted isolation level is required.");
                }

                // This path can pass the timeout straight to the command, so it needs no save and restore.
                var number = await dbContext.Database.ExecuteScalarAsync<int?>(
                    $"SET LOCK_TIMEOUT {(int)_timeout.TotalMilliseconds};SELECT value FROM dbo.umbracoLock WITH (ROWLOCK, REPEATABLEREAD) WHERE id=@id",
                    [new SqlParameter("@id", LockId)],
                    commandTimeOut: TimeSpan.FromSeconds(CommandTimeoutSeconds));

                if (number == null)
                {
                    // ensure we are actually locking!
                    throw new ArgumentException(@$"LockObject with id={LockId} does not exist.", nameof(LockId));
                }
            }).GetAwaiter().GetResult();
        }

        private void ObtainWriteLock()
        {
            IEfCoreScope<T>? scope = _parent._scopeAccessor.Value.AmbientScope;
            if (scope is null)
            {
                throw new PanicException("No ambient scope");
            }

            scope.ExecuteWithContextAsync<Task>(async dbContext =>
            {
                if (dbContext.Database.CurrentTransaction is null)
                {
                    throw new InvalidOperationException(
                        "SqlServerDistributedLockingMechanism requires a transaction to function.");
                }

                if (dbContext.Database.CurrentTransaction.GetDbTransaction().IsolationLevel < IsolationLevel.ReadCommitted)
                {
                    throw new InvalidOperationException(
                        "A transaction with minimum ReadCommitted isolation level is required.");
                }

                // Unlike the read lock, this path has no per-command timeout hook, so the context-wide
                // one has to be set and put back.
                int? originalCommandTimeout = dbContext.Database.GetCommandTimeout();
                dbContext.Database.SetCommandTimeout(CommandTimeoutSeconds);

                int rowsAffected;
                try
                {
                    // S2077: SET LOCK_TIMEOUT only accepts a literal, so the timeout cannot be a
                    // parameter. It is an int, and the lock id is parameterized, so no part of the
                    // statement comes from a string.
#pragma warning disable EF1002, S2077
                    rowsAffected = await dbContext.Database.ExecuteSqlRawAsync(
                        @$"SET LOCK_TIMEOUT {(int)_timeout.TotalMilliseconds};UPDATE umbracoLock WITH (ROWLOCK, REPEATABLEREAD) SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id={{0}}",
                        LockId);
#pragma warning restore EF1002, S2077
                }
                finally
                {
                    dbContext.Database.SetCommandTimeout(originalCommandTimeout);
                }

                if (rowsAffected == 0)
                {
                    // ensure we are actually locking!
                    throw new ArgumentException($"LockObject with id={LockId} does not exist.");
                }
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Gets the command timeout, in whole seconds, that the statement obtaining this lock runs
        ///     under, derived from the lock's own timeout.
        /// </summary>
        /// <remarks>
        ///     Without this the ambient command timeout can abort the statement while the server is still
        ///     waiting for the row lock, surfacing a raw timeout instead of a lock timeout exception.
        /// </remarks>
        private int CommandTimeoutSeconds => _timeout.ToLockCommandTimeoutSeconds();
    }
}
