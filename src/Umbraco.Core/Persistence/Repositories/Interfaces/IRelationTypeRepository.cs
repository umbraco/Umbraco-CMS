using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRelationTypeRepository : IUnitOfWorkRepository, IQueryRepository<int, IRelationType>, IReadRepository<Guid, IRelationType>
    { }
}
