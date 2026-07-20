using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Service for managing languages in Umbraco, including CRUD operations and validation.
/// </summary>
internal sealed class LanguageService : RepositoryService, ILanguageService
{
    private readonly ILanguageRepository _languageRepository;
    private readonly IAuditService _auditService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IIsoCodeValidator _isoCodeValidator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="isoCodeValidator">The ISO code validator.</param>
    public LanguageService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        ILanguageRepository languageRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver,
        IIsoCodeValidator isoCodeValidator)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _languageRepository = languageRepository;
        _auditService = auditService;
        _userIdKeyResolver = userIdKeyResolver;
        _isoCodeValidator = isoCodeValidator;
    }

    /// <inheritdoc />
    public Task<ILanguage?> GetAsync(string isoCode)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_languageRepository.GetByIsoCode(isoCode));
        }
    }

    /// <inheritdoc />
    public Task<ILanguage?> GetDefaultLanguageAsync()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_languageRepository.GetByIsoCode(_languageRepository.GetDefaultIsoCode()));
        }
    }

    /// <inheritdoc />
    public Task<string> GetDefaultIsoCodeAsync()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_languageRepository.GetDefaultIsoCode());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<ILanguage>> GetAllAsync()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_languageRepository.GetMany());
        }
    }

    /// <inheritdoc />
    public Task<string[]> GetIsoCodesByIdsAsync(ICollection<int> ids)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete:true);

        return Task.FromResult(_languageRepository.GetIsoCodesByIds(ids, throwOnNotFound: true));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ILanguage>> GetMultipleAsync(IEnumerable<string> isoCodes) => (await GetAllAsync()).Where(x => isoCodes.Contains(x.IsoCode));

    /// <inheritdoc />
    public async Task<Attempt<ILanguage, LanguageOperationStatus>> UpdateAsync(ILanguage language, Guid userKey)
    {
        // Create and update dates aren't tracked for languages. They exist on ILanguage due to the
        // inheritance from IEntity, but we don't store them.
        // However we have logic in ServerEventSender that will provide SignalR events for created and update operations,
        // where these dates are used to distinguish between the two (whether or not the entity has an identity cannot
        // be used here, as these events fire after persistence when the identity is known for both creates and updates).
        // So ensure we set something that can be distinguished here.
        if (language.CreateDate == default)
        {
            // Set such that it's prior to the update date, but not the default date which will be considered
            // uninitialized and get reset to the current date at the repository.
            language.CreateDate = DateTime.MinValue.AddHours(1);
        }

        if (language.UpdateDate == default)
        {
            language.UpdateDate = DateTime.UtcNow;
        }

        return await SaveAsync(
            language,
            () =>
            {
                ILanguage? currentLanguage = _languageRepository.Get(language.Id);
                if (currentLanguage == null)
                {
                    return LanguageOperationStatus.NotFound;
                }

                // ensure we don't un-default the default language
                if (currentLanguage.IsDefault && !language.IsDefault)
                {
                    return LanguageOperationStatus.MissingDefault;
                }

                return LanguageOperationStatus.Success;
            },
            AuditType.Save,
            "Update Language",
            userKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<ILanguage, LanguageOperationStatus>> CreateAsync(ILanguage language, Guid userKey)
    {
        if (language.Id != 0)
        {
            return Attempt.FailWithStatus(LanguageOperationStatus.InvalidId, language);
        }

        return await SaveAsync(
            language,
            () =>
            {
                // ensure no duplicates by ISO code
                if (_languageRepository.GetByIsoCode(language.IsoCode) != null)
                {
                    return LanguageOperationStatus.DuplicateIsoCode;
                }

                return LanguageOperationStatus.Success;
            },
            AuditType.New,
            "Create Language",
            userKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<ILanguage?, LanguageOperationStatus>> DeleteAsync(string isoCode, Guid userKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            // write-lock languages to guard against race conds when dealing with default language
            scope.WriteLock(Constants.Locks.Languages);

            ILanguage? language = _languageRepository.GetByIsoCode(isoCode);
            if (language == null)
            {
                return Attempt.FailWithStatus<ILanguage?, LanguageOperationStatus>(LanguageOperationStatus.NotFound, null);
            }

            if (language.IsDefault)
            {
                return Attempt.FailWithStatus<ILanguage?, LanguageOperationStatus>(LanguageOperationStatus.MissingDefault, language);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingLanguageNotification = new LanguageDeletingNotification(language, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(deletingLanguageNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus<ILanguage?, LanguageOperationStatus>(LanguageOperationStatus.CancelledByNotification, language);
            }

            // NOTE: Other than the fall-back language, there aren't any other constraints in the db, so possible references aren't deleted
            _languageRepository.Delete(language);

            scope.Notifications.Publish(
                new LanguageDeletedNotification(language, eventMessages).WithStateFrom(deletingLanguageNotification));

            await AuditAsync(AuditType.Delete, "Delete Language", userKey, language.Id, UmbracoObjectTypes.Language.GetName());
            scope.Complete();
            return Attempt.SucceedWithStatus<ILanguage?, LanguageOperationStatus>(LanguageOperationStatus.Success, language);
        }
    }

    private async Task<Attempt<ILanguage, LanguageOperationStatus>> SaveAsync(
        ILanguage language,
        Func<LanguageOperationStatus> operationValidation,
        AuditType auditType,
        string auditMessage,
        Guid userKey)
    {
        if (_isoCodeValidator.IsValid(language.IsoCode) == false)
        {
            return Attempt.FailWithStatus(LanguageOperationStatus.InvalidIsoCode, language);
        }

        if (language.FallbackIsoCode is not null && _isoCodeValidator.IsValid(language.FallbackIsoCode) == false)
        {
            return Attempt.FailWithStatus(LanguageOperationStatus.InvalidFallbackIsoCode, language);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            // write-lock languages to guard against race conds when dealing with default language
            scope.WriteLock(Constants.Locks.Languages);

            LanguageOperationStatus status = operationValidation();
            if (status != LanguageOperationStatus.Success)
            {
                return Attempt.FailWithStatus(status, language);
            }

            // validate the fallback language - within write-lock (important!)
            if (HasInvalidFallbackLanguage(language))
            {
                return Attempt.FailWithStatus(LanguageOperationStatus.InvalidFallback, language);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new LanguageSavingNotification(language, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(savingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(LanguageOperationStatus.CancelledByNotification, language);
            }

            _languageRepository.Save(language);
            scope.Notifications.Publish(
                new LanguageSavedNotification(language, eventMessages).WithStateFrom(savingNotification));

            await AuditAsync(auditType, auditMessage, userKey, language.Id, UmbracoObjectTypes.Language.GetName());

            scope.Complete();
            return Attempt.SucceedWithStatus(LanguageOperationStatus.Success, language);
        }
    }

    private async Task AuditAsync(AuditType type, string message, Guid userKey, int objectId, string? entityType) =>
        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            entityType,
            message);

    private bool HasInvalidFallbackLanguage(ILanguage language)
    {
        // no fallback language = valid
        if (language.FallbackIsoCode == null)
        {
            return false;
        }

        // does the fallback language actually exist?
        var languagesByIsoCode = _languageRepository.GetMany().ToDictionary(x => x.IsoCode, x => x, StringComparer.OrdinalIgnoreCase);
        if (!languagesByIsoCode.ContainsKey(language.FallbackIsoCode))
        {
            return true;
        }

        if (CreatesCycle(language, languagesByIsoCode))
        {
            // explicitly logging this because it may not be obvious, specially with implicit cyclic fallbacks
            LoggerFactory
                .CreateLogger<LanguageService>()
                .Log(LogLevel.Error, $"Cannot use language {language.FallbackIsoCode} as fallback for language {language.IsoCode} as this would create a fallback cycle.");

            return true;
        }

        return false;
    }

    private bool CreatesCycle(ILanguage language, IDictionary<string, ILanguage> languagesByIsoCode)
    {
        // a new language is not referenced yet, so cannot be part of a cycle
        if (!language.HasIdentity)
        {
            return false;
        }

        var isoCode = language.FallbackIsoCode;

        // assuming languages does not already contains a cycle, this must end
        while (true)
        {
            if (isoCode == null)
            {
                return false; // no fallback means no cycle
            }

            if (isoCode.InvariantEquals(language.IsoCode))
            {
                return true; // back to language = cycle!
            }

            isoCode = languagesByIsoCode[isoCode].FallbackIsoCode; // else keep chaining
        }
    }
}
