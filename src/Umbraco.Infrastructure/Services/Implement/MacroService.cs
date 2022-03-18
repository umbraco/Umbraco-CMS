using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Represents the Macro Service, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    internal class MacroService : RepositoryService, IMacroWithAliasService
    {
        private readonly IMacroRepository _macroRepository;
        private readonly IAuditRepository _auditRepository;

        public MacroService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IMacroRepository macroRepository, IAuditRepository auditRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
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
            if (_macroRepository is not IMacroWithAliasRepository macroWithAliasRepository)
            {
                return GetAll().FirstOrDefault(x=>x.Alias == alias);
            }

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return macroWithAliasRepository.GetByAlias(alias);
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

        public IEnumerable<IMacro> GetAll(params string[] aliases)
        {
            if (_macroRepository is not IMacroWithAliasRepository macroWithAliasRepository)
            {
                var hashset = new HashSet<string>(aliases);
                return GetAll().Where(x=> hashset.Contains(x.Alias));
            }

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return macroWithAliasRepository.GetAllByAlias(aliases);
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
        public void Delete(IMacro macro, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            using (IScope scope = ScopeProvider.CreateScope())
            {
                EventMessages eventMessages = EventMessagesFactory.Get();
                var deletingNotification = new MacroDeletingNotification(macro, eventMessages);
                if (scope.Notifications.PublishCancelable(deletingNotification))
                {
                    scope.Complete();
                    return;
                }

                _macroRepository.Delete(macro);

                scope.Notifications.Publish(new MacroDeletedNotification(macro, eventMessages).WithStateFrom(deletingNotification));
                Audit(AuditType.Delete, userId, -1);

                scope.Complete();
            }
        }

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional Id of the user deleting the macro</param>
        public void Save(IMacro macro, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            using (IScope scope = ScopeProvider.CreateScope())
            {
                EventMessages eventMessages = EventMessagesFactory.Get();
                var savingNotification = new MacroSavingNotification(macro, eventMessages);

                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(macro.Name))
                {
                    throw new ArgumentException("Cannot save macro with empty name.");
                }

                _macroRepository.Save(macro);

                scope.Notifications.Publish(new MacroSavedNotification(macro, eventMessages).WithStateFrom(savingNotification));
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
    }
}
