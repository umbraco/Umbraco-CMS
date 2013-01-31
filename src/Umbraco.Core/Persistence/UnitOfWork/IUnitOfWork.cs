using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
    /// Defines a Unit Of Work
    /// </summary>
    public interface IUnitOfWork
    {	    
        void RegisterAdded(IEntity entity, IUnitOfWorkRepository repository);
        void RegisterChanged(IEntity entity, IUnitOfWorkRepository repository);
        void RegisterRemoved(IEntity entity, IUnitOfWorkRepository repository);
        void Commit();
        object Key { get; }
    }
}