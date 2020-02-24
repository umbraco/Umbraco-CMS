using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IEntityContainerRepository : IReadRepository<int, EntityContainer>, IWriteRepository<EntityContainer>
    { }
}
