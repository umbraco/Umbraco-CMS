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
[Obsolete("Please use ILanguageService and IDictionaryItemService for localization. Scheduled for removal in Umbraco 18.")]
internal class LocalizationService : RepositoryService, ILocalizationService
{
    private readonly IDictionaryRepository _dictionaryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ILanguageService _languageService;
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalizationService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="dictionaryRepository">The dictionary repository.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="dictionaryItemService">The dictionary item service.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    [Obsolete("Please use ILanguageService and IDictionaryItemService for localization. Scheduled for removal in Umbraco 18.")]
    public LocalizationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDictionaryRepository dictionaryRepository,
        ILanguageRepository languageRepository,
        ILanguageService languageService,
        IDictionaryItemService dictionaryItemService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _dictionaryRepository = dictionaryRepository;
        _languageRepository = languageRepository;
        _languageService = languageService;
        _dictionaryItemService = dictionaryItemService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="int" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
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
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    public IDictionaryItem? GetDictionaryItemById(Guid id)
        => _dictionaryItemService.GetAsync(id).GetAwaiter().GetResult();

    /// <inheritdoc />
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    public Guid? GetDefaultLanguageKey()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _languageRepository.GetDefaultKeyAsync().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its key
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    public IDictionaryItem? GetDictionaryItemByKey(string key)
        => _dictionaryItemService.GetAsync(key).GetAwaiter().GetResult();

    /// <summary>
    /// Gets a list of children for a <see cref="IDictionaryItem"/>
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId)
        => _dictionaryItemService.GetChildrenAsync(parentId).GetAwaiter().GetResult();

    /// <summary>
    ///     Saves a <see cref="IDictionaryItem" /> object
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to save</param>
    /// <param name="userId">Optional id of the user saving the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
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
    ///     Gets a <see cref="Language" /> by its id
    /// </summary>
    /// <param name="id">Id of the <see cref="Language" /></param>
    /// <returns>
    ///     <see cref="Language" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    public ILanguage? GetLanguageById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _languageRepository.GetAsync(id, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    ///     Gets a <see cref="Language" /> by its iso code
    /// </summary>
    /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
    /// <returns>
    ///     <see cref="Language" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    public ILanguage? GetLanguageByIsoCode(string? isoCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(isoCode);
        return _languageService.GetAsync(isoCode).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    public string GetDefaultLanguageIsoCode()
        => _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

    /// <summary>
    ///     Gets all available languages
    /// </summary>
    /// <returns>An enumerable list of <see cref="ILanguage" /> objects</returns>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<ILanguage> GetAllLanguages()
        => _languageService.GetAllAsync().GetAwaiter().GetResult();

    /// <summary>
    ///     Saves a <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to save</param>
    /// <param name="userId">Optional id of the user saving the language</param>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
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
}
