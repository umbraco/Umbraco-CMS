using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
/// SQL Server implementation of <see cref="IDistributedLockingMechanism"/>.
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
        if (dbFactory.SqlContext.DatabaseType is not NPoco.DatabaseTypes.SqlServerDatabaseType)
        {
            throw new DistributedLockingException($"Invalid database type {dbFactory.SqlContext.DatabaseType}");
        }

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
        private readonly IUmbracoDatabase _db;

        public SqlServerDistributedLock(
            SqlServerDistributedLockingMechanism parent,
            int lockId,
            DistributedLockType lockType,
            TimeSpan timeout)
        {
            _db = parent._dbFactory.CreateDatabase();
            _parent = parent;
            LockId = lockId;
            LockType = lockType;

            _db.BeginTransaction(IsolationLevel.RepeatableRead);
            _db.Execute("SET LOCK_TIMEOUT " + timeout.TotalMilliseconds + ";");

            _parent._logger.LogDebug("{lockType} requested for id {id}", LockType, LockId);

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
                _db.AbortTransaction();
                _db.Dispose();
                throw new DistributedReadLockTimeoutException(LockId);
            }

            _parent._logger.LogDebug("Acquired {lockType} for id {id}", LockType, LockId);
        }

        public int LockId { get; }

        public DistributedLockType LockType { get; }

        public void Dispose()
        {
            if (_db.InTransaction)
            {
                try
                {
                    _db.CompleteTransaction();
                }
                catch (Exception ex)
                {
                    throw new DistributedLockingException($"Unexpected exception thrown whilst attempting to release {LockType} for id {LockId}.", ex);
                }
            }

            _db.Dispose();
            _parent._logger.LogDebug("Dropped {lockType} for id {id}", LockType, LockId);
        }

        public override string ToString()
            => $"SqlServerDistributedLock({LockId}, {LockType}";

        private void ObtainReadLock()
        {
            const string query = "SELECT value FROM umbracoLock WHERE id=@id";

            var i = _db.ExecuteScalar<int?>(query, new {id = LockId});

            if (i == null)
            {
                // ensure we are actually locking!
                throw new ArgumentException(@$"LockObject with id={LockId} does not exist.", nameof(LockId));
            }
        }

        private void ObtainWriteLock()
        {
            const string query = @"UPDATE umbracoLock SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id=@id";

            var i = _db.Execute(query, new {id = LockId});

            if (i == 0)
            {
                // ensure we are actually locking!
                throw new ArgumentException($"LockObject with id={LockId} does not exist.");
            }
        }
    }
}
