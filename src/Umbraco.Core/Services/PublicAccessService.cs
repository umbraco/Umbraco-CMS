using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IPublicAccessService" /> providing operations for managing public access entries and rules.
/// </summary>
internal sealed class PublicAccessService : AsyncRepositoryService, IPublicAccessService
{
    private readonly IPublicAccessRepository _publicAccessRepository;
    private readonly IEntityService _entityService;
    private readonly IContentService _contentService;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="publicAccessRepository">The repository for public access entry operations.</param>
    /// <param name="entityService">The entity service for entity-related operations.</param>
    /// <param name="contentService">The content service for content-related operations.</param>
    /// <param name="idKeyMap">The ID-key map for converting between IDs and keys.</param>
    public PublicAccessService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IPublicAccessRepository publicAccessRepository,
        IEntityService entityService,
        IContentService contentService,
        IIdKeyMap idKeyMap)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _publicAccessRepository = publicAccessRepository;
        _entityService = entityService;
        _contentService = contentService;
        _idKeyMap = idKeyMap;
    }

    /// <summary>
    ///     Gets all defined entries and associated rules
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<PublicAccessEntry>> GetAllAsync()
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<PublicAccessEntry> entries = await _publicAccessRepository.GetAllAsync(CancellationToken.None);
        scope.Complete();

        return entries;
    }

    /// <summary>
    ///     Gets the entry defined for the content item's path
    /// </summary>
    /// <param name="content"></param>
    /// <returns>Returns null if no entry is found</returns>
    public async Task<PublicAccessEntry?> GetEntryForContentAsync(IContent content) =>
        await GetEntryForContentAsync(content.Path.EnsureEndsWith("," + content.Id));

    /// <summary>
    ///     Gets the entry defined for the content item based on a content path
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns>Returns null if no entry is found</returns>
    /// <remarks>
    ///     NOTE: This method get's called *very* often! This will return the results from cache
    /// </remarks>
    public async Task<PublicAccessEntry?> GetEntryForContentAsync(string contentPath)
    {
        // Get all ids in the path for the content item and ensure they all
        // parse to ints that are not -1.
        // Start with the deepest id.
        IEnumerable<int> ids = contentPath.GetIdsFromPathReversed().Where(x => x != -1);

        using ICoreScope scope = ScopeProvider.CreateScope();
        // This will retrieve from cache!
        var entries = (await _publicAccessRepository.GetAllAsync(CancellationToken.None)).ToList();
        foreach (var id in ids)
        {
            PublicAccessEntry? found = entries.Find(x => x.ProtectedNodeId == id);
            if (found != null)
            {
                scope.Complete();
                return found;
            }
        }

        scope.Complete();
        return null;
    }

    /// <summary>
    ///     Returns true if the content has an entry for it's path
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<Attempt<PublicAccessEntry?>> IsProtectedAsync(IContent content)
    {
        PublicAccessEntry? result = await GetEntryForContentAsync(content);
        return Attempt.If(result != null, result);
    }

    /// <summary>
    ///     Returns true if the content has an entry based on a content path
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns></returns>
    public async Task<Attempt<PublicAccessEntry?>> IsProtectedAsync(string contentPath)
    {
        PublicAccessEntry? result = await GetEntryForContentAsync(contentPath);
        return Attempt.If(result != null, result);
    }

    /// <summary>
    ///     Adds a rule
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ruleType"></param>
    /// <param name="ruleValue"></param>
    /// <returns></returns>
    public async Task<Attempt<OperationResult<OperationResultType, PublicAccessEntry>?>> AddRuleAsync(IContent content, string ruleType, string ruleValue)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();
        PublicAccessEntry? entry;

        using ICoreScope scope = ScopeProvider.CreateScope();
        entry = (await _publicAccessRepository.GetAllAsync(CancellationToken.None)).FirstOrDefault(x => x.ProtectedNodeId == content.Id);

        if (entry == null)
        {
            scope.Complete();
            return OperationResult.Attempt.Cannot<PublicAccessEntry>(evtMsgs); // causes rollback
        }

        PublicAccessRule? existingRule =
            entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
        if (existingRule == null)
        {
            entry.AddRule(ruleValue, ruleType);
        }
        else
        {
            scope.Complete();
            // If they are both the same already then there's nothing to update, exit
            return OperationResult.Attempt.Succeed(evtMsgs, entry);
        }

        var savingNotification = new PublicAccessEntrySavingNotification(entry, evtMsgs);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return OperationResult.Attempt.Cancel(evtMsgs, entry);
        }

        await _publicAccessRepository.SaveAsync(entry, CancellationToken.None);

        scope.Complete();

        scope.Notifications.Publish(
            new PublicAccessEntrySavedNotification(entry, evtMsgs).WithStateFrom(savingNotification));

        return OperationResult.Attempt.Succeed(evtMsgs, entry);
    }

    /// <summary>
    ///     Removes a rule
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ruleType"></param>
    /// <param name="ruleValue"></param>
    public async Task<Attempt<OperationResult?>> RemoveRuleAsync(IContent content, string ruleType, string ruleValue)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateScope();

        PublicAccessEntry? entry = (await _publicAccessRepository.GetAllAsync(CancellationToken.None))
            .FirstOrDefault(x => x.ProtectedNodeId == content.Id);
        if (entry == null)
        {
            return Attempt<OperationResult?>.Fail(); // causes rollback
        }

        PublicAccessRule? existingRule =
            entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
        if (existingRule == null)
        {
            return Attempt<OperationResult?>.Fail(); // causes rollback
        }

        entry.RemoveRule(existingRule);

        var savingNotifiation = new PublicAccessEntrySavingNotification(entry, evtMsgs);
        if (await scope.Notifications.PublishCancelableAsync(savingNotifiation))
        {
            scope.Complete();
            return OperationResult.Attempt.Cancel(evtMsgs);
        }

        await _publicAccessRepository.SaveAsync(entry, CancellationToken.None);
        scope.Complete();

        scope.Notifications.Publish(
            new PublicAccessEntrySavedNotification(entry, evtMsgs).WithStateFrom(savingNotifiation));

        return OperationResult.Attempt.Succeed(evtMsgs);
    }

    /// <summary>
    ///     Saves the entry
    /// </summary>
    /// <param name="entry"></param>
    public async Task<Attempt<OperationResult?>> SaveAsync(PublicAccessEntry entry)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateScope();

        var savingNotifiation = new PublicAccessEntrySavingNotification(entry, evtMsgs);
        if (await scope.Notifications.PublishCancelableAsync(savingNotifiation))
        {
            scope.Complete();
            return OperationResult.Attempt.Cancel(evtMsgs);
        }

        await _publicAccessRepository.SaveAsync(entry, CancellationToken.None);
        scope.Complete();

        scope.Notifications.Publish(
            new PublicAccessEntrySavedNotification(entry, evtMsgs).WithStateFrom(savingNotifiation));

        return OperationResult.Attempt.Succeed(evtMsgs);
    }

    /// <inheritdoc />
    public async Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> CreateAsync(PublicAccessEntrySlim entry)
    {
        Attempt<PublicAccessNodesValidationResult, PublicAccessOperationStatus> validationAttempt = ValidatePublicAccessEntrySlim(entry);
        if (validationAttempt.Success is false)
        {
            return Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(validationAttempt.Status, null);
        }

        IEnumerable<PublicAccessRule> publicAccessRules =
            entry.MemberUserNames.Any() ? // We only need to check either member usernames or member group names, not both, as we have a check at the top of this method
                CreateAccessRuleList(entry.MemberUserNames, Constants.Conventions.PublicAccess.MemberUsernameRuleType) :
                CreateAccessRuleList(entry.MemberGroupNames, Constants.Conventions.PublicAccess.MemberRoleRuleType);

        var publicAccessEntry = new PublicAccessEntry(validationAttempt.Result.ProtectedNode!, validationAttempt.Result.LoginNode!, validationAttempt.Result.ErrorNode!, publicAccessRules);

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> attempt = await SaveEntryAsync(publicAccessEntry);
        return attempt.Success ? Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, attempt.Result!)
                : Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(attempt.Status, null);
    }

    /// <summary>
    ///     Saves a public access entry asynchronously.
    /// </summary>
    /// <param name="entry">The <see cref="PublicAccessEntry" /> to save.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with the saved entry and operation status.</returns>
    private async Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> SaveEntryAsync(PublicAccessEntry entry)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateScope();

        var savingNotification = new PublicAccessEntrySavingNotification(entry, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.CancelledByNotification, null);
        }

        await _publicAccessRepository.SaveAsync(entry, CancellationToken.None);
        scope.Complete();

        scope.Notifications.Publish(
            new PublicAccessEntrySavedNotification(entry, eventMessages).WithStateFrom(savingNotification));

        return Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, entry);
    }

    /// <summary>
    ///     Validates a <see cref="PublicAccessEntrySlim" /> and resolves its referenced content nodes.
    /// </summary>
    /// <param name="entry">The entry to validate.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}" /> containing the validation result with resolved nodes.</returns>
    private Attempt<PublicAccessNodesValidationResult, PublicAccessOperationStatus> ValidatePublicAccessEntrySlim(PublicAccessEntrySlim entry)
    {
        var result = new PublicAccessNodesValidationResult();

        if (entry.MemberUserNames.Any() is false && entry.MemberGroupNames.Any() is false)
        {
            return Attempt.FailWithStatus(PublicAccessOperationStatus.NoAllowedEntities, result);
        }

        if (entry.MemberUserNames.Any() && entry.MemberGroupNames.Any())
        {
            return Attempt.FailWithStatus(PublicAccessOperationStatus.AmbiguousRule, result);
        }

        result.ProtectedNode = _contentService.GetById(entry.ContentId);

        if (result.ProtectedNode is null)
        {
            return Attempt.FailWithStatus(PublicAccessOperationStatus.ContentNotFound, result);
        }

        result.LoginNode = _contentService.GetById(entry.LoginPageId);

        if (result.LoginNode is null)
        {
            return Attempt.FailWithStatus(PublicAccessOperationStatus.LoginNodeNotFound, result);
        }

        result.ErrorNode = _contentService.GetById(entry.ErrorPageId);

        if (result.ErrorNode is null)
        {
            return Attempt.FailWithStatus(PublicAccessOperationStatus.ErrorNodeNotFound, result);
        }

        return Attempt.SucceedWithStatus(PublicAccessOperationStatus.Success, result);
    }

    /// <inheritdoc />
    public async Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> UpdateAsync(PublicAccessEntrySlim entry)
    {
        Attempt<PublicAccessNodesValidationResult, PublicAccessOperationStatus> validationAttempt = ValidatePublicAccessEntrySlim(entry);

        if (validationAttempt.Success is false)
        {
            return Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(validationAttempt.Status, null);
        }

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> currentPublicAccessEntryAttempt = await GetEntryByContentKeyAsync(entry.ContentId);

        if (currentPublicAccessEntryAttempt.Success is false)
        {
            return currentPublicAccessEntryAttempt;
        }

        PublicAccessEntry mappedEntry = MapToUpdatedEntry(entry, currentPublicAccessEntryAttempt.Result!);

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> attempt = await SaveEntryAsync(mappedEntry);

        return attempt.Success
            ? Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, mappedEntry)
            : Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.CancelledByNotification, null);
    }

    /// <summary>
    ///     Deletes the entry and all associated rules
    /// </summary>
    /// <param name="entry"></param>
    public async Task<Attempt<OperationResult?>> DeleteAsync(PublicAccessEntry entry)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateScope();

        var deletingNotification = new PublicAccessEntryDeletingNotification(entry, evtMsgs);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return OperationResult.Attempt.Cancel(evtMsgs);
        }

        await _publicAccessRepository.DeleteAsync(entry, CancellationToken.None);
        scope.Complete();

        scope.Notifications.Publish(
            new PublicAccessEntryDeletedNotification(entry, evtMsgs).WithStateFrom(deletingNotification));

        return OperationResult.Attempt.Succeed(evtMsgs);
    }

    /// <inheritdoc />
    public async Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> GetEntryByContentKeyAsync(Guid key)
    {
        IEntitySlim? entity = _entityService.Get(key, UmbracoObjectTypes.Document);
        if (entity is null)
        {
            return Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.ContentNotFound, null);
        }

        PublicAccessEntry? entry = await GetEntryForContentAsync(entity.Path.EnsureEndsWith("," + entity.Id));

        if (entry is null)
        {
            return Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.EntryNotFound, null);
        }

        return Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, entry);
    }

    /// <inheritdoc />
    public async Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> GetEntryByContentKeyWithoutAncestorsAsync(Guid key)
    {
        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> result = await GetEntryByContentKeyAsync(key);
        if (result.Success is false || result.Result is null)
        {
            return result;
        }

        Attempt<Guid> idToKeyAttempt = await _idKeyMap.GetKeyForIdAsync(result.Result.ProtectedNodeId, UmbracoObjectTypes.Document);
        if (idToKeyAttempt.Success is false || idToKeyAttempt.Result != key)
        {
            return Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.EntryNotFound, null);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Attempt<PublicAccessOperationStatus>> DeleteAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> attempt = await GetEntryByContentKeyAsync(key);

        if (attempt.Success is false)
        {
            return Attempt.Fail(attempt.Status);
        }

        if (attempt.Result is null)
        {
            return Attempt.Fail(PublicAccessOperationStatus.EntryNotFound);
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        var deletingNotification = new PublicAccessEntryDeletingNotification(attempt.Result!, evtMsgs);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return Attempt.Fail(PublicAccessOperationStatus.CancelledByNotification);
        }

        await _publicAccessRepository.DeleteAsync(attempt.Result!, CancellationToken.None);

        scope.Complete();

        scope.Notifications.Publish(
            new PublicAccessEntryDeletedNotification(attempt.Result!, evtMsgs).WithStateFrom(deletingNotification));

        return Attempt.Succeed(PublicAccessOperationStatus.Success);
    }

    /// <summary>
    ///     Creates a collection of public access rules from the specified rule values and type.
    /// </summary>
    /// <param name="ruleValues">The values for the rules.</param>
    /// <param name="ruleType">The type of the rules.</param>
    /// <returns>An enumerable collection of <see cref="PublicAccessRule" /> objects.</returns>
    private IEnumerable<PublicAccessRule> CreateAccessRuleList(string[] ruleValues, string ruleType) =>
        ruleValues.Select(ruleValue => new PublicAccessRule
        {
            RuleValue = ruleValue,
            RuleType = ruleType,
        });

    /// <summary>
    ///     Maps updates from a <see cref="PublicAccessEntrySlim" /> to an existing <see cref="PublicAccessEntry" />.
    /// </summary>
    /// <param name="updatesModel">The model containing the updates.</param>
    /// <param name="entryToUpdate">The existing entry to update.</param>
    /// <returns>The updated <see cref="PublicAccessEntry" />.</returns>
    private PublicAccessEntry MapToUpdatedEntry(PublicAccessEntrySlim updatesModel, PublicAccessEntry entryToUpdate)
    {
        entryToUpdate.LoginNodeId = _entityService.GetId(updatesModel.LoginPageId, UmbracoObjectTypes.Document).Result;
        entryToUpdate.NoAccessNodeId = _entityService.GetId(updatesModel.ErrorPageId, UmbracoObjectTypes.Document).Result;

        var isGroupBased = updatesModel.MemberGroupNames.Any();
        var candidateRuleValues = isGroupBased
            ? updatesModel.MemberGroupNames
            : updatesModel.MemberUserNames;
        var newRuleType = isGroupBased
            ? Constants.Conventions.PublicAccess.MemberRoleRuleType
            : Constants.Conventions.PublicAccess.MemberUsernameRuleType;

        PublicAccessRule[] currentRules = entryToUpdate.Rules.ToArray();
        IEnumerable<PublicAccessRule> obsoleteRules = currentRules.Where(rule =>
            rule.RuleType != newRuleType
            || candidateRuleValues?.Contains(rule.RuleValue) == false);

        IEnumerable<string>? newRuleValues = candidateRuleValues?.Where(group =>
            currentRules.Any(rule =>
                rule.RuleType == newRuleType
                && rule.RuleValue == group) == false);

        foreach (PublicAccessRule rule in obsoleteRules)
        {
            entryToUpdate.RemoveRule(rule);
        }

        if (newRuleValues is not null)
        {
            foreach (var ruleValue in newRuleValues)
            {
                entryToUpdate.AddRule(ruleValue, newRuleType);
            }
        }

        return entryToUpdate;
    }
}
