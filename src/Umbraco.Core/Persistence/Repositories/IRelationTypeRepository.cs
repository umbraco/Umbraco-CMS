using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IRelationTypeRepository : IReadWriteQueryRepository<int, IRelationType>,
    IReadRepository<Guid, IRelationType>
{
}
