using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal class MemberGroupService : RepositoryService, IMemberGroupService
{
    private readonly IMemberGroupRepository _memberGroupRepository;

    public MemberGroupService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IMemberGroupRepository memberGroupRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _memberGroupRepository = memberGroupRepository;

    public IEnumerable<IMemberGroup> GetAll() => GetAllAsync().GetAwaiter().GetResult();

    public IEnumerable<IMemberGroup> GetByIds(IEnumerable<int> ids) => GetByIdsAsync(ids).GetAwaiter().GetResult();

    public IMemberGroup? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _memberGroupRepository.Get(id);
        }
    }

    public IMemberGroup? GetById(Guid id) => GetAsync(id).GetAwaiter().GetResult();

    public IMemberGroup? GetByName(string? name) => name is null ? null : GetByNameAsync(name).GetAwaiter().GetResult();

    public void Save(IMemberGroup memberGroup)
    {
        if (string.IsNullOrWhiteSpace(memberGroup.Name))
        {
            throw new InvalidOperationException("The name of a MemberGroup can not be empty");
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new MemberGroupSavingNotification(memberGroup, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            _memberGroupRepository.Save(memberGroup);
            scope.Complete();

            scope.Notifications.Publish(
                new MemberGroupSavedNotification(memberGroup, evtMsgs).WithStateFrom(savingNotification));
        }
    }

    public void Delete(IMemberGroup memberGroup) => DeleteAsync(memberGroup.Key).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public Task<IMemberGroup?> GetByNameAsync(string name)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_memberGroupRepository.GetByName(name));
    }

    /// <inheritdoc/>
    public Task<IMemberGroup?> GetAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_memberGroupRepository.Get(key));
    }

    /// <inheritdoc/>
    public Task<IEnumerable<IMemberGroup>> GetAllAsync()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_memberGroupRepository.GetMany());
    }

    public Task<IEnumerable<IMemberGroup>> GetByIdsAsync(IEnumerable<int> ids)
    {
        if (ids.Any() == false)
        {
            return Task.FromResult<IEnumerable<IMemberGroup>>(Array.Empty<IMemberGroup>());
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_memberGroupRepository.GetMany(ids.ToArray()));
    }

    /// <inheritdoc/>
    public async Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> CreateAsync(IMemberGroup memberGroup)
    {
        if (string.IsNullOrWhiteSpace(memberGroup.Name))
        {
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.CannotHaveEmptyName, null);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IMemberGroup? existingMemberGroup = await GetAsync(memberGroup.Key);
        if (existingMemberGroup is not null)
        {
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.DuplicateKey, null);
        }

        if (await NameAlreadyExistsAsync(memberGroup))
        {
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.DuplicateName, null);
        }

        var savingNotification = new MemberGroupSavingNotification(memberGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.CancelledByNotification, null);
        }

        _memberGroupRepository.Save(memberGroup);
        scope.Complete();

        scope.Notifications.Publish(new MemberGroupSavedNotification(memberGroup, eventMessages).WithStateFrom(savingNotification));
        return Attempt.SucceedWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.Success, memberGroup);
    }

    /// <inheritdoc/>
    public async Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> DeleteAsync(Guid key)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IMemberGroup? memberGroup = _memberGroupRepository.Get(key);

        if (memberGroup is null)
        {
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.NotFound, null);
        }

        var deletingNotification = new MemberGroupDeletingNotification(memberGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.CancelledByNotification, null);
        }

        _memberGroupRepository.Delete(memberGroup);
        scope.Complete();

        scope.Notifications.Publish(new MemberGroupDeletedNotification(memberGroup, eventMessages).WithStateFrom(deletingNotification));

        return Attempt.SucceedWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.Success, memberGroup);
    }

    public async Task<Attempt<IMemberGroup?, MemberGroupOperationStatus>> UpdateAsync(IMemberGroup memberGroup)
    {
        if (string.IsNullOrWhiteSpace(memberGroup.Name))
        {
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.CannotHaveEmptyName, null);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IMemberGroup? existingMemberGroup = await GetByNameAsync(memberGroup.Name!);

        if (existingMemberGroup is not null && existingMemberGroup.Key != memberGroup.Key)
        {
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.DuplicateName, null);
        }

        var savingNotification = new MemberGroupSavingNotification(memberGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.CancelledByNotification, null);
        }

        _memberGroupRepository.Save(memberGroup);
        scope.Complete();

        scope.Notifications.Publish(new MemberGroupSavedNotification(memberGroup, eventMessages).WithStateFrom(savingNotification));
        return Attempt.SucceedWithStatus<IMemberGroup?, MemberGroupOperationStatus>(MemberGroupOperationStatus.Success, memberGroup);
    }

    private async Task<bool> NameAlreadyExistsAsync(IMemberGroup memberGroup)
    {
        IMemberGroup? existingMemberGroup = await GetByNameAsync(memberGroup.Name!);
        return existingMemberGroup is not null;
    }
}
