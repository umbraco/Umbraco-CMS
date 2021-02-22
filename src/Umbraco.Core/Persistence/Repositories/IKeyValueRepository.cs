using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IKeyValueRepository : IReadRepository<string, IKeyValue>, IWriteRepository<IKeyValue>
    {
    }
}
