using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Macro Service, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    public class MacroService : ScopeRepositoryService, IMacroService
    {
        public MacroService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
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
                return MacroTypes.PartialView;

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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                var q = repository.QueryT.Where(x => x.Alias == alias);
                return repository.GetByQuery(q).FirstOrDefault();
            }
        }

        public IEnumerable<IMacro> GetAll()
        {
            return GetAll(new int[0]);
        }

        public IEnumerable<IMacro> GetAll(params int[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                return repository.GetAll(ids);
            }
        }

        public IEnumerable<IMacro> GetAll(params Guid[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                return repository.GetAll(ids);
            }
        }

        public IMacro GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                return repository.Get(id);
            }
        }

        public IMacro GetById(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMacroRepository>();
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Deletes an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the macro</param>
        public void Delete(IMacro macro, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IMacro>(macro)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IMacroRepository>();
                repository.Delete(macro);

                uow.Events.Dispatch(Deleted, this, new DeleteEventArgs<IMacro>(macro, false));
                Audit(uow, AuditType.Delete, "Delete Macro performed by user", userId, -1);

                uow.Complete();
            }          
        }

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional Id of the user deleting the macro</param>
        public void Save(IMacro macro, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMacro>(macro)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IMacroRepository>();
                repository.AddOrUpdate(macro);

                uow.Events.Dispatch(Saved, this, new SaveEventArgs<IMacro>(macro, false));
                Audit(uow, AuditType.Save, "Save Macro performed by user", userId, -1);

                uow.Complete();
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
            var repo = uow.CreateRepository<IAuditRepository>();
            repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
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