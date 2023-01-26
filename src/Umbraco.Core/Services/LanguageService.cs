using System.Globalization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal class LanguageService : RepositoryService, ILanguageService
{
    private readonly ILanguageRepository _languageRepository;
    private readonly IAuditRepository _auditRepository;

    public LanguageService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        ILanguageRepository languageRepository,
        IAuditRepository auditRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _languageRepository = languageRepository;
        _auditRepository = auditRepository;
    }

    /// <inheritdoc />
    public async Task<ILanguage?> GetAsync(string isoCode)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_languageRepository.GetByIsoCode(isoCode));
        }
    }

    /// <inheritdoc />
    public async Task<string> GetDefaultIsoCodeAsync()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_languageRepository.GetDefaultIsoCode());
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ILanguage>> GetAllAsync()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_languageRepository.GetMany());
        }
    }

    /// <inheritdoc />
    public async Task<Attempt<ILanguage, LanguageOperationStatus>> UpdateAsync(ILanguage language, int userId = Constants.Security.SuperUserId)
        => await SaveAsync(
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
            userId);

    /// <inheritdoc />
    public async Task<Attempt<ILanguage, LanguageOperationStatus>> CreateAsync(ILanguage language, int userId = Constants.Security.SuperUserId)
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
            userId);
    }

    /// <inheritdoc />
    public async Task<Attempt<ILanguage?, LanguageOperationStatus>> DeleteAsync(string isoCode, int userId = Constants.Security.SuperUserId)
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

            Audit(AuditType.Delete, "Delete Language", userId, language.Id, UmbracoObjectTypes.Language.GetName());
            scope.Complete();
            return await Task.FromResult(Attempt.SucceedWithStatus<ILanguage?, LanguageOperationStatus>(LanguageOperationStatus.Success, language));
        }
    }

    private async Task<Attempt<ILanguage, LanguageOperationStatus>> SaveAsync(
        ILanguage language,
        Func<LanguageOperationStatus> operationValidation,
        AuditType auditType,
        string auditMessage,
        int userId)
    {
        if (HasValidIsoCode(language) == false)
        {
            return Attempt.FailWithStatus(LanguageOperationStatus.InvalidIsoCode, language);
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

            Audit(auditType, auditMessage, userId, language.Id, UmbracoObjectTypes.Language.GetName());

            scope.Complete();
            return await Task.FromResult(Attempt.SucceedWithStatus(LanguageOperationStatus.Success, language));
        }
    }

    private void Audit(AuditType type, string message, int userId, int objectId, string? entityType) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, message));

    private bool HasInvalidFallbackLanguage(ILanguage language)
    {
        // no fallback language = valid
        if (language.FallbackLanguageId.HasValue == false)
        {
            return false;
        }

        // does the fallback language actually exist?
        var languages = _languageRepository.GetMany().ToDictionary(x => x.Id, x => x);
        if (languages.ContainsKey(language.FallbackLanguageId.Value) == false)
        {
            return true;
        }

        // does the fallback language create a cycle?
        if (CreatesCycle(language, languages))
        {
            // explicitly logging this because it may not be obvious, specially with implicit cyclic fallbacks
            LoggerFactory
                .CreateLogger<LanguageService>()
                .Log(LogLevel.Error, $"Cannot use language {languages[language.FallbackLanguageId.Value].IsoCode} as fallback for language {language.IsoCode} as this would create a fallback cycle.");

            return true;
        }

        return false;
    }

    private bool CreatesCycle(ILanguage language, IDictionary<int, ILanguage> languages)
    {
        // a new language is not referenced yet, so cannot be part of a cycle
        if (!language.HasIdentity)
        {
            return false;
        }

        var id = language.FallbackLanguageId;

        // assuming languages does not already contains a cycle, this must end
        while (true)
        {
            if (!id.HasValue)
            {
                return false; // no fallback means no cycle
            }

            if (id.Value == language.Id)
            {
                return true; // back to language = cycle!
            }

            id = languages[id.Value].FallbackLanguageId; // else keep chaining
        }
    }

    private static bool HasValidIsoCode(ILanguage language)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(language.IsoCode);
            return culture.Name == language.IsoCode;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
