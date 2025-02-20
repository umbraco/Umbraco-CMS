using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class DomainService : RepositoryService, IDomainService
{
    private readonly IDomainRepository _domainRepository;
    private readonly ILanguageService _languageService;
    private readonly IContentService _contentService;

    [Obsolete("Please use the constructor that accepts ILanguageService and IContentService. Will be removed in V15.")]
    public DomainService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDomainRepository domainRepository)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            domainRepository,
            StaticServiceProvider.Instance.GetRequiredService<ILanguageService>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentService>())
    {
    }

    public DomainService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDomainRepository domainRepository,
        ILanguageService languageService,
        IContentService contentService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _domainRepository = domainRepository;
        _languageService = languageService;
        _contentService = contentService;
    }

    public bool Exists(string domainName)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.Exists(domainName);
        }
    }

    [Obsolete($"Please use {nameof(UpdateDomainsAsync)}. Will be removed in V15")]
    public Attempt<OperationResult?> Delete(IDomain domain)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        var result = DeleteAll(new[] { domain }, scope, eventMessages);
        scope.Complete();

        return result ? OperationResult.Attempt.Succeed(eventMessages) : OperationResult.Attempt.Cancel(eventMessages);
    }

    public IDomain? GetByName(string name)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.GetByName(name);
        }
    }

    public IDomain? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.Get(id);
        }
    }

    [Obsolete($"Please use {nameof(GetAllAsync)}. Will be removed in V15")]
    public IEnumerable<IDomain> GetAll(bool includeWildcards)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.GetAll(includeWildcards);
        }
    }

    [Obsolete($"Please use {nameof(GetAssignedDomainsAsync)}. Will be removed in V15")]
    public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.GetAssignedDomains(contentId, includeWildcards);
        }
    }

    [Obsolete($"Please use {nameof(UpdateDomainsAsync)}. Will be removed in V15")]
    public Attempt<OperationResult?> Save(IDomain domainEntity)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        var result = SaveAll(new[] { domainEntity }, scope, eventMessages);
        scope.Complete();

        return result ? OperationResult.Attempt.Succeed(eventMessages) : OperationResult.Attempt.Cancel(eventMessages);
    }

    [Obsolete($"Please use {nameof(UpdateDomainsAsync)}. Will be removed in V15")]
    public Attempt<OperationResult?> Sort(IEnumerable<IDomain> items)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        IDomain[] domains = items.ToArray();
        if (domains.Length == 0)
        {
            return OperationResult.Attempt.NoOperation(eventMessages);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new DomainSavingNotification(domains, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(eventMessages);
            }

            scope.WriteLock(Constants.Locks.Domains);

            int sortOrder = 0;
            foreach (IDomain domain in domains)
            {
                // If the current sort order equals that of the domain we don't need to update it, so just increment the sort order and continue
                if (domain.SortOrder == sortOrder)
                {
                    sortOrder++;
                    continue;
                }

                domain.SortOrder = sortOrder++;
                _domainRepository.Save(domain);
            }

            scope.Complete();
            scope.Notifications.Publish(new DomainSavedNotification(domains, eventMessages).WithStateFrom(savingNotification));
        }

        return OperationResult.Attempt.Succeed(eventMessages);
    }

    /// <inheritdoc />
    public Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(Guid contentKey, bool includeWildcards)
    {
        IContent? content = _contentService.GetById(contentKey);
        if (content == null)
        {
            return Task.FromResult(Enumerable.Empty<IDomain>());
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_domainRepository.GetAssignedDomains(content.Id, includeWildcards));
    }

    /// <inheritdoc />
    public Task<IEnumerable<IDomain>> GetAllAsync(bool includeWildcards)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_domainRepository.GetAll(includeWildcards));
    }

    /// <inheritdoc />
    public async Task<Attempt<DomainUpdateResult, DomainOperationStatus>> UpdateDomainsAsync(Guid contentKey, DomainsUpdateModel updateModel)
    {
        IContent? content = _contentService.GetById(contentKey);
        if (content == null)
        {
            return Attempt.FailWithStatus(DomainOperationStatus.ContentNotFound, new DomainUpdateResult());
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IEnumerable<ILanguage> allLanguages = await _languageService.GetAllAsync();
        var languageIdByIsoCode = allLanguages.ToDictionary(l => l.IsoCode, l => l.Id);

        // validate language ISO codes
        if (HasInvalidIsoCode(updateModel, languageIdByIsoCode.Keys))
        {
            return Attempt.FailWithStatus(DomainOperationStatus.LanguageNotFound, new DomainUpdateResult());
        }

        // ensure all domain names in the update model are lowercased
        foreach (DomainModel domainModel in updateModel.Domains)
        {
            domainModel.DomainName = domainModel.DomainName.ToLowerInvariant();

            if(Uri.IsWellFormedUriString(domainModel.DomainName, UriKind.RelativeOrAbsolute) is false)
            {
                return Attempt.FailWithStatus(DomainOperationStatus.InvalidDomainName, new DomainUpdateResult());
            }
        }

        // make sure we're not attempting to assign duplicate domains
        if (updateModel.Domains.GroupBy(domain => domain.DomainName).Any(group => group.Count() > 1))
        {
            return Attempt.FailWithStatus(DomainOperationStatus.DuplicateDomainName, new DomainUpdateResult());
        }

        // grab all current domain assignments
        IDomain[] allDomains = (await GetAllAsync(true)).ToArray();

        // validate the domain names in the update model
        IDomain[] conflicts = GetDomainNameConflicts(content.Id, updateModel, allDomains);
        if (conflicts.Any())
        {
            return Attempt.FailWithStatus(
                DomainOperationStatus.ConflictingDomainName,
                new DomainUpdateResult { ConflictingDomains = conflicts });
        }

        // find the domains currently assigned to the content item
        IDomain[] currentlyAssignedDomains = allDomains.Where(domain => domain.RootContentId == content.Id).ToArray();

        // calculate the new domain assignments
        IDomain[] newAssignedDomains = CalculateNewAssignedDomains(content.Id, updateModel, currentlyAssignedDomains, languageIdByIsoCode);

        EventMessages eventMessages = EventMessagesFactory.Get();

        scope.WriteLock(Constants.Locks.Domains);

        // delete any obsolete domain assignments
        if (DeleteAll(currentlyAssignedDomains.Except(newAssignedDomains).ToArray(), scope, eventMessages) == false)
        {
            scope.Complete();

            // this is the only error scenario in DeleteAll
            return Attempt.FailWithStatus(DomainOperationStatus.CancelledByNotification, new DomainUpdateResult());
        }

        // update all domain assignments (also current ones, in case sort order or ISO code has changed)
        var result = SaveAll(newAssignedDomains, scope, eventMessages);
        scope.Complete();

        return result
            ? Attempt.SucceedWithStatus(
                DomainOperationStatus.Success,
                new DomainUpdateResult
                {
                    Domains = newAssignedDomains.AsEnumerable()
                })
            : Attempt.FailWithStatus(DomainOperationStatus.CancelledByNotification, new DomainUpdateResult());
    }

    /// <summary>
    /// Tests if any of the ISO codes in the update model are invalid
    /// </summary>
    private bool HasInvalidIsoCode(DomainsUpdateModel updateModel, IEnumerable<string> allIsoCodes)
        => new[] { updateModel.DefaultIsoCode }
            .Union(updateModel.Domains.Select(domainModel => domainModel.IsoCode))
            .WhereNotNull()
            .Except(allIsoCodes)
            .Any();

    /// <summary>
    /// Returns any current domain assignments in conflict with the updateModel domain names
    /// </summary>
    private IDomain[] GetDomainNameConflicts(int contentId, DomainsUpdateModel updateModel, IEnumerable<IDomain> allDomains)
    {
        IDomain[] domainsAssignedToOtherContent = allDomains
            .Where(domain => domain.IsWildcard == false && domain.RootContentId != contentId)
            .ToArray();

        var updateModelDomainNames = updateModel.Domains.Select(domain => domain.DomainName).ToArray();

        return domainsAssignedToOtherContent
            .Where(domain => updateModelDomainNames.InvariantContains(domain.DomainName))
            .ToArray();
    }

    /// <summary>
    /// Calculates the new domain assignment incl. wildcard domains
    /// </summary>
    private IDomain[] CalculateNewAssignedDomains(int contentId, DomainsUpdateModel updateModel, IDomain[] currentlyAssignedDomains, IDictionary<string, int> languageIdByIsoCode)
    {
        // calculate the assigned domains as they should be after updating (including wildcard domains)
        var newAssignedDomains = new List<IDomain>();
        if (updateModel.DefaultIsoCode.IsNullOrWhiteSpace() == false)
        {
            IDomain defaultDomain = currentlyAssignedDomains.FirstOrDefault(domain => domain.IsWildcard && domain.LanguageIsoCode == updateModel.DefaultIsoCode)
                                    ?? new UmbracoDomain($"*{contentId}")
                                    {
                                        LanguageId = languageIdByIsoCode[updateModel.DefaultIsoCode],
                                        RootContentId = contentId
                                    };

            // wildcard domains should have sort order -1 (lowest possible sort order)
            defaultDomain.SortOrder = -1;

            newAssignedDomains.Add(defaultDomain);
        }

        var sortOrder = 0;
        foreach (DomainModel domainModel in updateModel.Domains)
        {
            IDomain? assignedDomain = currentlyAssignedDomains.FirstOrDefault(domain => domainModel.DomainName.InvariantEquals(domain.DomainName));

            // If we do not have an assigned domain, or the domain-language has been changed, create new domain.
            if (assignedDomain is null || assignedDomain.LanguageId != languageIdByIsoCode[domainModel.IsoCode])
            {
                assignedDomain = new UmbracoDomain(domainModel.DomainName)
                {
                    LanguageId = languageIdByIsoCode[domainModel.IsoCode],
                    RootContentId = contentId
                };
            }

            assignedDomain.SortOrder = sortOrder++;
            newAssignedDomains.Add(assignedDomain);
        }

        return newAssignedDomains.ToArray();
    }

    /// <summary>
    /// Handles deletion of one or more domains incl. notifications
    /// </summary>
    private bool DeleteAll(IDomain[] domainsToDelete, ICoreScope scope, EventMessages eventMessages)
    {
        if (domainsToDelete.Any() == false)
        {
            return true;
        }

        var deletingNotification = new DomainDeletingNotification(domainsToDelete, eventMessages);
        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            return false;
        }

        foreach (IDomain domainToDelete in domainsToDelete)
        {
            _domainRepository.Delete(domainToDelete);
        }

        scope.Notifications.Publish(new DomainDeletedNotification(domainsToDelete, eventMessages).WithStateFrom(deletingNotification));
        return true;
    }

    /// <summary>
    /// Handles saving of one or more domains incl. notifications
    /// </summary>
    private bool SaveAll(IDomain[] domainsToSave, ICoreScope scope, EventMessages eventMessages)
    {
        if (domainsToSave.Any() == false)
        {
            return true;
        }

        var savingNotification = new DomainSavingNotification(domainsToSave, eventMessages);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            return false;
        }

        foreach (IDomain assignedDomain in domainsToSave)
        {
            _domainRepository.Save(assignedDomain);
        }

        scope.Notifications.Publish(new DomainSavedNotification(domainsToSave, eventMessages).WithStateFrom(savingNotification));
        return true;
    }
}
