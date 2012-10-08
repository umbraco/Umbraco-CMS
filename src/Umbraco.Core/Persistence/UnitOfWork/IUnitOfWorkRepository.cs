using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IUnitOfWorkRepository
    {
        void PersistNewItem(IEntity entity);
        void PersistUpdatedItem(IEntity entity);
        void PersistDeletedItem(IEntity entity);
    }
}