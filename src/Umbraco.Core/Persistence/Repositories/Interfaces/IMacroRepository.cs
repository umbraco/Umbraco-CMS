using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IMacroRepository : IRepositoryQueryable<int, IMacro>
    {

        //IEnumerable<IMacro> GetAll(params string[] aliases);

    }
}