using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Defines the Unit Of Work-part of a repository
    /// </summary>
    public interface IUnitOfWorkRepository
    {
        void PersistNewItem(IEntity entity);
        void PersistUpdatedItem(IEntity entity);
        void PersistDeletedItem(IEntity entity);
    }
}