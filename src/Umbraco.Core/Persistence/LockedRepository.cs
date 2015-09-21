using System;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    internal class LockedRepository<TRepository>
        where TRepository : IDisposable, IRepository
    {
        public LockedRepository(Transaction transaction, IDatabaseUnitOfWork unitOfWork, TRepository repository)
        {
            Transaction = transaction;
            UnitOfWork = unitOfWork;
            Repository = repository;
        }

        public Transaction Transaction { get; private set; }
        public IDatabaseUnitOfWork UnitOfWork { get; private set; }
        public TRepository Repository { get; private set; }

        public void Commit()
        {
            UnitOfWork.Commit();
            Transaction.Complete();
        }
    }
}