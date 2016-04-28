using System;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
    /// Defines a Unit Of Work
    /// </summary>
    public interface IUnitOfWork : IDisposable
	{
        void RegisterAdded(IEntity entity, IUnitOfWorkRepository repository);
        void RegisterChanged(IEntity entity, IUnitOfWorkRepository repository);
        void RegisterRemoved(IEntity entity, IUnitOfWorkRepository repository);
        void Commit();
        TRepository CreateRepository<TRepository>(string name = null) where TRepository : IRepository;
    }
}