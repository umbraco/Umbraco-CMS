using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Defines the Macro Repository, which exposes CRUD operations for <see cref="IMacro"/>
    /// </summary>
    /// <remarks>Uses string Alias as the Id type</remarks>
    public interface IMacroRepository : IRepository<string, IMacro>
    {
         
    }
}