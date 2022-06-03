using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IRelationTypeRepository : IAsyncReadWriteQueryRepository<int, IRelationType>, IReadRepository<Guid, IRelationType>
    { }
}
