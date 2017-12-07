using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRelationTypeRepository : IUnitOfWorkRepository, IReadWriteQueryRepository<int, IRelationType>, IReadRepository<Guid, IRelationType>
    { }
}
