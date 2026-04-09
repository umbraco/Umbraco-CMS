using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for managing member groups in Umbraco.
/// </summary>
/// <remarks>
///     This service handles CRUD operations for member groups, which are used to organize
///     and categorize members for access control and content personalization purposes.
/// </remarks>
internal sealed class MemberGroupService : RepositoryService, IMemberGroupService
{
    private readonly IMemberGroupRepository _memberGroupRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="memberGroupRepository">The repository for member group operations.</param>
    public MemberGroupService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IMemberGroupRepository memberGroupRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _memberGroupRepository = memberGroupRepository;

    /// <inheritdoc />
    public IEnumerable<IMemberGroup> GetAll() => GetAllAsync().GetAwaiter().GetResult();

    /// <inheritdoc />
    public IEnumerable<IMemberGroup> GetByIds(IEnumerable<int> ids) => GetByIdsAsync(ids).GetAwaiter().GetResult();

    /// <inheritdoc />
    public IMemberGroup? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _memberGroupRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public IMemberGroup? GetById(Guid id) => GetAsync(id).GetAwaiter().GetResult();

    /// <inheritdoc />
    public IMemberGroup? GetByName(string? name) => name is null ? null : GetByNameAsync(name).GetAwaiter().GetResult();

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    ///     Determines whether a member group with the same name already exists.
    /// </summary>
    /// <param name="memberGroup">The member group to check for name duplication.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result is <c>true</c> if a member group with the same name exists; otherwise, <c>false</c>.
    /// </returns>
    private async Task<bool> NameAlreadyExistsAsync(IMemberGroup memberGroup)
    {
        IMemberGroup? existingMemberGroup = await GetByNameAsync(memberGroup.Name!);
        return existingMemberGroup is not null;
    }
}
