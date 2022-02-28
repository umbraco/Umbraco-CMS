using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.DistributedLocking.Exceptions;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
/// SQL Server implementation of of <see cref="IDistributedLockingMechanism"/>.
/// </summary>
public class SqlServerDistributedLockingMechanism : IDistributedLockingMechanism
{
    private readonly ILogger<SqlServerDistributedLockingMechanism> _logger;
    private readonly IUmbracoDatabaseFactory _dbFactory;
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDistributedLockingMechanism"/> class.
    /// </summary>
    public SqlServerDistributedLockingMechanism(
        ILogger<SqlServerDistributedLockingMechanism> logger,
        IUmbracoDatabaseFactory dbFactory,
        IOptionsMonitor<GlobalSettings> globalSettings)
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _globalSettings = globalSettings;
    }

    /// <inheritdoc />
    public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null)
    {
        obtainLockTimeout ??= _globalSettings.CurrentValue.DistributedLockingReadLockDefaultTimeout;
        return new SqlServerDistributedLock(this, lockId, DistributedLockType.ReadLock, obtainLockTimeout.Value);
    }

    /// <inheritdoc />
    public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null)
    {
        obtainLockTimeout ??= _globalSettings.CurrentValue.DistributedLockingWriteLockDefaultTimeout;
        return new SqlServerDistributedLock(this, lockId, DistributedLockType.WriteLock, obtainLockTimeout.Value);
    }

    private class SqlServerDistributedLock : IDistributedLock
    {
        private readonly SqlServerDistributedLockingMechanism _parent;
        private readonly TimeSpan _timeout;
        private readonly IUmbracoDatabase _db;

        public SqlServerDistributedLock(
            SqlServerDistributedLockingMechanism parent,
            int lockId,
            DistributedLockType lockType,
            TimeSpan timeout)
        {
            _db = parent._dbFactory.CreateDatabase();
            _parent = parent;
            _timeout = timeout;
            LockId = lockId;
            LockType = lockType;

            _db.BeginTransaction(IsolationLevel.RepeatableRead);
            _db.Execute("SET LOCK_TIMEOUT " + _timeout.TotalMilliseconds + ";");

            _parent._logger.LogDebug("{lockType} requested for id {id}", LockType, LockId);

            if (LockType == DistributedLockType.ReadLock)
            {
                ObtainReadLock();
            }
            else
            {
                ObtainWriteLock();
            }

            _parent._logger.LogDebug("Acquired {lockType} for id {id}", LockType, LockId);
        }

        public int LockId { get; }

        public DistributedLockType LockType { get; }

        public void Dispose()
            => Release();

        public void Release()
        {
            _db.CompleteTransaction();
            _db.Dispose();
            _parent._logger.LogDebug("Dropped {lockType} for id {id}", LockType, LockId);
        }

        public override string ToString()
            => $"SqlServerDistributedLock({LockId}, {LockType}";

        private void ObtainReadLock()
        {
            try
            {
                const string query = "SELECT value FROM umbracoLock WHERE id=@id";

                var i = _db.ExecuteScalar<int?>(query, new { id = LockId });

                if (i == null)
                {
                    // ensure we are actually locking!
                    throw new ArgumentException($"LockObject with id={LockId} does not exist.", nameof(LockId));
                }
            }
            catch (SqlException ex)
            {
                if (ex?.Number == 1222)
                {
                    throw new DistributedReadLockTimeoutException(LockId);
                }

                throw;
            }
        }

        private void ObtainWriteLock()
        {
            try
            {
                const string query = @"UPDATE umbracoLock SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id=@id";

                var i = _db.Execute(query, new {id = LockId});

                if (i == 0)
                {
                    // ensure we are actually locking!
                    throw new ArgumentException($"LockObject with id={LockId} does not exist.");
                }
            }
            catch (SqlException ex)
            {
                if (ex?.Number == 1222)
                {
                    throw new DistributedReadLockTimeoutException(LockId);
                }

                throw;
            }
        }
    }
}
