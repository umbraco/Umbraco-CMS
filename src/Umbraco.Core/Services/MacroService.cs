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
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Macro Service, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    public class MacroService : RepositoryService, IMacroService
    {

        public MacroService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
        }

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
                return MacroTypes.PartialView;
            }

            if (string.IsNullOrEmpty(macro.ControlType) == false && macro.ControlType.InvariantContains(".ascx"))
                return MacroTypes.UserControl;

            return MacroTypes.Unknown;
        }

        /// <summary>
        /// Gets an <see cref="IMacro"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacro"/> for</param>
        /// <returns>An <see cref="IMacro"/> object</returns>
        public IMacro GetByAlias(string alias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                var q = repository.Query.Where(x => x.Alias == alias);
                var macro = repository.GetByQuery(q).FirstOrDefault();
                uow.Complete();
                return macro;
            }
        }

        ///// <summary>
        ///// Gets a list all available <see cref="IMacro"/> objects
        ///// </summary>
        ///// <param name="aliases">Optional array of aliases to limit the results</param>
        ///// <returns>An enumerable list of <see cref="IMacro"/> objects</returns>
        //public IEnumerable<IMacro> GetAll(params string[] aliases)
        //{
        //    using (var repository = RepositoryFactory.CreateMacroRepository(UowProvider.GetUnitOfWork()))
        //    {
        //        if (aliases.Any())
        //        {
        //            return GetAllByAliases(repository, aliases);
        //        }

        //        return repository.GetAll();
        //    }
        //}

        public IEnumerable<IMacro> GetAll(params int[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                var macros = repository.GetAll(ids);
                uow.Complete();
                return macros;
            }
        }

        public IMacro GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                var macro = repository.Get(id);
                uow.Complete();
                return macro;
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
			if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMacro>(macro), this))
				return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                repository.Delete(macro);
				uow.Complete();
			}

            Deleted.RaiseEvent(new DeleteEventArgs<IMacro>(macro, false), this);
            Audit(AuditType.Delete, "Delete Macro performed by user", userId, -1);
        }

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional Id of the user deleting the macro</param>
        public void Save(IMacro macro, int userId = 0)
        {
	        if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMacro>(macro), this)) 
				return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                repository.AddOrUpdate(macro);
		        uow.Complete();
	        }

            Saved.RaiseEvent(new SaveEventArgs<IMacro>(macro, false), this);
            Audit(AuditType.Save, "Save Macro performed by user", userId, -1);
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

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Complete();
            }
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