using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

[Obsolete("This interface will be merged with IMacroRepository in Umbraco 11")]
public interface IMacroWithAliasRepository : IMacroRepository
{
    IMacro? GetByAlias(string alias);

    IEnumerable<IMacro> GetAllByAlias(string[] aliases);
}
