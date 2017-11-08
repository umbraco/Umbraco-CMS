using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Localization Service, which is an easy access to operations involving <see cref="Language"/> and <see cref="DictionaryItem"/>
    /// </summary>
    public class LocalizationService : ScopeRepositoryService, ILocalizationService
    {
        public LocalizationService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        { }

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
            if (item == null) throw new ArgumentNullException("item");
            if (language == null) throw new ArgumentNullException("language");

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
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                if (parentId.HasValue && parentId.Value != Guid.Empty)
                {
                    var parent = GetDictionaryItemById(parentId.Value);
                    if (parent == null)
                    {
                        throw new ArgumentException("No parent dictionary item was found with id " + parentId.Value);
                    }
                }

                var item = new DictionaryItem(parentId, key);

                if (defaultValue.IsNullOrWhiteSpace() == false)
                {
                    var langs = GetAllLanguages();
                    var translations = langs.Select(language => new DictionaryTranslation(language, defaultValue)).Cast<IDictionaryTranslation>().ToList();

                    item.Translations = translations;
                }

                var saveEventArgs = new SaveEventArgs<IDictionaryItem>(item);
                if (uow.Events.DispatchCancelable(SavingDictionaryItem, this, saveEventArgs))
                {
                    uow.Commit();
                    return item;
                }

                repository.AddOrUpdate(item);
                uow.Commit();

                //ensure the lazy Language callback is assigned
                EnsureDictionaryItemLanguageCallback(item);
                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(SavedDictionaryItem, this, saveEventArgs);

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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                //ensure the lazy Language callback is assigned
                var item = repository.Get(id);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                //ensure the lazy Language callback is assigned
                var item = repository.Get(id);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                //ensure the lazy Language callback is assigned
                var item = repository.Get(key);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ParentId == parentId);
                //ensure the lazy Language callback is assigned
                var items = repository.GetByQuery(query).ToArray();
                //ensure the lazy Language callback is assigned
                items.ForEach(EnsureDictionaryItemLanguageCallback);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                //ensure the lazy Language callback is assigned
                var items = repository.GetDictionaryItemDescendants(parentId).ToArray();
                //ensure the lazy Language callback is assigned
                items.ForEach(EnsureDictionaryItemLanguageCallback);
                return items;
            }
        }

        /// <summary>
        /// Gets the root/top <see cref="IDictionaryItem"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetRootDictionaryItems()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ParentId == null);
                //ensure the lazy Language callback is assigned
                var items = repository.GetByQuery(query).ToArray();
                //ensure the lazy Language callback is assigned
                items.ForEach(EnsureDictionaryItemLanguageCallback);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                return repository.Get(key) != null;
            }
        }

        /// <summary>
        /// Saves a <see cref="IDictionaryItem"/> object
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to save</param>
        /// <param name="userId">Optional id of the user saving the dictionary item</param>
        public void Save(IDictionaryItem dictionaryItem, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingDictionaryItem, this, new SaveEventArgs<IDictionaryItem>(dictionaryItem)))
                {
                    uow.Commit();
                    return;
                }
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                repository.AddOrUpdate(dictionaryItem);

                // ensure the lazy Language callback is assigned (not using the uow)
                EnsureDictionaryItemLanguageCallback(dictionaryItem);
                uow.Events.Dispatch(SavedDictionaryItem, this, new SaveEventArgs<IDictionaryItem>(dictionaryItem, false));

                Audit(uow, AuditType.Save, "Save DictionaryItem performed by user", userId, dictionaryItem.Id);
                uow.Commit();
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
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var deleteEventArgs = new DeleteEventArgs<IDictionaryItem>(dictionaryItem);
                if (uow.Events.DispatchCancelable(DeletingDictionaryItem, this, deleteEventArgs))
                {
                    uow.Commit();
                    return;
                }

                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                repository.Delete(dictionaryItem);
                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedDictionaryItem, this, deleteEventArgs);

                Audit(uow, AuditType.Delete, "Delete DictionaryItem performed by user", userId, dictionaryItem.Id);
                uow.Commit();
            }
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its id
        /// </summary>
        /// <param name="id">Id of the <see cref="Language"/></param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateLanguageRepository(uow);
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its culture code
        /// </summary>
        /// <param name="cultureName">Culture Name - also refered to as the Friendly name</param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageByCultureCode(string cultureName)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateLanguageRepository(uow);
                return repository.GetByCultureName(cultureName);
            }
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its iso code
        /// </summary>
        /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageByIsoCode(string isoCode)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateLanguageRepository(uow);
                return repository.GetByIsoCode(isoCode);
            }
        }

        /// <summary>
        /// Gets all available languages
        /// </summary>
        /// <returns>An enumerable list of <see cref="ILanguage"/> objects</returns>
        public IEnumerable<ILanguage> GetAllLanguages()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateLanguageRepository(uow);
                return repository.GetAll();
            }
        }

        /// <summary>
        /// Saves a <see cref="ILanguage"/> object
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to save</param>
        /// <param name="userId">Optional id of the user saving the language</param>
        public void Save(ILanguage language, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<ILanguage>(language);
                if (uow.Events.DispatchCancelable(SavingLanguage, this, saveEventArgs))
                {
                    uow.Commit();
                    return;
                }
                var repository = RepositoryFactory.CreateLanguageRepository(uow);
                repository.AddOrUpdate(language);
                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(SavedLanguage, this, saveEventArgs);

                Audit(uow, AuditType.Save, "Save Language performed by user", userId, language.Id);
                uow.Commit();
            }
        }

        /// <summary>
        /// Deletes a <see cref="ILanguage"/> by removing it (but not its usages) from the db
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the language</param>
        public void Delete(ILanguage language, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var deleteEventArgs = new DeleteEventArgs<ILanguage>(language);
                if (uow.Events.DispatchCancelable(DeletingLanguage, this, deleteEventArgs))
                {
                    uow.Commit();
                    return;
                }

                var repository = RepositoryFactory.CreateLanguageRepository(uow);
                repository.Delete(language);

                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedLanguage, this, deleteEventArgs);

                Audit(uow, AuditType.Delete, "Delete Language performed by user", userId, language.Id);
                uow.Commit();
            }
        }

        private void Audit(IScopeUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var repository = RepositoryFactory.CreateAuditRepository(uow);
            repository.AddOrUpdate(new AuditItem(objectId, message, type, userId));
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
            {
                trans.GetLanguage = GetLanguageById;
            }
        }

        public Dictionary<string, Guid> GetDictionaryItemKeyMap()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDictionaryRepository(uow);
                return repository.GetDictionaryItemKeyMap();
            }
        }

        #region Event Handlers
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