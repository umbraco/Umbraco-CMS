using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    interface IEntityContainerRepository : IReadRepository<int, EntityContainer>, IWriteRepository<EntityContainer>
    { }
}
