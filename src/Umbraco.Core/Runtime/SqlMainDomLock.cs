using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private const string MainDomKey = "Umbraco.Core.Runtime.SqlMainDom";
        private readonly ILogger _logger;
        private IUmbracoDatabase _db;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private SqlServerSyntaxProvider _sqlServerSyntax = new SqlServerSyntaxProvider();
        private bool _mainDomChanging = false;

        public SqlMainDomLock(ILogger logger)
        {
            // unique id for our appdomain, this is more unique than the appdomain id which is just an INT counter to its safer
            _lockId = Guid.NewGuid().ToString();
            _logger = logger;
        }

        public async Task<bool> AcquireLockAsync(int millisecondsTimeout)
        {
            var factory = new UmbracoDatabaseFactory(
                Constants.System.UmbracoConnectionName,
                _logger,
                new Lazy<IMapperCollection>(() => new Persistence.Mappers.MapperCollection(Enumerable.Empty<BaseMapper>())));

            _db = factory.CreateDatabase();

            var tempId = Guid.NewGuid().ToString();

            try
            {
                _db.BeginTransaction(IsolationLevel.ReadCommitted);

                try
                {
                    // wait to get a write lock
                    _sqlServerSyntax.WriteLock(_db, millisecondsTimeout, Constants.Locks.MainDom);                    
                }
                catch (Exception ex)
                {
                    if (IsLockTimeoutException(ex))
                    {
                        return false;
                    }

                    // unexpected
                    throw;
                }

                var result = InsertLockRecord(tempId); //we change the row to a random Id to signal other MainDom to shutdown
                if (result == RecordPersistenceType.Insert)
                {
                    // if we've inserted, then there was no MainDom so we can instantly acquire

                    // TODO: see the other TODO, could we just delete the row and that would indicate that we
                    // are MainDom? then we don't leave any orphan rows behind.

                    InsertLockRecord(_lockId); // so update with our appdomain id
                    return true;
                }

                // if we've updated, this means there is an active MainDom, now we need to wait to
                // for the current MainDom to shutdown which also requires releasing our write lock
            }
            catch (Exception)
            {
                _db.AbortTransaction();

                // unexpected
                throw;
            }
            finally
            {
                _db.CompleteTransaction();
            }

            return await WaitForExistingAsync(tempId, millisecondsTimeout);
        }

        public Task ListenAsync()
        {
            // Create a long running task (dedicated thread)
            // to poll to check if we are still the MainDom registered in the DB
            return Task.Factory.StartNew(() =>
            {
                while(true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        break;

                    // poll every 1 second
                    Thread.Sleep(1000);

                    try
                    {
                        _db.BeginTransaction(IsolationLevel.ReadCommitted);

                        // get a read lock
                        _sqlServerSyntax.ReadLock(_db, Constants.Locks.MainDom);

                        // TODO: We could in theory just check if the main dom row doesn't exist, that could indicate that
                        // we are still the maindom. An empty value might be better because then we won't have any orphan rows
                        // if the app is terminated. Could that work?

                        if (!IsMainDomValue(_lockId))
                        {
                            // we are no longer main dom, another one has come online, exit
                            _mainDomChanging = true;
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        _db.AbortTransaction();
                        throw;
                    }
                    finally
                    {
                        _db.CompleteTransaction();
                    }
                }
                

            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        /// <summary>
        /// Wait for any existing MainDom to release so we can continue booting
        /// </summary>
        /// <param name="tempId"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        private Task<bool> WaitForExistingAsync(string tempId, int millisecondsTimeout)
        {
            var updatedTempId = tempId + "_updated";

            return Task.Run(() =>
            {
                var watch = new Stopwatch();
                watch.Start();
                while(true)
                {
                    // poll very often, we need to take over as fast as we can
                    Thread.Sleep(100);

                    try
                    {
                        _db.BeginTransaction(IsolationLevel.ReadCommitted);

                        // get a read lock
                        _sqlServerSyntax.ReadLock(_db, Constants.Locks.MainDom);

                        var mainDomRows = _db.Fetch<KeyValueDto>("SELECT * FROM umbracoKeyValue WHERE [key] = @key", new { key = MainDomKey });

                        if (mainDomRows.Count == 0 || mainDomRows[0].Value == updatedTempId)
                        {
                            // the other main dom has updated our record
                            // Or the other maindom shutdown super fast and just deleted the record
                            // which indicates that we
                            // can acquire it and it has shutdown.

                            _sqlServerSyntax.WriteLock(_db, Constants.Locks.MainDom);

                            // so now we update the row with our appdomain id
                            InsertLockRecord(_lockId);

                            return true; 
                        }
                        else if (mainDomRows.Count == 1 && mainDomRows[0].Value.EndsWith("_updated"))
                        {
                            // in this case, there is a suffixed _updated value but it's not for our ID which means
                            // another new AppDomain has come online and is wanting to take over. In that case, we will not
                            // acquire.

                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _db.AbortTransaction();

                        if (IsLockTimeoutException(ex))
                        {
                            return false;
                        }

                        throw;
                    }
                    finally
                    {
                        _db.CompleteTransaction();
                    }

                    if (watch.ElapsedMilliseconds >= millisecondsTimeout)
                    {
                        // if the timeout has elapsed, it either means that the other main dom is taking too long to shutdown,
                        // or it could mean that the previous appdomain was terminated and didn't clear out the main dom SQL row
                        // and it's just been left as an orphan row.
                        // There's really know way of knowing unless we are constantly updating the row for the current maindom
                        // which isn't ideal.
                        // So... we're going to 'just' take over, if the writelock works then we'll assume we're ok


                        try
                        {
                            _db.BeginTransaction(IsolationLevel.ReadCommitted);

                            _sqlServerSyntax.WriteLock(_db, Constants.Locks.MainDom);

                            // so now we update the row with our appdomain id
                            InsertLockRecord(_lockId);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _db.AbortTransaction();

                            if (IsLockTimeoutException(ex))
                            {
                                // something is wrong, we cannot acquire, not much we can do 
                                return false;
                            }

                            throw;
                        }
                        finally
                        {
                            _db.CompleteTransaction();
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
            return _db.InsertOrUpdate(new KeyValueDto
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
            return _db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoKeyValue WHERE [key] = @key AND [value] = @val",
                new { key = MainDomKey, val = val }) == 1;
        }

        /// <summary>
        /// Checks if the exception is an SQL timeout
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private bool IsLockTimeoutException(Exception exception) => exception is SqlException sqlException && sqlException.Number == 1222;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // capture locally just in case in some strange way a sub task somehow updates this
                    var mainDomChanging = _mainDomChanging;

                    // immediately cancel all sub-tasks, we don't want them to keep querying
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();

                    try
                    {
                        _db.BeginTransaction(IsolationLevel.ReadCommitted);

                        // get a write lock
                        _sqlServerSyntax.WriteLock(_db, Constants.Locks.MainDom);

                        // When we are disposed, it means we have released the MainDom lock
                        // and called all MainDom release callbacks, in this case
                        // if another maindom is actually coming online we need
                        // to signal to the MainDom coming online that we have shutdown.
                        // To do that, we update the existing main dom DB record with a suffixed "_updated" string.
                        // Otherwise, if we are just shutting down, we want to just delete the row.
                        if (mainDomChanging)
                        {
                            _db.Execute("UPDATE umbracoKeyValue SET [value] = [value] + '_updated' WHERE [key] = @key", new { key = MainDomKey });
                        }
                        else
                        {
                            _db.Execute("DELETE FROM umbracoKeyValue WHERE [key] = @key", new { key = MainDomKey });
                        }
                    }
                    catch (Exception)
                    {
                        _db.AbortTransaction();
                        throw;
                    }
                    finally
                    {
                        _db.CompleteTransaction();

                        _db.Dispose();                        
                    }
                }

                disposedValue = true;
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
