using System;
using System.Data;
using System.Data.SqlClient;
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
        private string _appDomainId;
        private const string MainDomKey = "Umbraco.Core.Runtime.SqlMainDom";
        private readonly ILogger _logger;
        private IUmbracoDatabase _db;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private SqlServerSyntaxProvider _sqlServerSyntax = new SqlServerSyntaxProvider();

        public SqlMainDomLock(ILogger logger)
        {
            _appDomainId = AppDomain.CurrentDomain.Id.ToString();
            _logger = logger;
        }

        

        public Task<bool> AcquireLockAsync(int millisecondsTimeout)
        {
            var factory = new UmbracoDatabaseFactory(
                Constants.System.UmbracoConnectionName,
                _logger,
                new Lazy<IMapperCollection>(() => new Persistence.Mappers.MapperCollection(Enumerable.Empty<BaseMapper>())));

            _db = factory.CreateDatabase();

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
                        return Task.FromResult(false);
                    }

                    // unexpected
                    throw;
                }

                InsertLockRecord();

                return Task.FromResult(true);
            }
            catch(Exception)
            {
                _db.AbortTransaction();

                // unexpected
                throw;
            }
            finally
            {
                _db.CompleteTransaction();
            }
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

                        if (!IsStillMainDom())
                        {
                            // we are no longer main dom, exit
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
        /// Inserts or updates the key/value row to check if the current appdomain is registerd as the maindom
        /// </summary>
        private void InsertLockRecord()
        {
            _db.InsertOrUpdate(new KeyValueDto
            {
                Key = MainDomKey,
                Value = _appDomainId,
                Updated = DateTime.Now
            });
        }

        /// <summary>
        /// Checks if the DB row value is our current appdomain value
        /// </summary>
        /// <returns></returns>
        private bool IsStillMainDom()
        {
            return _db.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoKeyValue WHERE [key] = @key AND [value] = @val",
                new { key = MainDomKey, val = _appDomainId }) == 1;
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
                    _db.Dispose();
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
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
