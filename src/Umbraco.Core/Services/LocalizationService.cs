using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the Localization Service, which is an easy access to operations involving <see cref="Language" /> and
///     <see cref="DictionaryItem" />
/// </summary>
[Obsolete("Please use ILanguageService and IDictionaryItemService for localization. Will be removed in V15.")]
internal class LocalizationService : RepositoryService, ILocalizationService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IDictionaryRepository _dictionaryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ILanguageService _languageService;
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    [Obsolete("Please use constructor with language, dictionary and user services. Will be removed in V15")]
    public LocalizationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDictionaryRepository dictionaryRepository,
        IAuditRepository auditRepository,
        ILanguageRepository languageRepository)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            dictionaryRepository,
            auditRepository,
            languageRepository,
            StaticServiceProvider.Instance.GetRequiredService<ILanguageService>(),
            StaticServiceProvider.Instance.GetRequiredService<IDictionaryItemService>(),
            StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>())
    {
    }

    [Obsolete("Please use ILanguageService and IDictionaryItemService for localization. Will be removed in V15.")]
    public LocalizationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDictionaryRepository dictionaryRepository,
        IAuditRepository auditRepository,
        ILanguageRepository languageRepository,
        ILanguageService languageService,
        IDictionaryItemService dictionaryItemService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _dictionaryRepository = dictionaryRepository;
        _auditRepository = auditRepository;
        _languageRepository = languageRepository;
        _languageService = languageService;
        _dictionaryItemService = dictionaryItemService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <summary>
    ///     Adds or updates a translation for a dictionary item and language
    /// </summary>
    /// <param name="item"></param>
    /// <param name="language"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This does not save the item, that needs to be done explicitly
    /// </remarks>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public void AddOrUpdateDictionaryValue(IDictionaryItem item, ILanguage? language, string value)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (language == null)
        {
            throw new ArgumentNullException(nameof(language));
        }

        item.AddOrUpdateDictionaryValue(language, value);
    }

    /// <summary>
    ///     Creates and saves a new dictionary item and assigns a value to all languages if defaultValue is specified.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="parentId"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IDictionaryItem CreateDictionaryItemWithIdentity(string key, Guid? parentId, string? defaultValue = null)
    {
        IEnumerable<IDictionaryTranslation> translations = defaultValue.IsNullOrWhiteSpace()
            ? Array.Empty<IDictionaryTranslation>()
            : GetAllLanguages()
                .Select(language => new DictionaryTranslation(language, defaultValue!))
                .ToArray();

        Attempt<IDictionaryItem, DictionaryItemOperationStatus> result = _dictionaryItemService
            .CreateAsync(new DictionaryItem(parentId, key) { Translations = translations }, Constants.Security.SuperUserKey)
            .GetAwaiter()
            .GetResult();
        // mimic old service behavior
        return result.Success || result.Status == DictionaryItemOperationStatus.CancelledByNotification
            ? result.Result
            : throw new ArgumentException($"Could not create a dictionary item with key: {key} under parent: {parentId}");
    }

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="int" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IDictionaryItem? GetDictionaryItemById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _dictionaryRepository.Get(id);
        }
    }

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="Guid" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="DictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IDictionaryItem? GetDictionaryItemById(Guid id)
        => _dictionaryItemService.GetAsync(id).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a collection <see cref="IDictionaryItem" /> by their <see cref="Guid" /> ids
    /// </summary>
    /// <param name="ids">Ids of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IEnumerable<IDictionaryItem> GetDictionaryItemsByIds(params Guid[] ids)
        => _dictionaryItemService.GetManyAsync(ids).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its key
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IDictionaryItem? GetDictionaryItemByKey(string key)
        => _dictionaryItemService.GetAsync(key).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a collection of <see cref="IDictionaryItem" /> by their keys
    /// </summary>
    /// <param name="keys">Keys of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IEnumerable<IDictionaryItem> GetDictionaryItemsByKeys(params string[] keys)
        => _dictionaryItemService.GetManyAsync(keys).GetAwaiter().GetResult();

    /// <summary>
    /// Gets a list of children for a <see cref="IDictionaryItem"/>
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId)
        => _dictionaryItemService.GetChildrenAsync(parentId).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a list of descendants for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent, null will return all dictionary items</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId)
        => _dictionaryItemService.GetDescendantsAsync(parentId).GetAwaiter().GetResult();

    /// <summary>
    /// Gets the root/top <see cref="IDictionaryItem"/> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public IEnumerable<IDictionaryItem> GetRootDictionaryItems()
        => _dictionaryItemService.GetAtRootAsync().GetAwaiter().GetResult();

    /// <summary>
    ///     Checks if a <see cref="IDictionaryItem" /> with given key exists
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>True if a <see cref="IDictionaryItem" /> exists, otherwise false</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public bool DictionaryItemExists(string key)
        => _dictionaryItemService.ExistsAsync(key).GetAwaiter().GetResult();

    /// <summary>
    ///     Saves a <see cref="IDictionaryItem" /> object
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to save</param>
    /// <param name="userId">Optional id of the user saving the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public void Save(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId)
    {
        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        if (dictionaryItem.Id > 0)
        {
            _dictionaryItemService.UpdateAsync(dictionaryItem, currentUserKey).GetAwaiter().GetResult();
        }
        else
        {
            _dictionaryItemService.CreateAsync(dictionaryItem, currentUserKey).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    ///     Deletes a <see cref="IDictionaryItem" /> object and its related translations
    ///     as well as its children.
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public void Delete(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId)
    {
        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        _dictionaryItemService.DeleteAsync(dictionaryItem.Key, currentUserKey).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Gets a <see cref="Language" /> by its id
    /// </summary>
    /// <param name="id">Id of the <see cref="Language" /></param>
    /// <returns>
    ///     <see cref="Language" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public ILanguage? GetLanguageById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _languageRepository.Get(id);
        }
    }

    /// <summary>
    ///     Gets a <see cref="Language" /> by its iso code
    /// </summary>
    /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
    /// <returns>
    ///     <see cref="Language" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public ILanguage? GetLanguageByIsoCode(string? isoCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(isoCode);
        return _languageService.GetAsync(isoCode).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public int? GetLanguageIdByIsoCode(string isoCode)
        => _languageService.GetAsync(isoCode).GetAwaiter().GetResult()?.Id;

    /// <inheritdoc />
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public string? GetLanguageIsoCodeById(int id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _languageRepository.GetIsoCodeById(id);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public string GetDefaultLanguageIsoCode()
        => _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

    /// <inheritdoc />
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public int? GetDefaultLanguageId()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _languageRepository.GetDefaultId();
        }
    }

    /// <summary>
    ///     Gets all available languages
    /// </summary>
    /// <returns>An enumerable list of <see cref="ILanguage" /> objects</returns>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public IEnumerable<ILanguage> GetAllLanguages()
        => _languageService.GetAllAsync().GetAwaiter().GetResult();

    /// <summary>
    ///     Saves a <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to save</param>
    /// <param name="userId">Optional id of the user saving the language</param>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public void Save(ILanguage language, int userId = Constants.Security.SuperUserId)
    {
        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        Attempt<ILanguage, LanguageOperationStatus> result = language.Id > 0
            ? _languageService.UpdateAsync(language, currentUserKey).GetAwaiter().GetResult()
            : _languageService.CreateAsync(language, currentUserKey).GetAwaiter().GetResult();

        // mimic old Save behavior
        if (result.Status == LanguageOperationStatus.InvalidFallback)
        {
            throw new InvalidOperationException($"Cannot save language {language.IsoCode} with fallback {language.FallbackIsoCode}.");
        }
    }

    /// <summary>
    ///     Deletes a <see cref="ILanguage" /> by removing it (but not its usages) from the db
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the language</param>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    public void Delete(ILanguage language, int userId = Constants.Security.SuperUserId)
    {
        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        _languageService.DeleteAsync(language.IsoCode, currentUserKey).GetAwaiter().GetResult();
    }

    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    public Dictionary<string, Guid> GetDictionaryItemKeyMap()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _dictionaryRepository.GetDictionaryItemKeyMap();
        }
    }
}
