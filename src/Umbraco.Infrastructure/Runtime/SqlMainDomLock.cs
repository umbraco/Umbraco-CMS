using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Extensions;
using MapperCollection = Umbraco.Cms.Infrastructure.Persistence.Mappers.MapperCollection;

namespace Umbraco.Cms.Infrastructure.Runtime;

public class SqlMainDomLock : IMainDomLock
{
    private const string MainDomKeyPrefix = "Umbraco.Core.Runtime.SqlMainDom";
    private const string UpdatedSuffix = "_updated";
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IUmbracoDatabase? _db;
    private readonly UmbracoDatabaseFactory _dbFactory;
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly object _locker = new();
    private readonly string _lockId;
    private readonly ILogger<SqlMainDomLock> _logger;
    private bool _acquireWhenTablesNotAvailable;
    private bool _errorDuringAcquiring;
    private bool _hasTable;
    private bool _mainDomChanging;

    public SqlMainDomLock(
        ILoggerFactory loggerFactory,
        IOptions<GlobalSettings> globalSettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        IDbProviderFactoryCreator dbProviderFactoryCreator,
        IMainDomKeyGenerator mainDomKeyGenerator,
        DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        NPocoMapperCollection npocoMappers)
    {
        // unique id for our appdomain, this is more unique than the appdomain id which is just an INT counter to its safer
        _lockId = Guid.NewGuid().ToString();
        _logger = loggerFactory.CreateLogger<SqlMainDomLock>();
        _globalSettings = globalSettings;

        _dbFactory = new UmbracoDatabaseFactory(
            loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
            loggerFactory,
            _globalSettings,
            connectionStrings,
            new MapperCollection(() => Enumerable.Empty<BaseMapper>()),
            dbProviderFactoryCreator,
            databaseSchemaCreatorFactory,
            npocoMappers);

        MainDomKey = MainDomKeyPrefix + "-" + mainDomKeyGenerator.GenerateKey();
    }

    /// <summary>
    ///     Returns the keyvalue table key for the current server/app
    /// </summary>
    /// <remarks>
    ///     The key is the the normal MainDomId which takes into account the AppDomainAppId and the physical file path of the
    ///     app and this is
    ///     combined with the current machine name. The machine name is required because the default semaphore lock is machine
    ///     wide so it implicitly
    ///     takes into account machine name whereas this needs to be explicitly per machine.
    /// </remarks>
    private string MainDomKey { get; }

    public async Task<bool> AcquireLockAsync(int millisecondsTimeout)
    {
        if (!_dbFactory.Configured)
        {
            // if we aren't configured then we're in an install state, in which case we have no choice but to assume we can acquire
            return true;
        }

        _logger.LogDebug("Acquiring lock...");

        var tempId = Guid.NewGuid().ToString();

        IUmbracoDatabase? db = null;

        try
        {
            db = _dbFactory.CreateDatabase();


            _hasTable = db.HasTable(Constants.DatabaseSchema.Tables.KeyValue);
            if (!_hasTable)
            {
                _logger.LogDebug(
                    "The DB does not contain the required table so we must be in an install state. We have no choice but to assume we can acquire.");
                _acquireWhenTablesNotAvailable = true;
                return true;
            }

            db.BeginTransaction(IsolationLevel.Serializable);

            RecordPersistenceType
                result = InsertLockRecord(tempId,
                    db); //we change the row to a random Id to signal other MainDom to shutdown
            if (result == RecordPersistenceType.Insert)
            {
                // if we've inserted, then there was no MainDom so we can instantly acquire

                InsertLockRecord(_lockId, db); // so update with our appdomain id
                _logger.LogDebug("Acquired with ID {LockId}", _lockId);
                return true;
            }

            // if we've updated, this means there is an active MainDom, now we need to wait to
            // for the current MainDom to shutdown which also requires releasing our write lock
        }
        catch (Exception ex)
        {
            // unexpected
            _logger.LogError(ex, "Unexpected error, cannot acquire MainDom");
            _errorDuringAcquiring = true;
            return false;
        }
        finally
        {
            db?.CompleteTransaction();
            db?.Dispose();
        }


        return await WaitForExistingAsync(tempId, millisecondsTimeout);
    }

    public Task ListenAsync()
    {
        if (_errorDuringAcquiring)
        {
            _logger.LogWarning("Could not acquire MainDom, listening is canceled.");
            return Task.CompletedTask;
        }

        // Create a long running task (dedicated thread)
        // to poll to check if we are still the MainDom registered in the DB
        return Task.Factory.StartNew(
            ListeningLoop,
            _cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning,
            // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
            TaskScheduler.Default);
    }

    private void ListeningLoop()
    {
        while (true)
        {
            // poll every couple of seconds
            // local testing shows the actual query to be executed from client/server is approx 300ms but would change depending on environment/IO
            Thread.Sleep(_globalSettings.Value.MainDomReleaseSignalPollingInterval);

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
                    _logger.LogDebug("Task canceled, exiting loop");
                    return;
                }

                IUmbracoDatabase? db = null;

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

                    // In case we acquired the main dom doing install when there was no database. We therefore have to insert our lockId now, but only handle this once.
                    if (_acquireWhenTablesNotAvailable)
                    {
                        _acquireWhenTablesNotAvailable = false;
                        InsertLockRecord(_lockId, db);
                    }

                    db.BeginTransaction(IsolationLevel.Serializable);

                    if (!IsMainDomValue(_lockId, db))
                    {
                        // we are no longer main dom, another one has come online, exit
                        _mainDomChanging = true;
                        _logger.LogDebug("Detected new booting application, releasing MainDom lock.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during listening.");

                    // We need to keep on listening unless we've been notified by our own AppDomain to shutdown since
                    // we don't want to shutdown resources controlled by MainDom inadvertently. We'll just keep listening otherwise.
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        _logger.LogDebug("Task canceled, exiting loop");
                        return;
                    }
                }
                finally
                {
                    db?.CompleteTransaction();
                    db?.Dispose();
                }
            }
        }
    }

    /// <summary>
    ///     Wait for any existing MainDom to release so we can continue booting
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
                    using IUmbracoDatabase db = _dbFactory.CreateDatabase();

                    var watch = new Stopwatch();
                    watch.Start();
                    while (true)
                    {
                        // poll very often, we need to take over as fast as we can
                        // local testing shows the actual query to be executed from client/server is approx 300ms but would change depending on environment/IO
                        Thread.Sleep(1000);

                        var acquired = TryAcquire(db, tempId, updatedTempId);
                        if (acquired.HasValue)
                        {
                            return acquired.Value;
                        }

                        if (watch.ElapsedMilliseconds >= millisecondsTimeout)
                        {
                            return AcquireWhenMaxWaitTimeElapsed(db);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "An error occurred trying to acquire and waiting for existing SqlMainDomLock to shutdown");
                    return false;
                }
            }, _cancellationTokenSource.Token);
        }
    }

    private bool? TryAcquire(IUmbracoDatabase db, string tempId, string updatedTempId)
    {
        // Creates a separate transaction to the DB instance so we aren't allocating tons of new DB instances for each transaction
        // since this is executed in a tight loop

        ITransaction? transaction = null;

        try
        {
            transaction = db.GetTransaction(IsolationLevel.Serializable);

            // the row
            List<KeyValueDto>? mainDomRows = db.Fetch<KeyValueDto>("SELECT * FROM umbracoKeyValue WHERE [key] = @key",
                new {key = MainDomKey});

            if (mainDomRows.Count == 0 || mainDomRows[0].Value == updatedTempId)
            {
                // the other main dom has updated our record
                // Or the other maindom shutdown super fast and just deleted the record
                // which indicates that we
                // can acquire it and it has shutdown.

                // so now we update the row with our appdomain id
                InsertLockRecord(_lockId, db);
                _logger.LogDebug("Acquired with ID {LockId}", _lockId);
                return true;
            }
            else if (mainDomRows.Count == 1 && (!mainDomRows[0].Value?.StartsWith(tempId) ?? false))
            {
                // in this case, the prefixed ID is different which  means
                // another new AppDomain has come online and is wanting to take over. In that case, we will not
                // acquire.

                _logger.LogDebug("Cannot acquire, another booting application detected.");
                return false;
            }
        }
        catch (Exception ex)
        {
            // unexpected
            _logger.LogError(ex, "Unexpected error, waiting for existing MainDom is canceled.");
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

        _logger.LogDebug("Timeout elapsed, assuming orphan row, acquiring MainDom.");

        ITransaction? transaction = null;

        try
        {
            transaction = db.GetTransaction(IsolationLevel.Serializable);

            // so now we update the row with our appdomain id
            InsertLockRecord(_lockId, db);
            _logger.LogDebug("Acquired with ID {LockId}", _lockId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error, could not forcibly acquire MainDom.");
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
    ///     Inserts or updates the key/value row
    /// </summary>
    private RecordPersistenceType InsertLockRecord(string id, IUmbracoDatabase db) =>
        db.InsertOrUpdate(new KeyValueDto {Key = MainDomKey, Value = id, UpdateDate = DateTime.Now});

    /// <summary>
    ///     Checks if the DB row value is equals the value
    /// </summary>
    /// <returns></returns>
    private bool IsMainDomValue(string val, IUmbracoDatabase db) =>
        db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoKeyValue WHERE [key] = @key AND [value] = @val",
            new {key = MainDomKey, val}) == 1;

    #region IDisposable Support

    private bool _disposedValue; // To detect redundant calls


    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                lock (_locker)
                {
                    _logger.LogDebug($"{nameof(SqlMainDomLock)} Disposing...");

                    // immediately cancel all sub-tasks, we don't want them to keep querying
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();

                    if (_dbFactory.Configured && _hasTable)
                    {
                        IUmbracoDatabase? db = null;
                        try
                        {
                            db = _dbFactory.CreateDatabase();
                            db!.BeginTransaction(IsolationLevel.Serializable);

                            // When we are disposed, it means we have released the MainDom lock
                            // and called all MainDom release callbacks, in this case
                            // if another maindom is actually coming online we need
                            // to signal to the MainDom coming online that we have shutdown.
                            // To do that, we update the existing main dom DB record with a suffixed "_updated" string.
                            // Otherwise, if we are just shutting down, we want to just delete the row.
                            if (_mainDomChanging)
                            {
                                _logger.LogDebug("Releasing MainDom, updating row, new application is booting.");
                                var count = db.Execute(
                                    $"UPDATE umbracoKeyValue SET [value] = [value] + '{UpdatedSuffix}' WHERE [key] = @key",
                                    new {key = MainDomKey});
                            }
                            else
                            {
                                _logger.LogDebug("Releasing MainDom, deleting row, application is shutting down.");
                                var count = db.Execute("DELETE FROM umbracoKeyValue WHERE [key] = @key",
                                    new {key = MainDomKey});
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unexpected error during dipsose.");
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
                                _logger.LogError(ex, "Unexpected error during dispose when completing transaction.");
                            }
                        }
                    }
                }
            }

            _disposedValue = true;
        }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() =>
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);

    #endregion
}
