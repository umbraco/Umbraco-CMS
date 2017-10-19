using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Macro Service, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    public class MacroService : ScopeRepositoryService, IMacroService
    {
        public MacroService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        { }

        /// <summary>
        /// Returns an enum <see cref="MacroTypes"/> based on the properties on the Macro
        /// </summary>
        /// <returns><see cref="MacroTypes"/></returns>
        internal static MacroTypes GetMacroType(IMacro macro)
        {
            if (string.IsNullOrEmpty(macro.XsltPath) == false)
                return MacroTypes.Xslt;

            if (string.IsNullOrEmpty(macro.ScriptPath) == false)
            {
                //we need to check if the file path saved is a virtual path starting with ~/Views/MacroPartials, if so then this is 
                //a partial view macro, not a script macro
                //we also check if the file exists in ~/App_Plugins/[Packagename]/Views/MacroPartials, if so then it is also a partial view.
                return (macro.ScriptPath.InvariantStartsWith(SystemDirectories.MvcViews + "/MacroPartials/")
                        || (Regex.IsMatch(macro.ScriptPath, "~/App_Plugins/.+?/Views/MacroPartials", RegexOptions.Compiled | RegexOptions.IgnoreCase)))
                           ? MacroTypes.PartialView
                           : MacroTypes.Script;
            }

            if (string.IsNullOrEmpty(macro.ControlType) == false && macro.ControlType.InvariantContains(".ascx"))
                return MacroTypes.UserControl;

            if (string.IsNullOrEmpty(macro.ControlType) == false && string.IsNullOrEmpty(macro.ControlAssembly) == false)
                return MacroTypes.CustomControl;

            return MacroTypes.Unknown;
        }

        /// <summary>
        /// Gets an <see cref="IMacro"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacro"/> for</param>
        /// <returns>An <see cref="IMacro"/> object</returns>
        public IMacro GetByAlias(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroRepository(uow);
                var q = new Query<IMacro>();
                q.Where(macro => macro.Alias == alias);
                return repository.GetByQuery(q).FirstOrDefault();
            }
        }

        ///// <summary>
        ///// Gets a list all available <see cref="IMacro"/> objects
        ///// </summary>
        ///// <param name="aliases">Optional array of aliases to limit the results</param>
        ///// <returns>An enumerable list of <see cref="IMacro"/> objects</returns>
        //public IEnumerable<IMacro> GetAll(params string[] aliases)
        //{
        //    using (var repository = RepositoryFactory.CreateMacroRepository(UowProvider.GetReadOnlyUnitOfWork()))
        //    {
        //        if (aliases.Any())
        //        {
        //            return GetAllByAliases(repository, aliases);
        //        }

        //        return repository.GetAll();
        //    }
        //}

        public IEnumerable<IMacro> GetAll()
        {
            return GetAll(new int[0]);
        }

        public IEnumerable<IMacro> GetAll(params int[] ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroRepository(uow);
                return repository.GetAll(ids);
            }
        }

        public IEnumerable<IMacro> GetAll(params Guid[] ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroRepository(uow);
                return repository.GetAll(ids);
            }
        }

        public IMacro GetById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroRepository(uow);
                return repository.Get(id);
            }
        }

        public IMacro GetById(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMacroRepository(uow);
                return repository.Get(id);
            }
        }

        //private IEnumerable<IMacro> GetAllByAliases(IMacroRepository repo, IEnumerable<string> aliases)
        //{
        //    foreach (var alias in aliases)
        //    {
        //        var q = new Query<IMacro>();
        //        q.Where(macro => macro.Alias == alias);
        //        yield return repo.GetByQuery(q).FirstOrDefault();
        //    }
        //}

        /// <summary>
        /// Deletes an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the macro</param>
        public void Delete(IMacro macro, int userId = 0)
        {
			using (var uow = UowProvider.GetUnitOfWork())
            {
                var deleteEventArgs = new DeleteEventArgs<IMacro>(macro);
                if (uow.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    uow.Commit();
                    return;
                }
			    var repository = RepositoryFactory.CreateMacroRepository(uow);
				repository.Delete(macro);
                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(Deleted, this, deleteEventArgs);

                Audit(uow, AuditType.Delete, "Delete Macro performed by user", userId, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional Id of the user deleting the macro</param>
        public void Save(IMacro macro, int userId = 0)
        {            
			using (var uow = UowProvider.GetUnitOfWork())
	        {
	            var saveEventArgs = new SaveEventArgs<IMacro>(macro);
	            if (uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    uow.Commit();
                    return;
                }

                if (string.IsNullOrWhiteSpace(macro.Name))
                {
                    throw new ArgumentException("Cannot save macro with empty name.");
                }

                var repository = RepositoryFactory.CreateMacroRepository(uow);
		        repository.AddOrUpdate(macro);
	            saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(Saved, this, saveEventArgs);

                Audit(uow, AuditType.Save, "Save Macro performed by user", userId, -1);
                uow.Commit();
            }
        }

        ///// <summary>
        ///// Gets a list all available <see cref="IMacroPropertyType"/> plugins
        ///// </summary>
        ///// <returns>An enumerable list of <see cref="IMacroPropertyType"/> objects</returns>
        //public IEnumerable<IMacroPropertyType> GetMacroPropertyTypes()
        //{
        //    return MacroPropertyTypeResolver.Current.MacroPropertyTypes;
        //}

        ///// <summary>
        ///// Gets an <see cref="IMacroPropertyType"/> by its alias
        ///// </summary>
        ///// <param name="alias">Alias to retrieve an <see cref="IMacroPropertyType"/> for</param>
        ///// <returns>An <see cref="IMacroPropertyType"/> object</returns>
        //public IMacroPropertyType GetMacroPropertyTypeByAlias(string alias)
        //{
        //    return MacroPropertyTypeResolver.Current.MacroPropertyTypes.FirstOrDefault(x => x.Alias == alias);
        //}

        private void Audit(IScopeUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var repository = RepositoryFactory.CreateAuditRepository(uow);
            repository.AddOrUpdate(new AuditItem(objectId, message, type, userId));
        }

        #region Event Handlers
        /// <summary>
        /// Occurs before Delete
        /// </summary>
		public static event TypedEventHandler<IMacroService, DeleteEventArgs<IMacro>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
		public static event TypedEventHandler<IMacroService, DeleteEventArgs<IMacro>> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
		public static event TypedEventHandler<IMacroService, SaveEventArgs<IMacro>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
		public static event TypedEventHandler<IMacroService, SaveEventArgs<IMacro>> Saved;
        #endregion

        
    }
}