using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IKeyValueRepository : IReadRepository<string, IKeyValue>, IWriteRepository<IKeyValue>
    {
    }
}
