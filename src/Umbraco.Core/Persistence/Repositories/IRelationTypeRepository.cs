using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRelationTypeRepository : IReadWriteQueryRepository<int, IRelationType>, IReadRepository<Guid, IRelationType>
    { }
}
