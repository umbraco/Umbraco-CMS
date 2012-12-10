using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Macro Service, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    internal class MacroService : IMacroService
    {
        private readonly IUnitOfWork _unitOfWork;
	    private readonly IMacroRepository _macroRepository;

        public MacroService()
            : this(new FileUnitOfWorkProvider())
        {
        }

        public MacroService(IUnitOfWorkProvider provider)
        {
            _unitOfWork = provider.GetUnitOfWork();
	        _macroRepository = RepositoryResolver.Current.Factory.CreateMacroRepository(_unitOfWork);
        }

        public MacroService(IUnitOfWorkProvider provider, bool ensureCachedMacros)
        {
            _unitOfWork = provider.GetUnitOfWork();

            if(ensureCachedMacros)
                EnsureMacroCache();
        }

        /// <summary>
        /// Ensures the macro cache by getting all macros
        /// from the repository and thus caching them.
        /// </summary>
        private void EnsureMacroCache()
        {
            IEnumerable<IMacro> macros = GetAll();
        }

        /// <summary>
        /// Gets an <see cref="IMacro"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacro"/> for</param>
        /// <returns>An <see cref="IMacro"/> object</returns>
        public IMacro GetByAlias(string alias)
        {
            var repository = _macroRepository;
            return repository.Get(alias);
        }

        /// <summary>
        /// Gets a list all available <see cref="IMacro"/> objects
        /// </summary>
        /// <param name="aliases">Optional array of aliases to limit the results</param>
        /// <returns>An enumerable list of <see cref="IMacro"/> objects</returns>
        public IEnumerable<IMacro> GetAll(params string[] aliases)
        {
            var repository = _macroRepository;
            return repository.GetAll(aliases);
        }

        /// <summary>
        /// Deletes an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the macro</param>
        public void Delete(IMacro macro, int userId = -1)
        {
            var repository = _macroRepository;
            repository.Delete(macro);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional Id of the user deleting the macro</param>
        public void Save(IMacro macro, int userId = -1)
        {
            var repository = _macroRepository;
            repository.AddOrUpdate(macro);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Gets a list all available <see cref="IMacroPropertyType"/> plugins
        /// </summary>
        /// <returns>An enumerable list of <see cref="IMacroPropertyType"/> objects</returns>
        public IEnumerable<IMacroPropertyType> GetMacroPropertyTypes()
        {
            return MacroPropertyTypeResolver.Current.MacroPropertyTypes;
        }

        /// <summary>
        /// Gets an <see cref="IMacroPropertyType"/> by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacroPropertyType"/> for</param>
        /// <returns>An <see cref="IMacroPropertyType"/> object</returns>
        public IMacroPropertyType GetMacroPropertyTypeByAlias(string alias)
        {
            return MacroPropertyTypeResolver.Current.MacroPropertyTypes.FirstOrDefault(x => x.Alias == alias);
        }
    }
}