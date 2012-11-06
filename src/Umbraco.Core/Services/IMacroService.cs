using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the ContentService, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    public interface IMacroService : IService
    {
        //TODO Possibly create import macro method and ToXml?

        /// <summary>
        /// Gets an <see cref="IMacro"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacro"/> for</param>
        /// <returns>An <see cref="IMacro"/> object</returns>
        IMacro GetByAlias(string alias);
        
        /// <summary>
        /// Gets a list all available <see cref="IMacro"/> objects
        /// </summary>
        /// <param name="aliases">Optional array of aliases to limit the results</param>
        /// <returns>An enumerable list of <see cref="IMacro"/> objects</returns>
        IEnumerable<IMacro> GetAll(params string[] aliases);
        
        /// <summary>
        /// Deletes an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to delete</param>
        void Delete(IMacro macro);

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        void Save(IMacro macro);

        /// <summary>
        /// Gets a list all available <see cref="IMacroPropertyType"/> plugins
        /// </summary>
        /// <returns>An enumerable list of <see cref="IMacroPropertyType"/> objects</returns>
        IEnumerable<IMacroPropertyType> GetMacroPropertyTypes();

        /// <summary>
        /// Gets an <see cref="IMacroPropertyType"/> by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacroPropertyType"/> for</param>
        /// <returns>An <see cref="IMacroPropertyType"/> object</returns>
        IMacroPropertyType GetMacroPropertyTypeByAlias(string alias);
    }
}