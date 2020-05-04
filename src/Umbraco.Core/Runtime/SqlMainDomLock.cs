using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Runtime
{
    internal class SqlMainDomLock : IMainDomLock
    {
        private string _lockId;
        private const string MainDomKeyPrefix = "Umbraco.Core.Runtime.SqlMainDom";
        private const string UpdatedSuffix = "_updated";
        private readonly ILogger _logger;
        private IUmbracoDatabase _db;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private SqlServerSyntaxProvider _sqlServerSyntax = new SqlServerSyntaxProvider();
        private bool _mainDomChanging = false;
        private readonly UmbracoDatabaseFactory _dbFactory;
        private bool _hasError;
        private object _locker = new object();

        public SqlMainDomLock(ILogger logger)
        {
            // unique id for our appdomain, this is more unique than the appdomain id which is just an INT counter to its safer
            _lockId = Guid.NewGuid().ToString();
            _logger = logger;
            
            _dbFactory = new UmbracoDatabaseFactory(
               Constants.System.UmbracoConnectionName,
               _logger,
               new Lazy<IMapperCollection>(() => new Persistence.Mappers.MapperCollection(Enumerable.Empty<BaseMapper>())));
        }

        public async Task<bool> AcquireLockAsync(int millisecondsTimeout)
        {
            if (!_dbFactory.Configured)
            {
                // if we aren't configured, then we're in an install state, in which case we have no choice but to assume we can acquire
                return true;
            }

            if (!(_dbFactory.SqlContext.SqlSyntax is SqlServerSyntaxProvider sqlServerSyntaxProvider))
                throw new NotSupportedException("SqlMainDomLock is only supported for Sql Server");

            _sqlServerSyntax = sqlServerSyntaxProvider;

            _logger.Debug<SqlMainDomLock>("Acquiring lock...");

            var db = GetDatabase();

            var tempId = Guid.NewGuid().ToString();

            try
            {
                db.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    // wait to get a write lock
                    _sqlServerSyntax.WriteLock(db, TimeSpan.FromMilliseconds(millisecondsTimeout), Constants.Locks.MainDom);                    
                }
                catch (Exception ex)
                {
                    if (IsLockTimeoutException(ex))
                    {
                        _logger.Error<SqlMainDomLock>(ex, "Sql timeout occurred, could not acquire MainDom.");
                        _hasError = true;
                        return false;
                    }

                    // unexpected (will be caught below)
                    throw;
                }

                var result = InsertLockRecord(tempId); //we change the row to a random Id to signal other MainDom to shutdown
                if (result == RecordPersistenceType.Insert)
                {
                    // if we've inserted, then there was no MainDom so we can instantly acquire

                    // TODO: see the other TODO, could we just delete the row and that would indicate that we
                    // are MainDom? then we don't leave any orphan rows behind.

                    InsertLockRecord(_lockId); // so update with our appdomain id
                    _logger.Debug<SqlMainDomLock>("Acquired with ID {LockId}", _lockId);
                    return true;
                }

                // if we've updated, this means there is an active MainDom, now we need to wait to
                // for the current MainDom to shutdown which also requires releasing our write lock
            }
            catch (Exception ex)
            {
                ResetDatabase();
                // unexpected
                _logger.Error<SqlMainDomLock>(ex, "Unexpected error, cannot acquire MainDom");
                _hasError = true;
                return false;
            }
            finally
            {
                db?.CompleteTransaction();
            }

            return await WaitForExistingAsync(tempId, millisecondsTimeout);
        }

        public Task ListenAsync()
        {
            if (_hasError)
            {
                _logger.Warn<SqlMainDomLock>("Could not acquire MainDom, listening is canceled.");
                return Task.CompletedTask;
            }

            // Create a long running task (dedicated thread)
            // to poll to check if we are still the MainDom registered in the DB
            return Task.Factory.StartNew(ListeningLoop, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

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
                // poll every 1 second
                Thread.Sleep(1000);

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
                        return;

                    var db = GetDatabase();

                    try
                    {
                        db.BeginTransaction(IsolationLevel.ReadCommitted);

                        // get a read lock
                        _sqlServerSyntax.ReadLock(db, Constants.Locks.MainDom);

                        // TODO: We could in theory just check if the main dom row doesn't exist, that could indicate that
                        // we are still the maindom. An empty value might be better because then we won't have any orphan rows
                        // if the app is terminated. Could that work?

                        if (!IsMainDomValue(_lockId))
                        {
                            // we are no longer main dom, another one has come online, exit
                            _mainDomChanging = true;
                            _logger.Debug<SqlMainDomLock>("Detected new booting application, releasing MainDom lock.");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ResetDatabase();
                        // unexpected
                        _logger.Error<SqlMainDomLock>(ex, "Unexpected error, listening is canceled.");
                        _hasError = true;
                        return;
                    }
                    finally
                    {
                        db?.CompleteTransaction();
                    }
                }

            }
        }

        private void ResetDatabase()
        {
            if (_db.InTransaction)
                _db.AbortTransaction();
            _db.Dispose();
            _db = null;
        }

        private IUmbracoDatabase GetDatabase()
        {
            if (_db != null)
                return _db;

            _db = _dbFactory.CreateDatabase();
            return _db;
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

            return Task.Run(() =>
            {
                var db = GetDatabase();
                var watch = new Stopwatch();
                watch.Start();
                while(true)
                {
                    // poll very often, we need to take over as fast as we can
                    Thread.Sleep(100);

                    try
                    {
                        db.BeginTransaction(IsolationLevel.ReadCommitted);

                        // get a read lock
                        _sqlServerSyntax.ReadLock(db, Constants.Locks.MainDom);

                        // the row 
                        var mainDomRows = db.Fetch<KeyValueDto>("SELECT * FROM umbracoKeyValue WHERE [key] = @key", new { key = MainDomKey });

                        if (mainDomRows.Count == 0 || mainDomRows[0].Value == updatedTempId)
                        {
                            // the other main dom has updated our record
                            // Or the other maindom shutdown super fast and just deleted the record
                            // which indicates that we
                            // can acquire it and it has shutdown.

                            _sqlServerSyntax.WriteLock(db, Constants.Locks.MainDom);

                            // so now we update the row with our appdomain id
                            InsertLockRecord(_lockId);
                            _logger.Debug<SqlMainDomLock>("Acquired with ID {LockId}", _lockId);
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
                        ResetDatabase();

                        if (IsLockTimeoutException(ex))
                        {
                            _logger.Error<SqlMainDomLock>(ex, "Sql timeout occurred, waiting for existing MainDom is canceled.");
                            _hasError = true;
                            return false;
                        }
                        // unexpected
                        _logger.Error<SqlMainDomLock>(ex, "Unexpected error, waiting for existing MainDom is canceled.");
                        _hasError = true;
                        return false;
                    }
                    finally
                    {
                        db?.CompleteTransaction();
                    }

                    if (watch.ElapsedMilliseconds >= millisecondsTimeout)
                    {
                        // if the timeout has elapsed, it either means that the other main dom is taking too long to shutdown,
                        // or it could mean that the previous appdomain was terminated and didn't clear out the main dom SQL row
                        // and it's just been left as an orphan row.
                        // There's really know way of knowing unless we are constantly updating the row for the current maindom
                        // which isn't ideal.
                        // So... we're going to 'just' take over, if the writelock works then we'll assume we're ok

                        _logger.Debug<SqlMainDomLock>("Timeout elapsed, assuming orphan row, acquiring MainDom.");

                        try
                        {
                            db.BeginTransaction(IsolationLevel.ReadCommitted);

                            _sqlServerSyntax.WriteLock(db, Constants.Locks.MainDom);

                            // so now we update the row with our appdomain id
                            InsertLockRecord(_lockId);
                            _logger.Debug<SqlMainDomLock>("Acquired with ID {LockId}", _lockId);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            ResetDatabase();

                            if (IsLockTimeoutException(ex))
                            {
                                // something is wrong, we cannot acquire, not much we can do
                                _logger.Error<SqlMainDomLock>(ex, "Sql timeout occurred, could not forcibly acquire MainDom.");
                                _hasError = true;
                                return false;
                            }
                            _logger.Error<SqlMainDomLock>(ex, "Unexpected error, could not forcibly acquire MainDom.");
                            _hasError = true;
                            return false;
                        }
                        finally
                        {
                            db?.CompleteTransaction();
                        }
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Inserts or updates the key/value row 
        /// </summary>
        private RecordPersistenceType InsertLockRecord(string id)
        {
            var db = GetDatabase();
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
        private bool IsMainDomValue(string val)
        {
            var db = GetDatabase();
            return db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoKeyValue WHERE [key] = @key AND [value] = @val",
                new { key = MainDomKey, val = val }) == 1;
        }

        /// <summary>
        /// Checks if the exception is an SQL timeout
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private bool IsLockTimeoutException(Exception exception) => exception is SqlException sqlException && sqlException.Number == 1222;

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
                        // immediately cancel all sub-tasks, we don't want them to keep querying
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();

                        if (_dbFactory.Configured)
                        {
                            var db = GetDatabase();
                            try
                            {
                                db.BeginTransaction(IsolationLevel.ReadCommitted);

                                // get a write lock
                                _sqlServerSyntax.WriteLock(db, Constants.Locks.MainDom);

                                // When we are disposed, it means we have released the MainDom lock
                                // and called all MainDom release callbacks, in this case
                                // if another maindom is actually coming online we need
                                // to signal to the MainDom coming online that we have shutdown.
                                // To do that, we update the existing main dom DB record with a suffixed "_updated" string.
                                // Otherwise, if we are just shutting down, we want to just delete the row.
                                if (_mainDomChanging)
                                {
                                    _logger.Debug<SqlMainDomLock>("Releasing MainDom, updating row, new application is booting.");
                                    db.Execute($"UPDATE umbracoKeyValue SET [value] = [value] + '{UpdatedSuffix}' WHERE [key] = @key", new { key = MainDomKey });
                                }
                                else
                                {
                                    _logger.Debug<SqlMainDomLock>("Releasing MainDom, deleting row, application is shutting down.");
                                    db.Execute("DELETE FROM umbracoKeyValue WHERE [key] = @key", new { key = MainDomKey });
                                }
                            }
                            catch (Exception ex)
                            {
                                ResetDatabase();
                                _logger.Error<SqlMainDomLock>(ex, "Unexpected error during dipsose.");
                                _hasError = true;
                            }
                            finally
                            {
                                db?.CompleteTransaction();
                                ResetDatabase();
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
