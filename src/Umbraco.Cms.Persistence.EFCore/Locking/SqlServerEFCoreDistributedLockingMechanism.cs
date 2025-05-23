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
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Locking;

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

    private sealed class SqlServerDistributedLock : IDistributedLock
    {
        private readonly SqlServerEFCoreDistributedLockingMechanism<T> _parent;
        private readonly TimeSpan _timeout;

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
            catch (SqlException ex) when (ex.Number == 1222)
            {
                if (LockType == DistributedLockType.ReadLock)
                {
                    throw new DistributedReadLockTimeoutException(LockId);
                }

                throw new DistributedWriteLockTimeoutException(LockId);
            }

            _parent._logger.LogDebug("Acquired {lockType} for id {id}", LockType, LockId);
        }

        public int LockId { get; }

        public DistributedLockType LockType { get; }

        public void Dispose() =>
            // Mostly no op, cleaned up by completing transaction in scope.
            _parent._logger.LogDebug("Dropped {lockType} for id {id}", LockType, LockId);

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

                var number = await dbContext.Database.ExecuteScalarAsync<int?>($"SET LOCK_TIMEOUT {(int)_timeout.TotalMilliseconds};SELECT value FROM dbo.umbracoLock WITH (REPEATABLEREAD) WHERE id={LockId}");

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

#pragma warning disable EF1002
                var rowsAffected = await dbContext.Database.ExecuteSqlRawAsync(@$"SET LOCK_TIMEOUT {(int)_timeout.TotalMilliseconds};UPDATE umbracoLock WITH (REPEATABLEREAD) SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id={LockId}");
#pragma warning restore EF1002

                if (rowsAffected == 0)
                {
                    // ensure we are actually locking!
                    throw new ArgumentException($"LockObject with id={LockId} does not exist.");
                }
            }).GetAwaiter().GetResult();
        }
    }
}
