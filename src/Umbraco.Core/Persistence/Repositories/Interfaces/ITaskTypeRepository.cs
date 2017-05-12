using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ITaskTypeRepository : IUnitOfWorkRepository, IQueryRepository<int, TaskType>
    { }
}