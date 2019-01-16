using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the Macro Service, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    public class MacroService : ScopeRepositoryService, IMacroService
    {
        private readonly IMacroRepository _macroRepository;
        private readonly IAuditRepository _auditRepository;

        public MacroService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IMacroRepository macroRepository, IAuditRepository auditRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _macroRepository = macroRepository;
            _auditRepository = auditRepository;
        }

        /// <summary>
        /// Gets an <see cref="IMacro"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacro"/> for</param>
        /// <returns>An <see cref="IMacro"/> object</returns>
        public IMacro GetByAlias(string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var q = Query<IMacro>().Where(x => x.Alias == alias);
                return _macroRepository.Get(q).FirstOrDefault();
            }
        }

        public IEnumerable<IMacro> GetAll()
        {
            return GetAll(new int[0]);
        }

        public IEnumerable<IMacro> GetAll(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _macroRepository.GetMany(ids);
            }
        }

        public IEnumerable<IMacro> GetAll(params Guid[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _macroRepository.GetMany(ids);
            }
        }

        public IMacro GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _macroRepository.Get(id);
            }
        }

        public IMacro GetById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _macroRepository.Get(id);
            }
        }

        /// <summary>
        /// Deletes an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the macro</param>
        public void Delete(IMacro macro, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IMacro>(macro);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _macroRepository.Delete(macro);
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
                Audit(AuditType.Delete, userId, -1);

                scope.Complete();
            }
        }

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional Id of the user deleting the macro</param>
        public void Save(IMacro macro, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IMacro>(macro);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(macro.Name))
                {
                    throw new ArgumentException("Cannot save macro with empty name.");
                }

                _macroRepository.Save(macro);
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
                Audit(AuditType.Save, userId, -1);

                scope.Complete();
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

        private void Audit(AuditType type, int userId, int objectId)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, "Macro"));
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
