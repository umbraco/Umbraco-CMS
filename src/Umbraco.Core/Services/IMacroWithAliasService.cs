using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

[Obsolete("This interface will be merged with IMacroService in Umbraco 11")]
public interface IMacroWithAliasService : IMacroService
{
    /// <summary>
    ///     Gets a list of available <see cref="IMacro" /> objects by alias.
    /// </summary>
    /// <param name="aliases">Optional array of aliases to limit the results</param>
    /// <returns>An enumerable list of <see cref="IMacro" /> objects</returns>
    IEnumerable<IMacro> GetAll(params string[] aliases);
}
