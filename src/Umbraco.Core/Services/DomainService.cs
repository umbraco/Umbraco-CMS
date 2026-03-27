using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing domains (hostnames and culture assignments for content).
/// </summary>
public class DomainService : AsyncRepositoryService, IDomainService
{
    private readonly IDomainRepository _domainRepository;
    private readonly ILanguageService _languageService;
    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainService"/> class.
    /// </summary>
    public DomainService(
        IScopeProvider provider,
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

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string domainName)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        var exists = await _domainRepository.ExistsAsync(domainName);
        scope.Complete();

        return exists;
    }

    /// <inheritdoc />
    public async Task<IDomain?> GetByNameAsync(string name)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IDomain? domain = await _domainRepository.GetByNameAsync(name);
        scope.Complete();

        return domain;
    }

    /// <inheritdoc />
    public async Task<IDomain?> GetByIdAsync(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IDomain> all = await _domainRepository.GetAllAsync(true);
        scope.Complete();

        return all.FirstOrDefault(x => x.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(Guid contentKey, bool includeWildcards)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IDomain> domains = await _domainRepository.GetAssignedDomainsAsync(contentKey, includeWildcards);

        return domains;
    }

    /// <inheritdoc />
    // TODO: Remove this method once any usages has been migrated to the key alternative method.
    public async Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(int contentId, bool includeWildcards)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IContent? content = _contentService.GetById(contentId);
        if (content == null)
        {
            return Enumerable.Empty<IDomain>();
        }

        IEnumerable<IDomain> domains = await _domainRepository.GetAssignedDomainsAsync(content.Key, includeWildcards);
        scope.Complete();

        return domains;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDomain>> GetAllAsync(bool includeWildcards)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IDomain> domains = await _domainRepository.GetAllAsync(includeWildcards);
        scope.Complete();

        return domains;
    }

    /// <inheritdoc />
    public async Task<Attempt<DomainUpdateResult, DomainOperationStatus>> UpdateDomainsAsync(Guid contentKey, DomainsUpdateModel updateModel)
    {
        IContent? content = _contentService.GetById(contentKey);
        if (content == null)
        {
            return Attempt.FailWithStatus(DomainOperationStatus.ContentNotFound, new DomainUpdateResult());
        }

        using ICoreScope scope = ScopeProvider.CreateScope();

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

            if (Uri.IsWellFormedUriString(domainModel.DomainName, UriKind.RelativeOrAbsolute) is false)
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
        if (await DeleteAllAsync(currentlyAssignedDomains.Except(newAssignedDomains).ToArray(), scope, eventMessages) == false)
        {
            scope.Complete();
            return Attempt.FailWithStatus(DomainOperationStatus.CancelledByNotification, new DomainUpdateResult());
        }

        // update all domain assignments (also current ones, in case sort order or ISO code has changed)
        var result = await SaveAllAsync(newAssignedDomains, scope, eventMessages);
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

    private bool HasInvalidIsoCode(DomainsUpdateModel updateModel, IEnumerable<string> allIsoCodes)
        => new[] { updateModel.DefaultIsoCode }
            .Union(updateModel.Domains.Select(domainModel => domainModel.IsoCode))
            .WhereNotNull()
            .Except(allIsoCodes)
            .Any();

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

    private IDomain[] CalculateNewAssignedDomains(int contentId, DomainsUpdateModel updateModel, IDomain[] currentlyAssignedDomains, IDictionary<string, int> languageIdByIsoCode)
    {
        var newAssignedDomains = new List<IDomain>();
        if (updateModel.DefaultIsoCode.IsNullOrWhiteSpace() == false)
        {
            IDomain defaultDomain = currentlyAssignedDomains.FirstOrDefault(domain => domain.IsWildcard && domain.LanguageIsoCode == updateModel.DefaultIsoCode)
                                    ?? new UmbracoDomain($"*{contentId}")
                                    {
                                        LanguageId = languageIdByIsoCode[updateModel.DefaultIsoCode],
                                        RootContentId = contentId,
                                    };

            defaultDomain.SortOrder = -1;
            newAssignedDomains.Add(defaultDomain);
        }

        var sortOrder = 0;
        foreach (DomainModel domainModel in updateModel.Domains)
        {
            IDomain? assignedDomain = currentlyAssignedDomains.FirstOrDefault(domain => domainModel.DomainName.InvariantEquals(domain.DomainName));

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

    private async Task<bool> DeleteAllAsync(IDomain[] domainsToDelete, ICoreScope scope, EventMessages eventMessages)
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
            await _domainRepository.DeleteAsync(domainToDelete, CancellationToken.None);
        }

        scope.Notifications.Publish(new DomainDeletedNotification(domainsToDelete, eventMessages).WithStateFrom(deletingNotification));
        return true;
    }

    private async Task<bool> SaveAllAsync(IDomain[] domainsToSave, ICoreScope scope, EventMessages eventMessages)
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
            await _domainRepository.SaveAsync(assignedDomain, CancellationToken.None);
        }

        scope.Notifications.Publish(new DomainSavedNotification(domainsToSave, eventMessages).WithStateFrom(savingNotification));
        return true;
    }
}
