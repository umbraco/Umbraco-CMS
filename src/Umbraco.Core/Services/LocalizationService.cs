using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Localization Service, which is an easy access to operations involving <see cref="Language"/> and <see cref="DictionaryItem"/>
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
	    private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
	    private static readonly Guid RootParentId = new Guid("41c7638d-f529-4bff-853e-59a0c2fb1bde");

        public LocalizationService()
            : this(new RepositoryFactory())
        {}

        public LocalizationService(RepositoryFactory repositoryFactory)
			: this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        public LocalizationService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

		public LocalizationService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
			_repositoryFactory = repositoryFactory;
		    _uowProvider = provider;
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Int32"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemById(int id)
        {
            using (var repository = _repositoryFactory.CreateDictionaryRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Guid"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="DictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemById(Guid id)
        {
            using (var repository = _repositoryFactory.CreateDictionaryRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IDictionaryItem>.Builder.Where(x => x.Key == id);
                var items = repository.GetByQuery(query);

                return items.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its key
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemByKey(string key)
        {
            using (var repository = _repositoryFactory.CreateDictionaryRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ItemKey == key);
                var items = repository.GetByQuery(query);

                return items.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId)
        {
            using (var repository = _repositoryFactory.CreateDictionaryRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ParentId == parentId);
                var items = repository.GetByQuery(query);

                return items;
            }
        }

        /// <summary>
        /// Gets the root/top <see cref="IDictionaryItem"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetRootDictionaryItems()
        {
            using (var repository = _repositoryFactory.CreateDictionaryRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ParentId == RootParentId);
                var items = repository.GetByQuery(query);

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
            using (var repository = _repositoryFactory.CreateDictionaryRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IDictionaryItem>.Builder.Where(x => x.ItemKey == key);
                var items = repository.GetByQuery(query);

                return items.Any();
            }
        }

        /// <summary>
        /// Saves a <see cref="IDictionaryItem"/> object
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to save</param>
        /// <param name="userId">Optional id of the user saving the dictionary item</param>
        public void Save(IDictionaryItem dictionaryItem, int userId = 0)
        {
	        if (SavingDictionaryItem.IsRaisedEventCancelled(new SaveEventArgs<IDictionaryItem>(dictionaryItem), this)) 
				return;
	        
			var uow = _uowProvider.GetUnitOfWork();
	        using (var repository = _repositoryFactory.CreateDictionaryRepository(uow))
	        {
		        repository.AddOrUpdate(dictionaryItem);
		        uow.Commit();
	        }

            SavedDictionaryItem.RaiseEvent(new SaveEventArgs<IDictionaryItem>(dictionaryItem, false), this);

	        Audit.Add(AuditTypes.Save, "Save DictionaryItem performed by user", userId, dictionaryItem.Id);
        }

        /// <summary>
        /// Deletes a <see cref="IDictionaryItem"/> object and its related translations
        /// as well as its children.
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the dictionary item</param>
        public void Delete(IDictionaryItem dictionaryItem, int userId = 0)
        {
	        if (DeletingDictionaryItem.IsRaisedEventCancelled(new DeleteEventArgs<IDictionaryItem>(dictionaryItem), this)) 
				return;
	        
			var uow = _uowProvider.GetUnitOfWork();
	        using (var repository = _repositoryFactory.CreateDictionaryRepository(uow))
	        {
		        //NOTE: The recursive delete is done in the repository
		        repository.Delete(dictionaryItem);
		        uow.Commit();
	        }

            DeletedDictionaryItem.RaiseEvent(new DeleteEventArgs<IDictionaryItem>(dictionaryItem, false), this);

	        Audit.Add(AuditTypes.Delete, "Delete DictionaryItem performed by user", userId, dictionaryItem.Id);
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its id
        /// </summary>
        /// <param name="id">Id of the <see cref="Language"/></param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageById(int id)
        {
            using (var repository = _repositoryFactory.CreateLanguageRepository(_uowProvider.GetUnitOfWork()))
            {
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
            using (var repository = _repositoryFactory.CreateLanguageRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<ILanguage>.Builder.Where(x => x.CultureName == cultureName);
                var items = repository.GetByQuery(query);

                return items.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its iso code
        /// </summary>
        /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageByIsoCode(string isoCode)
        {
            using (var repository = _repositoryFactory.CreateLanguageRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<ILanguage>.Builder.Where(x => x.IsoCode == isoCode);
                var items = repository.GetByQuery(query);

                return items.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets all available languages
        /// </summary>
        /// <returns>An enumerable list of <see cref="ILanguage"/> objects</returns>
        public IEnumerable<ILanguage> GetAllLanguages()
        {
            using (var repository = _repositoryFactory.CreateLanguageRepository(_uowProvider.GetUnitOfWork()))
            {
                var languages = repository.GetAll();
                return languages;
            }
        }

        /// <summary>
        /// Saves a <see cref="ILanguage"/> object
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to save</param>
        /// <param name="userId">Optional id of the user saving the language</param>
        public void Save(ILanguage language, int userId = 0)
        {
	        if (SavingLanguage.IsRaisedEventCancelled(new SaveEventArgs<ILanguage>(language), this)) 
				return;
	        
			var uow = _uowProvider.GetUnitOfWork();
	        using (var repository = _repositoryFactory.CreateLanguageRepository(uow))
	        {
		        repository.AddOrUpdate(language);
		        uow.Commit();
	        }

            SavedLanguage.RaiseEvent(new SaveEventArgs<ILanguage>(language, false), this);

	        Audit.Add(AuditTypes.Save, "Save Language performed by user", userId, language.Id);
        }

        /// <summary>
        /// Deletes a <see cref="ILanguage"/> by removing it (but not its usages) from the db
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the language</param>
        public void Delete(ILanguage language, int userId = 0)
        {
            if (DeletingLanguage.IsRaisedEventCancelled(new DeleteEventArgs<ILanguage>(language), this)) 
				return;
	        
			var uow = _uowProvider.GetUnitOfWork();
	        using (var repository = _repositoryFactory.CreateLanguageRepository(uow))
	        {
		        //NOTE: There isn't any constraints in the db, so possible references aren't deleted
		        repository.Delete(language);
		        uow.Commit();
	        }

            DeletedLanguage.RaiseEvent(new DeleteEventArgs<ILanguage>(language, false), this);

	        Audit.Add(AuditTypes.Delete, "Delete Language performed by user", userId, language.Id);
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