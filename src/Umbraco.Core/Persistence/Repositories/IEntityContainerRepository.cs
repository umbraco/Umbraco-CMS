using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IEntityContainerRepository : IReadRepository<int, EntityContainer>, IWriteRepository<EntityContainer>
{
    EntityContainer? Get(Guid id);

    IEnumerable<EntityContainer> Get(string name, int level);
}
