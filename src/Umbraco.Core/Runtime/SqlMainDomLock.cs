using NPoco;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using MapperCollection = Umbraco.Core.Persistence.Mappers.MapperCollection;

namespace Umbraco.Core.Runtime
{
    public class SqlMainDomLock : IMainDomLock
    {
        private readonly TimeSpan _lockTimeout;
        private string _lockId;
        private const string MainDomKeyPrefix = "Umbraco.Core.Runtime.SqlMainDom";
        private const string UpdatedSuffix = "_updated";
        private readonly ILogger _logger;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private SqlServerSyntaxProvider _sqlServerSyntax = new SqlServerSyntaxProvider();
        private bool _mainDomChanging = false;
        private readonly UmbracoDatabaseFactory _dbFactory;
        private bool _errorDuringAcquiring;
        private object _locker = new object();
        private bool _hasTable = false;

        public SqlMainDomLock(ILogger logger, string connectionStringName = Constants.System.UmbracoConnectionName)
        {
            // unique id for our appdomain, this is more unique than the appdomain id which is just an INT counter to its safer
            _lockId = Guid.NewGuid().ToString();
            _logger = logger;

            _dbFactory = new UmbracoDatabaseFactory(
               connectionStringName,
               _logger,
               new Lazy<IMapperCollection>(() => new MapperCollection(Enumerable.Empty<BaseMapper>())));

            _lockTimeout = TimeSpan.FromMilliseconds(GlobalSettings.GetSqlWriteLockTimeoutFromConfigFile(logger));
        }

        public async Task<bool> AcquireLockAsync(int millisecondsTimeout)
        {
            if (!_dbFactory.Configured)
            {
                // if we aren't configured then we're in an install state, in which case we have no choice but to assume we can acquire
                return true;
            }

            if (!(_dbFactory.SqlContext.SqlSyntax is SqlServerSyntaxProvider sqlServerSyntaxProvider))
            {
                throw new NotSupportedException("SqlMainDomLock is only supported for Sql Server");
            }

            _sqlServerSyntax = sqlServerSyntaxProvider;

            _logger.Debug<SqlMainDomLock>("Acquiring lock...");

            var tempId = Guid.NewGuid().ToString();

            IUmbracoDatabase db = null;

            try
            {
                db = _dbFactory.CreateDatabase();

                _hasTable = db.HasTable(Constants.DatabaseSchema.Tables.KeyValue);
                if (!_hasTable)
                {
                    // the Db does not contain the required table, we must be in an install state we have no choice but to assume we can acquire
                    return true;
                }

                db.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    // wait to get a write lock
                    _sqlServerSyntax.WriteLock(db, TimeSpan.FromMilliseconds(millisecondsTimeout), Constants.Locks.MainDom);
                }
                catch (SqlException ex)
                {
                    if (IsLockTimeoutException(ex))
                    {
                        _logger.Error<SqlMainDomLock>(ex, "Sql timeout occurred, could not acquire MainDom.");
                        _errorDuringAcquiring = true;
                        return false;
                    }

                    // unexpected (will be caught below)
                    throw;
                }

                var result = InsertLockRecord(tempId, db); //we change the row to a random Id to signal other MainDom to shutdown
                if (result == RecordPersistenceType.Insert)
                {
                    // if we've inserted, then there was no MainDom so we can instantly acquire

                    InsertLockRecord(_lockId, db); // so update with our appdomain id
                    _logger.Debug<SqlMainDomLock, string>("Acquired with ID {LockId}", _lockId);
                    return true;
                }

                // if we've updated, this means there is an active MainDom, now we need to wait to
                // for the current MainDom to shutdown which also requires releasing our write lock
            }
            catch (Exception ex)
            {
                // unexpected
                _logger.Error<SqlMainDomLock>(ex, "Unexpected error, cannot acquire MainDom");
                _errorDuringAcquiring = true;
                return false;
            }
            finally
            {
                db?.CompleteTransaction();
                db?.Dispose();
            }


            return await WaitForExistingAsync(tempId, millisecondsTimeout).ConfigureAwait(false);
        }

        public Task ListenAsync()
        {
            if (_errorDuringAcquiring)
            {
                _logger.Warn<SqlMainDomLock>("Could not acquire MainDom, listening is canceled.");
                return Task.CompletedTask;
            }

            // Create a long running task (dedicated thread)
            // to poll to check if we are still the MainDom registered in the DB
            using (ExecutionContext.SuppressFlow())
            {
                return Task.Factory.StartNew(
                    ListeningLoop,
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
                    TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Returns the keyvalue table key for the current server/app
        /// </summary>
        /// <remarks>
        /// The key is the the normal MainDomId which takes into account the AppDomainAppId and the physical file path of the app and this is
        /// combined with the current machine name. The machine name is required because the default semaphore lock is machine wide so it implicitly
        /// takes into account machine name whereas this needs to be explicitly per machine.
        /// </remarks>
        private string MainDomKey { get; } = MainDomKeyPrefix + "-" + (NetworkHelper.MachineName + MainDom.GetMainDomId()).GenerateHash<SHA1>();

        private void ListeningLoop()
        {
            while (true)
            {
                // poll every couple of seconds
                // local testing shows the actual query to be executed from client/server is approx 300ms but would change depending on environment/IO
                Thread.Sleep(2000);

                if (!_dbFactory.Configured)
                {
                    // if we aren't configured, we just keep looping since we can't query the db
                    continue;
                }

                lock (_locker)
                {
                    // If cancellation has been requested we will just exit. Depending on timing of the shutdown,
                    // we will have already flagged _mainDomChanging = true, or we're shutting down faster than
                    // the other MainDom is taking to startup. In this case the db row will just be deleted and the
                    // new MainDom will just take over.
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        _logger.Debug<SqlMainDomLock>("Task canceled, exiting loop");
                        return;
                    }
                    IUmbracoDatabase db = null;

                    try
                    {
                        db = _dbFactory.CreateDatabase();

                        if (!_hasTable)
                        {
                            // re-check if its still false, we don't want to re-query once we know its there since this
                            // loop needs to use minimal resources
                            _hasTable = db.HasTable(Constants.DatabaseSchema.Tables.KeyValue);
                            if (!_hasTable)
                            {
                                // the Db does not contain the required table, we just keep looping since we can't query the db
                                continue;
                            }
                        }

                        db.BeginTransaction(IsolationLevel.ReadCommitted);
                        // get a read lock
                        _sqlServerSyntax.ReadLock(db, _lockTimeout, Constants.Locks.MainDom);

                        if (!IsMainDomValue(_lockId, db))
                        {
                            // we are no longer main dom, another one has come online, exit
                            _mainDomChanging = true;
                            _logger.Debug<SqlMainDomLock>("Detected new booting application, releasing MainDom lock.");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error<SqlMainDomLock>(ex, "Unexpected error during listening.");

                        // We need to keep on listening unless we've been notified by our own AppDomain to shutdown since
                        // we don't want to shutdown resources controlled by MainDom inadvertently. We'll just keep listening otherwise.
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            _logger.Debug<SqlMainDomLock>("Task canceled, exiting loop");
                            return;
                        }
                    }
                    finally
                    {
                        // Even if any of the above fail like BeginTransaction, or even a query after the
                        // Transaction is started, the calls below will not throw. I've tried all sorts of
                        // combinations to see if I can make this throw but I can't. In any case, we'll be
                        // extra safe and try/catch/log
                        try
                        {
                            db?.CompleteTransaction();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<SqlMainDomLock>(ex, "Unexpected error completing transaction.");
                        }

                        try
                        {
                            db?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<SqlMainDomLock>(ex, "Unexpected error completing disposing.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Wait for any existing MainDom to release so we can continue booting
        /// </summary>
        /// <param name="tempId"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        private Task<bool> WaitForExistingAsync(string tempId, int millisecondsTimeout)
        {
            var updatedTempId = tempId + UpdatedSuffix;

            using (ExecutionContext.SuppressFlow())
            {
                return Task.Run(() =>
                {
                    try
                    {
                        using var db = _dbFactory.CreateDatabase();

                        var watch = new Stopwatch();
                        watch.Start();
                        while (true)
                        {
                            // poll very often, we need to take over as fast as we can
                            // local testing shows the actual query to be executed from client/server is approx 300ms but would change depending on environment/IO
                            Thread.Sleep(1000);

                            var acquired = TryAcquire(db, tempId, updatedTempId);
                            if (acquired.HasValue)
                                return acquired.Value;

                            if (watch.ElapsedMilliseconds >= millisecondsTimeout)
                            {
                                return AcquireWhenMaxWaitTimeElapsed(db);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error<SqlMainDomLock>(ex, "An error occurred trying to acquire and waiting for existing SqlMainDomLock to shutdown");
                        return false;
                    }

                }, _cancellationTokenSource.Token);
            }
        }

        private bool? TryAcquire(IUmbracoDatabase db, string tempId, string updatedTempId)
        {
            // Creates a separate transaction to the DB instance so we aren't allocating tons of new DB instances for each transaction
            // since this is executed in a tight loop

            ITransaction transaction = null;

            try
            {
                transaction = db.GetTransaction(IsolationLevel.ReadCommitted);
                // get a read lock
                _sqlServerSyntax.ReadLock(db, _lockTimeout, Constants.Locks.MainDom);

                // the row
                var mainDomRows = db.Fetch<KeyValueDto>("SELECT * FROM umbracoKeyValue WHERE [key] = @key", new { key = MainDomKey });

                if (mainDomRows.Count == 0 || mainDomRows[0].Value == updatedTempId)
                {
                    // the other main dom has updated our record
                    // Or the other maindom shutdown super fast and just deleted the record
                    // which indicates that we
                    // can acquire it and it has shutdown.

                    _sqlServerSyntax.WriteLock(db, _lockTimeout, Constants.Locks.MainDom);

                    // so now we update the row with our appdomain id
                    InsertLockRecord(_lockId, db);
                    _logger.Debug<SqlMainDomLock, string>("Acquired with ID {LockId}", _lockId);
                    return true;
                }
                else if (mainDomRows.Count == 1 && !mainDomRows[0].Value.StartsWith(tempId))
                {
                    // in this case, the prefixed ID is different which  means
                    // another new AppDomain has come online and is wanting to take over. In that case, we will not
                    // acquire.

                    _logger.Debug<SqlMainDomLock>("Cannot acquire, another booting application detected.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (IsLockTimeoutException(ex as SqlException))
                {
                    _logger.Error<SqlMainDomLock>(ex, "Sql timeout occurred, waiting for existing MainDom is canceled.");
                    _errorDuringAcquiring = true;
                    return false;
                }
                // unexpected
                _logger.Error<SqlMainDomLock>(ex, "Unexpected error, waiting for existing MainDom is canceled.");
                _errorDuringAcquiring = true;
                return false;
            }
            finally
            {
                transaction?.Complete();
                transaction?.Dispose();
            }

            return null; // continue
        }

        private bool AcquireWhenMaxWaitTimeElapsed(IUmbracoDatabase db)
        {
            // Creates a separate transaction to the DB instance so we aren't allocating tons of new DB instances for each transaction
            // since this is executed in a tight loop

            // if the timeout has elapsed, it either means that the other main dom is taking too long to shutdown,
            // or it could mean that the previous appdomain was terminated and didn't clear out the main dom SQL row
            // and it's just been left as an orphan row.
            // There's really know way of knowing unless we are constantly updating the row for the current maindom
            // which isn't ideal.
            // So... we're going to 'just' take over, if the writelock works then we'll assume we're ok

            _logger.Debug<SqlMainDomLock>("Timeout elapsed, assuming orphan row, acquiring MainDom.");

            ITransaction transaction = null;

            try
            {
                transaction = db.GetTransaction(IsolationLevel.ReadCommitted);

                _sqlServerSyntax.WriteLock(db, _lockTimeout, Constants.Locks.MainDom);

                // so now we update the row with our appdomain id
                InsertLockRecord(_lockId, db);
                _logger.Debug<SqlMainDomLock, string>("Acquired with ID {LockId}", _lockId);
                return true;
            }
            catch (Exception ex)
            {
                if (IsLockTimeoutException(ex as SqlException))
                {
                    // something is wrong, we cannot acquire, not much we can do
                    _logger.Error<SqlMainDomLock>(ex, "Sql timeout occurred, could not forcibly acquire MainDom.");
                    _errorDuringAcquiring = true;
                    return false;
                }
                _logger.Error<SqlMainDomLock>(ex, "Unexpected error, could not forcibly acquire MainDom.");
                _errorDuringAcquiring = true;
                return false;
            }
            finally
            {
                transaction?.Complete();
                transaction?.Dispose();
            }
        }

        /// <summary>
        /// Inserts or updates the key/value row
        /// </summary>
        private RecordPersistenceType InsertLockRecord(string id, IUmbracoDatabase db)
        {
            return db.InsertOrUpdate(new KeyValueDto
            {
                Key = MainDomKey,
                Value = id,
                Updated = DateTime.Now
            });
        }

        /// <summary>
        /// Checks if the DB row value is equals the value
        /// </summary>
        /// <returns></returns>
        private bool IsMainDomValue(string val, IUmbracoDatabase db)
        {
            return db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoKeyValue WHERE [key] = @key AND [value] = @val",
                new { key = MainDomKey, val = val }) == 1;
        }

        /// <summary>
        /// Checks if the exception is an SQL timeout
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private bool IsLockTimeoutException(SqlException sqlException) => sqlException?.Number == 1222;

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (_locker)
                    {
                        _logger.Debug<SqlMainDomLock>($"{nameof(SqlMainDomLock)} Disposing...");

                        // immediately cancel all sub-tasks, we don't want them to keep querying
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();

                        if (_dbFactory.Configured && _hasTable)
                        {
                            IUmbracoDatabase db = null;
                            try
                            {
                                db = _dbFactory.CreateDatabase();
                                db.BeginTransaction(IsolationLevel.ReadCommitted);

                                // get a write lock
                                _sqlServerSyntax.WriteLock(db, _lockTimeout, Constants.Locks.MainDom);

                                // When we are disposed, it means we have released the MainDom lock
                                // and called all MainDom release callbacks, in this case
                                // if another maindom is actually coming online we need
                                // to signal to the MainDom coming online that we have shutdown.
                                // To do that, we update the existing main dom DB record with a suffixed "_updated" string.
                                // Otherwise, if we are just shutting down, we want to just delete the row.
                                if (_mainDomChanging)
                                {
                                    _logger.Debug<SqlMainDomLock>("Releasing MainDom, updating row, new application is booting.");
                                    var count = db.Execute($"UPDATE umbracoKeyValue SET [value] = [value] + '{UpdatedSuffix}' WHERE [key] = @key", new { key = MainDomKey });
                                }
                                else
                                {
                                    _logger.Debug<SqlMainDomLock>("Releasing MainDom, deleting row, application is shutting down.");
                                    var count = db.Execute("DELETE FROM umbracoKeyValue WHERE [key] = @key", new { key = MainDomKey });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error<SqlMainDomLock>(ex, "Unexpected error during dipsose.");
                            }
                            finally
                            {
                                try
                                {
                                    db?.CompleteTransaction();
                                    db?.Dispose();
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error<SqlMainDomLock>(ex, "Unexpected error during dispose when completing transaction.");
                                }
                            }
                        }
                    }
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
