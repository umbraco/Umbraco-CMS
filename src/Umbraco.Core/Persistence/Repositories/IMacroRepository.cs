using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMacroRepository : IReadWriteQueryRepository<int, IMacro>, IReadRepository<Guid, IMacro>
    {

        //IEnumerable<IMacro> GetAll(params string[] aliases);
        IMacro GetByAlias(string alias);
        IEnumerable<IMacro> GetAllByAlias(string[] aliases);

    }
}
