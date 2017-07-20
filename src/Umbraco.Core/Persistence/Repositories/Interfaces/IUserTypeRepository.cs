using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IUserTypeRepository : IUnitOfWorkRepository, IQueryRepository<int, IUserType>
    {

    }
}
