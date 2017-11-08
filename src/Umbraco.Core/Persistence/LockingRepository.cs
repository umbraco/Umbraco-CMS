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
        private readonly IScopeUnitOfWorkProvider _uowProvider;
        private readonly Func<IScopeUnitOfWork, TRepository> _repositoryFactory;
        private readonly int[] _readLockIds, _writeLockIds;

        public LockingRepository(IScopeUnitOfWorkProvider uowProvider, Func<IScopeUnitOfWork, TRepository> repositoryFactory,
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
            using (var uow = _uowProvider.GetUnitOfWork(IsolationLevel.RepeatableRead))
            {
                // getting the database creates a scope and a transaction
                // the scope is IsolationLevel.RepeatableRead (because UnitOfWork is)
                // and will throw if outer scope (if any) has a lower isolation level

                foreach (var lockId in _readLockIds)
                    uow.Database.AcquireLockNodeReadLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    action(new LockedRepository<TRepository>(uow, repository));
                    if (autoCommit == false) return;
                    uow.Commit();

                } // dispose repository => dispose uow => complete (or not) scope
            } // dispose uow again => nothing
        }

        public TResult WithReadLocked<TResult>(Func<LockedRepository<TRepository>, TResult> func, bool autoCommit = true)
        {
            using (var uow = _uowProvider.GetUnitOfWork(IsolationLevel.RepeatableRead))
            {
                // getting the database creates a scope and a transaction
                // the scope is IsolationLevel.RepeatableRead (because UnitOfWork is)
                // and will throw if outer scope (if any) has a lower isolation level

                foreach (var lockId in _readLockIds)
                    uow.Database.AcquireLockNodeReadLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    var ret = func(new LockedRepository<TRepository>(uow, repository));
                    if (autoCommit == false) return ret;
                    uow.Commit();
                    return ret;

                }  // dispose repository => dispose uow => complete (or not) scope
            } // dispose uow again => nothing
        }

        public void WithWriteLocked(Action<LockedRepository<TRepository>> action, bool autoCommit = true)
        {
            using (var uow = _uowProvider.GetUnitOfWork(IsolationLevel.RepeatableRead))
            {
                // getting the database creates a scope and a transaction
                // the scope is IsolationLevel.RepeatableRead (because UnitOfWork is)
                // and will throw if outer scope (if any) has a lower isolation level

                foreach (var lockId in _writeLockIds)
                    uow.Database.AcquireLockNodeWriteLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    action(new LockedRepository<TRepository>(uow, repository));
                    if (autoCommit == false) return;
                    uow.Commit();

                } // dispose repository => dispose uow => complete (or not) scope
            } // dispose uow again => nothing
        }

        public TResult WithWriteLocked<TResult>(Func<LockedRepository<TRepository>, TResult> func, bool autoCommit = true)
        {
            using (var uow = _uowProvider.GetUnitOfWork(IsolationLevel.RepeatableRead))
            {
                // getting the database creates a scope and a transaction
                // the scope is IsolationLevel.RepeatableRead (because UnitOfWork is)
                // and will throw if outer scope (if any) has a lower isolation level

                foreach (var lockId in _writeLockIds)
                    uow.Database.AcquireLockNodeReadLock(lockId);

                using (var repository = _repositoryFactory(uow))
                {
                    var ret = func(new LockedRepository<TRepository>(uow, repository));
                    if (autoCommit == false) return ret;
                    uow.Commit();
                    return ret;

                } // dispose repository => dispose uow => complete (or not) scope
            } // dispose uow again => nothing
        }
    }
}
