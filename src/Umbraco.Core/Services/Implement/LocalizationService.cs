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
    /// Represents the Localization Service, which is an easy access to operations involving <see cref="Language"/> and <see cref="DictionaryItem"/>
    /// </summary>
    public class LocalizationService : ScopeRepositoryService, ILocalizationService
    {
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IAuditRepository _auditRepository;

        public LocalizationService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IDictionaryRepository dictionaryRepository, IAuditRepository auditRepository, ILanguageRepository languageRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _dictionaryRepository = dictionaryRepository;
            _auditRepository = auditRepository;
            _languageRepository = languageRepository;
        }

        /// <summary>
        /// Adds or updates a translation for a dictionary item and language
        /// </summary>
        /// <param name="item"></param>
        /// <param name="language"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// This does not save the item, that needs to be done explicitly
        /// </remarks>
        public void AddOrUpdateDictionaryValue(IDictionaryItem item, ILanguage language, string value)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (language == null) throw new ArgumentNullException(nameof(language));

            var existing = item.Translations.FirstOrDefault(x => x.Language.Id == language.Id);
            if (existing != null)
            {
                existing.Value = value;
            }
            else
            {
                item.Translations = new List<IDictionaryTranslation>(item.Translations)
                {
                    new DictionaryTranslation(language, value)
                };
            }
        }

        /// <summary>
        /// Creates and saves a new dictionary item and assigns a value to all languages if defaultValue is specified.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parentId"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public IDictionaryItem CreateDictionaryItemWithIdentity(string key, Guid? parentId, string defaultValue = null)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                //validate the parent

                if (parentId.HasValue && parentId.Value != Guid.Empty)
                {
                    var parent = GetDictionaryItemById(parentId.Value);
                    if (parent == null)
                        throw new ArgumentException($"No parent dictionary item was found with id {parentId.Value}.");
                }

                var item = new DictionaryItem(parentId, key);

                if (defaultValue.IsNullOrWhiteSpace() == false)
                {
                    var langs = GetAllLanguages();
                    var translations = langs.Select(language => new DictionaryTranslation(language, defaultValue))
                        .Cast<IDictionaryTranslation>()
                        .ToList();

                    item.Translations = translations;
                }
                var saveEventArgs = new SaveEventArgs<IDictionaryItem>(item);

                if (scope.Events.DispatchCancelable(SavingDictionaryItem, this, saveEventArgs))
                {
                    scope.Complete();
                    return item;
                }
                _dictionaryRepository.Save(item);

                // ensure the lazy Language callback is assigned
                EnsureDictionaryItemLanguageCallback(item);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedDictionaryItem, this, saveEventArgs);

                scope.Complete();

                return item;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Int32"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var item = _dictionaryRepository.Get(id);
                //ensure the lazy Language callback is assigned
                EnsureDictionaryItemLanguageCallback(item);
                return item;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Guid"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="DictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var item = _dictionaryRepository.Get(id);
                //ensure the lazy Language callback is assigned
                EnsureDictionaryItemLanguageCallback(item);
                return item;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its key
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemByKey(string key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var item = _dictionaryRepository.Get(key);
                //ensure the lazy Language callback is assigned
                EnsureDictionaryItemLanguageCallback(item);
                return item;
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDictionaryItem>().Where(x => x.ParentId == parentId);
                var items = _dictionaryRepository.Get(query).ToArray();
                //ensure the lazy Language callback is assigned
                foreach (var item in items)
                    EnsureDictionaryItemLanguageCallback(item);
                return items;
            }
        }

        /// <summary>
        /// Gets a list of descendants for a <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="parentId">Id of the parent, null will return all dictionary items</param>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var items = _dictionaryRepository.GetDictionaryItemDescendants(parentId).ToArray();
                //ensure the lazy Language callback is assigned
                foreach (var item in items)
                    EnsureDictionaryItemLanguageCallback(item);
                return items;
            }
        }

        /// <summary>
        /// Gets the root/top <see cref="IDictionaryItem"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetRootDictionaryItems()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDictionaryItem>().Where(x => x.ParentId == null);
                var items = _dictionaryRepository.Get(query).ToArray();
                //ensure the lazy Language callback is assigned
                foreach (var item in items)
                    EnsureDictionaryItemLanguageCallback(item);
                return items;
            }
        }

        /// <summary>
        /// Checks if a <see cref="IDictionaryItem"/> with given key exists
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns>True if a <see cref="IDictionaryItem"/> exists, otherwise false</returns>
        public bool DictionaryItemExists(string key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var item = _dictionaryRepository.Get(key);
                return item != null;
            }
        }

        /// <summary>
        /// Saves a <see cref="IDictionaryItem"/> object
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to save</param>
        /// <param name="userId">Optional id of the user saving the dictionary item</param>
        public void Save(IDictionaryItem dictionaryItem, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Events.DispatchCancelable(SavingDictionaryItem, this, new SaveEventArgs<IDictionaryItem>(dictionaryItem)))
                {
                    scope.Complete();
                    return;
                }

                _dictionaryRepository.Save(dictionaryItem);

                // ensure the lazy Language callback is assigned
                // ensure the lazy Language callback is assigned

                EnsureDictionaryItemLanguageCallback(dictionaryItem);
                scope.Events.Dispatch(SavedDictionaryItem, this, new SaveEventArgs<IDictionaryItem>(dictionaryItem, false));

                Audit(AuditType.Save, "Save DictionaryItem", userId, dictionaryItem.Id, "DictionaryItem");
                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes a <see cref="IDictionaryItem"/> object and its related translations
        /// as well as its children.
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the dictionary item</param>
        public void Delete(IDictionaryItem dictionaryItem, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IDictionaryItem>(dictionaryItem);
                if (scope.Events.DispatchCancelable(DeletingDictionaryItem, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _dictionaryRepository.Delete(dictionaryItem);
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedDictionaryItem, this, deleteEventArgs);

                Audit(AuditType.Delete, "Delete DictionaryItem", userId, dictionaryItem.Id, "DictionaryItem");

                scope.Complete();
            }
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its id
        /// </summary>
        /// <param name="id">Id of the <see cref="Language"/></param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _languageRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its iso code
        /// </summary>
        /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageByIsoCode(string isoCode)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _languageRepository.GetByIsoCode(isoCode);
            }
        }

        /// <inheritdoc />
        public int? GetLanguageIdByIsoCode(string isoCode)
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _languageRepository.GetIdByIsoCode(isoCode);
            }
        }

        /// <inheritdoc />
        public string GetLanguageIsoCodeById(int id)
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _languageRepository.GetIsoCodeById(id);
            }
        }

        /// <inheritdoc />
        public string GetDefaultLanguageIsoCode()
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _languageRepository.GetDefaultIsoCode();
            }
        }

        /// <inheritdoc />
        public int? GetDefaultLanguageId()
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _languageRepository.GetDefaultId();
            }
        }

        /// <summary>
        /// Gets all available languages
        /// </summary>
        /// <returns>An enumerable list of <see cref="ILanguage"/> objects</returns>
        public IEnumerable<ILanguage> GetAllLanguages()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _languageRepository.GetMany();
            }
        }

        /// <summary>
        /// Saves a <see cref="ILanguage"/> object
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to save</param>
        /// <param name="userId">Optional id of the user saving the language</param>
        public void Save(ILanguage language, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                // write-lock languages to guard against race conds when dealing with default language
                scope.WriteLock(Constants.Locks.Languages);

                // look for cycles - within write-lock
                if (language.FallbackLanguageId.HasValue)
                {
                    var languages = _languageRepository.GetMany().ToDictionary(x => x.Id, x => x);
                    if (!languages.ContainsKey(language.FallbackLanguageId.Value))
                        throw new InvalidOperationException($"Cannot save language {language.IsoCode} with fallback id={language.FallbackLanguageId.Value} which is not a valid language id.");
                    if (CreatesCycle(language, languages))
                        throw new InvalidOperationException($"Cannot save language {language.IsoCode} with fallback {languages[language.FallbackLanguageId.Value].IsoCode} as it would create a fallback cycle.");
                }

                var saveEventArgs = new SaveEventArgs<ILanguage>(language);
                if (scope.Events.DispatchCancelable(SavingLanguage, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _languageRepository.Save(language);
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedLanguage, this, saveEventArgs);

                Audit(AuditType.Save, "Save Language", userId, language.Id, ObjectTypes.GetName(UmbracoObjectTypes.Language));

                scope.Complete();
            }
        }

        private bool CreatesCycle(ILanguage language, IDictionary<int, ILanguage> languages)
        {
            // a new language is not referenced yet, so cannot be part of a cycle
            if (!language.HasIdentity) return false;

            var id = language.FallbackLanguageId;
            while (true) // assuming languages does not already contains a cycle, this must end
            {
                if (!id.HasValue) return false; // no fallback means no cycle
                if (id.Value == language.Id) return true; // back to language = cycle!
                id = languages[id.Value].FallbackLanguageId; // else keep chaining
            }
        }

        /// <summary>
        /// Deletes a <see cref="ILanguage"/> by removing it (but not its usages) from the db
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the language</param>
        public void Delete(ILanguage language, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                // write-lock languages to guard against race conds when dealing with default language
                scope.WriteLock(Constants.Locks.Languages);

                var deleteEventArgs = new DeleteEventArgs<ILanguage>(language);
                if (scope.Events.DispatchCancelable(DeletingLanguage, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                // NOTE: Other than the fall-back language, there aren't any other constraints in the db, so possible references aren't deleted
                _languageRepository.Delete(language);
                deleteEventArgs.CanCancel = false;

                scope.Events.Dispatch(DeletedLanguage, this, deleteEventArgs);

                Audit(AuditType.Delete, "Delete Language", userId, language.Id, ObjectTypes.GetName(UmbracoObjectTypes.Language));
                scope.Complete();
            }
        }

        private void Audit(AuditType type, string message, int userId, int objectId, string entityType)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, message));
        }

        /// <summary>
        /// This is here to take care of a hack - the DictionaryTranslation model contains an ILanguage reference which we don't want but
        /// we cannot remove it because it would be a large breaking change, so we need to make sure it's resolved lazily. This is because
        /// if developers have a lot of dictionary items and translations, the caching and cloning size gets much much larger because of
        /// the large object graphs. So now we don't cache or clone the attached ILanguage
        /// </summary>
        private void EnsureDictionaryItemLanguageCallback(IDictionaryItem d)
        {
            var item = d as DictionaryItem;
            if (item == null) return;

            item.GetLanguage = GetLanguageById;
            foreach (var trans in item.Translations.OfType<DictionaryTranslation>())
                trans.GetLanguage = GetLanguageById;
        }

        public Dictionary<string, Guid> GetDictionaryItemKeyMap()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dictionaryRepository.GetDictionaryItemKeyMap();
            }
        }

        #region Events
        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, DeleteEventArgs<ILanguage>> DeletingLanguage;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, DeleteEventArgs<ILanguage>> DeletedLanguage;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, DeleteEventArgs<IDictionaryItem>> DeletingDictionaryItem;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, DeleteEventArgs<IDictionaryItem>> DeletedDictionaryItem;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, SaveEventArgs<IDictionaryItem>> SavingDictionaryItem;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, SaveEventArgs<IDictionaryItem>> SavedDictionaryItem;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, SaveEventArgs<ILanguage>> SavingLanguage;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<ILocalizationService, SaveEventArgs<ILanguage>> SavedLanguage;
        #endregion
    }
}
