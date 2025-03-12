using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal class PublicAccessService : RepositoryService, IPublicAccessService
{
    private readonly IPublicAccessRepository _publicAccessRepository;
    private readonly IEntityService _entityService;
    private readonly IContentService _contentService;
    private readonly IIdKeyMap _idKeyMap;

    public PublicAccessService(
        ICoreScopeProvider provider,
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
    public IEnumerable<PublicAccessEntry> GetAll()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _publicAccessRepository.GetMany();
        }
    }

    /// <summary>
    ///     Gets the entry defined for the content item's path
    /// </summary>
    /// <param name="content"></param>
    /// <returns>Returns null if no entry is found</returns>
    public PublicAccessEntry? GetEntryForContent(IContent content) =>
        GetEntryForContent(content.Path.EnsureEndsWith("," + content.Id));

    /// <summary>
    ///     Gets the entry defined for the content item based on a content path
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns>Returns null if no entry is found</returns>
    /// <remarks>
    ///     NOTE: This method get's called *very* often! This will return the results from cache
    /// </remarks>
    public PublicAccessEntry? GetEntryForContent(string contentPath)
    {
        // Get all ids in the path for the content item and ensure they all
        // parse to ints that are not -1.
        var ids = contentPath.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : -1)
            .Where(x => x != -1)
            .ToList();

        // start with the deepest id
        ids.Reverse();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            // This will retrieve from cache!
            var entries = _publicAccessRepository.GetMany().ToList();
            foreach (var id in CollectionsMarshal.AsSpan(ids))
            {
                PublicAccessEntry? found = entries.Find(x => x.ProtectedNodeId == id);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Returns true if the content has an entry for it's path
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public Attempt<PublicAccessEntry?> IsProtected(IContent content)
    {
        PublicAccessEntry? result = GetEntryForContent(content);
        return Attempt.If(result != null, result);
    }

    /// <summary>
    ///     Returns true if the content has an entry based on a content path
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns></returns>
    public Attempt<PublicAccessEntry?> IsProtected(string contentPath)
    {
        PublicAccessEntry? result = GetEntryForContent(contentPath);
        return Attempt.If(result != null, result);
    }

    /// <summary>
    ///     Adds a rule
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ruleType"></param>
    /// <param name="ruleValue"></param>
    /// <returns></returns>
    public Attempt<OperationResult<OperationResultType, PublicAccessEntry>?> AddRule(IContent content, string ruleType, string ruleValue)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();
        PublicAccessEntry? entry;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            entry = _publicAccessRepository.GetMany().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
            if (entry == null)
            {
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
                // If they are both the same already then there's nothing to update, exit
                return OperationResult.Attempt.Succeed(evtMsgs, entry);
            }

            var savingNotifiation = new PublicAccessEntrySavingNotification(entry, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotifiation))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(evtMsgs, entry);
            }

            _publicAccessRepository.Save(entry);

            scope.Complete();

            scope.Notifications.Publish(
                new PublicAccessEntrySavedNotification(entry, evtMsgs).WithStateFrom(savingNotifiation));
        }

        return OperationResult.Attempt.Succeed(evtMsgs, entry);
    }

    /// <summary>
    ///     Removes a rule
    /// </summary>
    /// <param name="content"></param>
    /// <param name="ruleType"></param>
    /// <param name="ruleValue"></param>
    public Attempt<OperationResult?> RemoveRule(IContent content, string ruleType, string ruleValue)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();
        PublicAccessEntry? entry;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            entry = _publicAccessRepository.GetMany().FirstOrDefault(x => x.ProtectedNodeId == content.Id);
            if (entry == null)
            {
                return Attempt<OperationResult?>.Fail(); // causes rollback // causes rollback
            }

            PublicAccessRule? existingRule =
                entry.Rules.FirstOrDefault(x => x.RuleType == ruleType && x.RuleValue == ruleValue);
            if (existingRule == null)
            {
                return Attempt<OperationResult?>.Fail(); // causes rollback // causes rollback
            }

            entry.RemoveRule(existingRule);

            var savingNotifiation = new PublicAccessEntrySavingNotification(entry, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotifiation))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(evtMsgs);
            }

            _publicAccessRepository.Save(entry);
            scope.Complete();

            scope.Notifications.Publish(
                new PublicAccessEntrySavedNotification(entry, evtMsgs).WithStateFrom(savingNotifiation));
        }

        return OperationResult.Attempt.Succeed(evtMsgs);
    }

    /// <summary>
    ///     Saves the entry
    /// </summary>
    /// <param name="entry"></param>
    public Attempt<OperationResult?> Save(PublicAccessEntry entry)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotifiation = new PublicAccessEntrySavingNotification(entry, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotifiation))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(evtMsgs);
            }

            _publicAccessRepository.Save(entry);
            scope.Complete();

            scope.Notifications.Publish(
                new PublicAccessEntrySavedNotification(entry, evtMsgs).WithStateFrom(savingNotifiation));
        }

        return OperationResult.Attempt.Succeed(evtMsgs);
    }

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

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> attempt = await SaveAsync(publicAccessEntry);
        return attempt.Success ? Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, attempt.Result!)
                : Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(attempt.Status, null);
    }

    private async Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> SaveAsync(PublicAccessEntry entry)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new PublicAccessEntrySavingNotification(entry, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(savingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.CancelledByNotification, null);
            }

            _publicAccessRepository.Save(entry);
            scope.Complete();

            scope.Notifications.Publish(
                new PublicAccessEntrySavedNotification(entry, eventMessages).WithStateFrom(savingNotification));
        }

        return Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, entry);
    }

    private Attempt<PublicAccessNodesValidationResult, PublicAccessOperationStatus> ValidatePublicAccessEntrySlim(PublicAccessEntrySlim entry)
    {
        var result = new PublicAccessNodesValidationResult();

        if (entry.MemberUserNames.Any() is false && entry.MemberGroupNames.Any() is false)
        {
            return Attempt.FailWithStatus(PublicAccessOperationStatus.NoAllowedEntities, result);
        }

        if(entry.MemberUserNames.Any() && entry.MemberGroupNames.Any())
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

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> attempt = await SaveAsync(mappedEntry);

        return attempt.Success
            ? Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, mappedEntry)
            : Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.CancelledByNotification, null);
    }

    /// <summary>
    ///     Deletes the entry and all associated rules
    /// </summary>
    /// <param name="entry"></param>
    public Attempt<OperationResult?> Delete(PublicAccessEntry entry)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var deletingNotification = new PublicAccessEntryDeletingNotification(entry, evtMsgs);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(evtMsgs);
            }

            _publicAccessRepository.Delete(entry);
            scope.Complete();

            scope.Notifications.Publish(
                new PublicAccessEntryDeletedNotification(entry, evtMsgs).WithStateFrom(deletingNotification));
        }

        return OperationResult.Attempt.Succeed(evtMsgs);
    }

    public Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> GetEntryByContentKeyAsync(Guid key)
    {
        IEntitySlim? entity = _entityService.Get(key, UmbracoObjectTypes.Document);
        if (entity is null)
        {
            return Task.FromResult(Attempt.FailWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.ContentNotFound, null));
        }

        PublicAccessEntry? entry = GetEntryForContent(entity.Path.EnsureEndsWith("," + entity.Id));

        if (entry is null)
        {
            return Task.FromResult(Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.EntryNotFound, null));
        }

        return Task.FromResult(Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.Success, entry));
    }

    public async Task<Attempt<PublicAccessEntry?, PublicAccessOperationStatus>> GetEntryByContentKeyWithoutAncestorsAsync(Guid key)
    {
        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> result = await GetEntryByContentKeyAsync(key);
        if (result.Success is false || result.Result is null)
        {
            return result;
        }

        Attempt<Guid> idToKeyAttempt = _idKeyMap.GetKeyForId(result.Result.ProtectedNodeId, UmbracoObjectTypes.Document);
        if (idToKeyAttempt.Success is false || idToKeyAttempt.Result != key)
        {
            return Attempt.SucceedWithStatus<PublicAccessEntry?, PublicAccessOperationStatus>(PublicAccessOperationStatus.EntryNotFound, null);
        }

        return result;
    }

    public async Task<Attempt<PublicAccessOperationStatus>> DeleteAsync(Guid key)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
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
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return Attempt.Fail(PublicAccessOperationStatus.CancelledByNotification);
            }

            _publicAccessRepository.Delete(attempt.Result!);

            scope.Complete();

            scope.Notifications.Publish(
                new PublicAccessEntryDeletedNotification(attempt.Result!, evtMsgs).WithStateFrom(deletingNotification));
        }

        return Attempt.Succeed(PublicAccessOperationStatus.Success);
    }

    private IEnumerable<PublicAccessRule> CreateAccessRuleList(string[] ruleValues, string ruleType) =>
        ruleValues.Select(ruleValue => new PublicAccessRule
        {
            RuleValue = ruleValue,
            RuleType = ruleType,
        });

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
