using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IMacroRepository : IReadWriteQueryRepository<int, IMacro>, IReadRepository<Guid, IMacro>
    {

        //IEnumerable<IMacro> GetAll(params string[] aliases);

    }
}
