using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the MacroService, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    public interface IMacroService : IService
    {

        /// <summary>
        /// Gets an <see cref="IMacro"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacro"/> for</param>
        /// <returns>An <see cref="IMacro"/> object</returns>
        IMacro GetByAlias(string alias);
        
        ///// <summary>
        ///// Gets a list all available <see cref="IMacro"/> objects
        ///// </summary>
        ///// <param name="aliases">Optional array of aliases to limit the results</param>
        ///// <returns>An enumerable list of <see cref="IMacro"/> objects</returns>
        //IEnumerable<IMacro> GetAll(params string[] aliases);

        IEnumerable<IMacro> GetAll(params int[] ids);

        IMacro GetById(int id);

        /// <summary>
        /// Deletes an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the macro</param>
        void Delete(IMacro macro, int userId = 0);

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional id of the user saving the macro</param>
        void Save(IMacro macro, int userId = 0);

        ///// <summary>
        ///// Gets a list all available <see cref="IMacroPropertyType"/> plugins
        ///// </summary>
        ///// <returns>An enumerable list of <see cref="IMacroPropertyType"/> objects</returns>
        //IEnumerable<IMacroPropertyType> GetMacroPropertyTypes();

        ///// <summary>
        ///// Gets an <see cref="IMacroPropertyType"/> by its alias
        ///// </summary>
        ///// <param name="alias">Alias to retrieve an <see cref="IMacroPropertyType"/> for</param>
        ///// <returns>An <see cref="IMacroPropertyType"/> object</returns>
        //IMacroPropertyType GetMacroPropertyTypeByAlias(string alias);

        /// <summary>
        /// Creates and saves an <see cref="IMacro"/>.
        /// Use <see cref="Save"/> method if needed.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="useInEditor"></param>
        /// <param name="cacheDuration"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="controlType"></param>
        /// <param name="controlAssembly"></param>
        /// <param name="xsltPath"></param>
        /// <param name="cacheByPage"></param>
        /// <param name="cacheByMember"></param>
        /// <param name="dontRender"></param>
        /// <param name="scriptPath"></param>
        /// <returns></returns>
        IMacro CreateMacro(int id, bool useInEditor, int cacheDuration, string @alias, string name, string controlType, string controlAssembly, string xsltPath, bool cacheByPage, bool cacheByMember, bool dontRender, string scriptPath, int userId = 0);
    }
}