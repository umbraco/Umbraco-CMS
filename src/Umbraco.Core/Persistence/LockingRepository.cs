using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    internal class LockingRepository<TRepository>
        where TRepository : IDisposable, IRepository
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private readonly Func<IDatabaseUnitOfWork, TRepository> _repositoryFactory;
        private readonly int[] _readLockIds, _writeLockIds;

        public LockingRepository(IDatabaseUnitOfWorkProvider uowProvider, Func<IDatabaseUnitOfWork, TRepository> repositoryFactory,
            IEnumerable<int> readLockIds, IEnumerable<int> writeLockIds)
        {
            Mandate.ParameterNotNull(uowProvider, "uowProvider");
            Mandate.ParameterNotNull(repositoryFactory, "repositoryFactory");

            _uowProvider = uowProvider;
            _repositoryFactory = repositoryFactory;
            _readLockIds = readLockIds == null ? new int[0] : readLockIds.ToArray();
            _writeLockIds = writeLockIds == null ? new int[0] : writeLockIds.ToArray();
        }

        public void WithReadLocked(Action<LockedRepository<TRepository>> action, bool autoCommit = true)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var transaction = uow.Database.GetTransaction(IsolationLevel.RepeatableRead))
            {
                foreach (var lockId in _readLockIds)
                    uow.Database.AcquireLockNodeReadLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    action(new LockedRepository<TRepository>(transaction, uow, repository));
                    if (autoCommit == false) return;
                    uow.Commit();
                    transaction.Complete();
                }
            }
        }

        public TResult WithReadLocked<TResult>(Func<LockedRepository<TRepository>, TResult> func, bool autoCommit = true)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            using (var transaction = uow.Database.GetTransaction(IsolationLevel.RepeatableRead))
            {
                foreach (var lockId in _readLockIds)
                    uow.Database.AcquireLockNodeReadLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    var ret = func(new LockedRepository<TRepository>(transaction, uow, repository));
                    if (autoCommit == false) return ret;
                    uow.Commit();
                    transaction.Complete();
                    return ret;
                }
            }
        }

        public void WithWriteLocked(Action<LockedRepository<TRepository>> action, bool autoCommit = true)
        {
            // must use the uow to ensure it's disposed if GetTransaction fails!

            using (var uow = _uowProvider.GetUnitOfWork())
            using (var transaction = uow.Database.GetTransaction(IsolationLevel.RepeatableRead))
            {
                foreach (var lockId in _writeLockIds)
                    uow.Database.AcquireLockNodeWriteLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    action(new LockedRepository<TRepository>(transaction, uow, repository));
                    if (autoCommit == false) return;
                    uow.Commit();
                    transaction.Complete();
                }
            }
            // fixme
            // the change above ensures that scopes are properly disposed
            // TODO apply to all methods
            // now how can we manage the isolation level?

            //uow.ScopeIsolationLevel = IsolationLevel.RepeatableRead; // might throw when creating the scope
            // getTransaction here throws because of different levels
            // but the exception gets eaten because on the way out, disposing the _outer_ scope fails
            // so... how shall we figure it out?!
            // - we should be able to set the isolation level on the uow scope
            // - should we be able to dispose parent scopes and then what happens? NO!
            //
            // if we can create scopes with isolation levels what happens when we nest scopes?
            // note: this is a *very* special use case
        }

        public TResult WithWriteLocked<TResult>(Func<LockedRepository<TRepository>, TResult> func, bool autoCommit = true)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            using (var transaction = uow.Database.GetTransaction(IsolationLevel.RepeatableRead))
            {
                foreach (var lockId in _writeLockIds)
                    uow.Database.AcquireLockNodeReadLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    var ret = func(new LockedRepository<TRepository>(transaction, uow, repository));
                    if (autoCommit == false) return ret;
                    uow.Commit();
                    transaction.Complete();
                    return ret;
                }
            }
        }
    }
}
