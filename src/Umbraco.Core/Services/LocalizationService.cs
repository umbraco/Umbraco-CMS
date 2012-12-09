using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Localization Service, which is an easy access to operations involving <see cref="Language"/> and <see cref="DictionaryItem"/>
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private static readonly Guid RootParentId = new Guid("41c7638d-f529-4bff-853e-59a0c2fb1bde");
        private readonly IUnitOfWork _unitOfWork;
	    private readonly IDictionaryRepository _dictionaryRepository;
	    private readonly ILanguageRepository _languageRepository;

        public LocalizationService() : this(new PetaPocoUnitOfWorkProvider())
        {
        }

        public LocalizationService(IUnitOfWorkProvider provider)
        {
            _unitOfWork = provider.GetUnitOfWork();
	        _dictionaryRepository = RepositoryResolver.Current.Factory.CreateDictionaryRepository(_unitOfWork);
	        _languageRepository = RepositoryResolver.Current.Factory.CreateLanguageRepository(_unitOfWork);
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Int32"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemById(int id)
        {
            var repository = _dictionaryRepository;
            return repository.Get(id);
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Guid"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="DictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemById(Guid id)
        {
            var repository = _dictionaryRepository;

            var query = Query<IDictionaryItem>.Builder.Where(x => x.Key == id);
            var items = repository.GetByQuery(query);

            return items.FirstOrDefault();
        }

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its key
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        public IDictionaryItem GetDictionaryItemByKey(string key)
        {
            var repository = _dictionaryRepository;

            var query = Query<IDictionaryItem>.Builder.Where(x => x.ItemKey == key);
            var items = repository.GetByQuery(query);

            return items.FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId)
        {
            var repository = _dictionaryRepository;

            var query = Query<IDictionaryItem>.Builder.Where(x => x.ParentId == parentId);
            var items = repository.GetByQuery(query);

            return items;
        }

        /// <summary>
        /// Gets the root/top <see cref="IDictionaryItem"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        public IEnumerable<IDictionaryItem> GetRootDictionaryItems()
        {
            var repository = _dictionaryRepository;

            var query = Query<IDictionaryItem>.Builder.Where(x => x.ParentId == RootParentId);
            var items = repository.GetByQuery(query);

            return items;
        }

        /// <summary>
        /// Checks if a <see cref="IDictionaryItem"/> with given key exists
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns>True if a <see cref="IDictionaryItem"/> exists, otherwise false</returns>
        public bool DictionaryItemExists(string key)
        {
            var repository = _dictionaryRepository;

            var query = Query<IDictionaryItem>.Builder.Where(x => x.ItemKey == key);
            var items = repository.GetByQuery(query);

            return items.Any();
        }

        /// <summary>
        /// Saves a <see cref="IDictionaryItem"/> object
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to save</param>
        public void Save(IDictionaryItem dictionaryItem)
        {
            var repository = _dictionaryRepository;

            repository.AddOrUpdate(dictionaryItem);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a <see cref="IDictionaryItem"/> object and its related translations
        /// as well as its children.
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to delete</param>
        public void Delete(IDictionaryItem dictionaryItem)
        {
            var repository = _dictionaryRepository;

            //NOTE: The recursive delete is done in the repository
            repository.Delete(dictionaryItem);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its id
        /// </summary>
        /// <param name="id">Id of the <see cref="Language"/></param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageById(int id)
        {
            var repository = _languageRepository;
            return repository.Get(id);
        }

        /// <summary>
        /// Gets a <see cref="Language"/> by its culture code
        /// </summary>
        /// <param name="culture">Culture Code</param>
        /// <returns><see cref="Language"/></returns>
        public ILanguage GetLanguageByCultureCode(string culture)
        {
            var repository = _languageRepository;

            var query = Query<ILanguage>.Builder.Where(x => x.CultureName == culture);
            var items = repository.GetByQuery(query);

            return items.FirstOrDefault();
        }

        /// <summary>
        /// Gets all available languages
        /// </summary>
        /// <returns>An enumerable list of <see cref="ILanguage"/> objects</returns>
        public IEnumerable<ILanguage> GetAllLanguages()
        {
            var repository = _languageRepository;
            var languages = repository.GetAll();
            return languages;
        }

        /// <summary>
        /// Saves a <see cref="ILanguage"/> object
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to save</param>
        public void Save(ILanguage language)
        {
            var repository = _languageRepository;

            repository.AddOrUpdate(language);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a <see cref="ILanguage"/> by removing it (but not its usages) from the db
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to delete</param>
        public void Delete(ILanguage language)
        {
            var repository = _languageRepository;

            //NOTE: There isn't any constraints in the db, so possible references aren't deleted
            repository.Delete(language);
            _unitOfWork.Commit();
        }
    }
}