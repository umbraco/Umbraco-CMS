using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IMacroRepository : IReadWriteQueryRepository<int, IMacro>, IReadRepository<Guid, IMacro>
    {
        IMacro? GetByAlias(string alias);

        IEnumerable<IMacro> GetAllByAlias(string[] aliases);
    }
}
