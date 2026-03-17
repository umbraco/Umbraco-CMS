// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
///     Implements <see cref="IExternalMemberService"/> for managing external-only members
///     that are not backed by the content system.
/// </summary>
internal sealed class ExternalMemberService : RepositoryService, IExternalMemberService
{
    private readonly IExternalMemberRepository _repository;
    private readonly IMemberService _memberService;
    private readonly IMemberGroupService _memberGroupService;
    private readonly IExternalLoginWithKeyRepository _externalLoginRepository;
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberService"/> class.
    /// </summary>
    public ExternalMemberService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IExternalMemberRepository repository,
        IMemberService memberService,
        IMemberGroupService memberGroupService,
        IExternalLoginWithKeyRepository externalLoginRepository,
        IOptionsMonitor<SecuritySettings> securitySettings)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _repository = repository;
        _memberService = memberService;
        _memberGroupService = memberGroupService;
        _externalLoginRepository = externalLoginRepository;
        _securitySettings = securitySettings.CurrentValue;
    }

    /// <inheritdoc />
    public async Task<ExternalMemberIdentity?> GetByKeyAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetByKeyAsync(key);
    }

    /// <inheritdoc />
    public async Task<ExternalMemberIdentity?> GetByEmailAsync(string email)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetByEmailAsync(email);
    }

    /// <inheritdoc />
    public async Task<ExternalMemberIdentity?> GetByUsernameAsync(string username)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetByUsernameAsync(username);
    }

    /// <inheritdoc />
    public async Task<PagedModel<ExternalMemberIdentity>> GetAllAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetPagedAsync(skip, take);
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> CreateAsync(ExternalMemberIdentity member, IExternalLogin? externalLogin = null)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Cross-store uniqueness: check username across both external and content member stores.
        ExternalMemberOperationStatus? uniquenessResult = await ValidateUsernameUniqueAsync(member.UserName, null);
        if (uniquenessResult is not null)
        {
            scope.Complete();
            return Attempt.FailWithStatus(uniquenessResult.Value, member);
        }

        // Cross-store uniqueness: check email if required.
        if (_securitySettings.MemberRequireUniqueEmail)
        {
            uniquenessResult = await ValidateEmailUniqueAsync(member.Email, null);
            if (uniquenessResult is not null)
            {
                scope.Complete();
                return Attempt.FailWithStatus(uniquenessResult.Value, member);
            }
        }

        // Publish cancelable saving notification.
        var savingNotification = new ExternalMemberSavingNotification(member, evtMsgs);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(ExternalMemberOperationStatus.CancelledByNotification, member);
        }

        // Persist.
        var id = await _repository.CreateAsync(member);
        member.Id = id;

        // Batched creation: save external login in the same scope if provided.
        if (externalLogin is not null)
        {
            _externalLoginRepository.Save(member.Key, new[] { externalLogin });
        }

        // Publish saved notification.
        scope.Notifications.Publish(
            new ExternalMemberSavedNotification(member, evtMsgs).WithStateFrom(savingNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> UpdateAsync(ExternalMemberIdentity member)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Cross-store uniqueness: check username across both stores (exclude self).
        ExternalMemberOperationStatus? uniquenessResult = await ValidateUsernameUniqueAsync(member.UserName, member.Key);
        if (uniquenessResult is not null)
        {
            scope.Complete();
            return Attempt.FailWithStatus(uniquenessResult.Value, member);
        }

        // Cross-store uniqueness: check email if required (exclude self).
        if (_securitySettings.MemberRequireUniqueEmail)
        {
            uniquenessResult = await ValidateEmailUniqueAsync(member.Email, member.Key);
            if (uniquenessResult is not null)
            {
                scope.Complete();
                return Attempt.FailWithStatus(uniquenessResult.Value, member);
            }
        }

        // Publish cancelable saving notification.
        var savingNotification = new ExternalMemberSavingNotification(member, evtMsgs);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(ExternalMemberOperationStatus.CancelledByNotification, member);
        }

        await _repository.UpdateAsync(member);

        scope.Notifications.Publish(
            new ExternalMemberSavedNotification(member, evtMsgs).WithStateFrom(savingNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> DeleteAsync(Guid key)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        ExternalMemberIdentity? member = await _repository.GetByKeyAsync(key);
        if (member is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.NotFound, null);
        }

        // Publish cancelable deleting notification.
        var deletingNotification = new ExternalMemberDeletingNotification(member, evtMsgs);
        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.CancelledByNotification, member);
        }

        // Delete external logins first.
        _externalLoginRepository.DeleteUserLogins(key);

        await _repository.DeleteAsync(key);

        scope.Notifications.Publish(
            new ExternalMemberDeletedNotification(member, evtMsgs));

        scope.Complete();
        return Attempt.SucceedWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetRolesAsync(Guid memberKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetRolesAsync(memberKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> AssignRolesAsync(Guid memberKey, string[] roleNames)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        ExternalMemberIdentity? member = await _repository.GetByKeyAsync(memberKey);
        if (member is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus(ExternalMemberOperationStatus.NotFound, member!);
        }

        var groupIds = ResolveGroupIds(roleNames);
        await _repository.AssignRolesAsync(member.Id, groupIds);

        scope.Notifications.Publish(new AssignedExternalMemberRolesNotification([memberKey], roleNames));

        scope.Complete();
        return Attempt.SucceedWithStatus(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> RemoveRolesAsync(Guid memberKey, string[] roleNames)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        ExternalMemberIdentity? member = await _repository.GetByKeyAsync(memberKey);
        if (member is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus(ExternalMemberOperationStatus.NotFound, member!);
        }

        var groupIds = ResolveGroupIds(roleNames);
        await _repository.RemoveRolesAsync(member.Id, groupIds);

        scope.Notifications.Publish(new RemovedExternalMemberRolesNotification([memberKey], roleNames));

        scope.Complete();
        return Attempt.SucceedWithStatus(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<Attempt<IMember?, ExternalMemberOperationStatus>> ConvertToContentMemberAsync(Guid memberKey, string memberTypeAlias, Action<IMember, string?>? mapProfileData = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Load the external member.
        ExternalMemberIdentity? externalMember = await _repository.GetByKeyAsync(memberKey);
        if (externalMember is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IMember?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.NotFound, null);
        }

        // Create the content member entity.
        IMember contentMember = _memberService.CreateMember(
            externalMember.UserName,
            externalMember.Email,
            externalMember.Name ?? externalMember.UserName,
            memberTypeAlias);

        // Preserve the Guid key so external login links continue to resolve.
        contentMember.Key = externalMember.Key;
        contentMember.IsApproved = externalMember.IsApproved;
        contentMember.IsLockedOut = externalMember.IsLockedOut;

        // Invalidate active sessions by setting a new security stamp.
        contentMember.SecurityStamp = Guid.NewGuid().ToString();

        // Allow the caller to map profileData fields to content properties before save.
        mapProfileData?.Invoke(contentMember, externalMember.ProfileData);

        // Save the content member (this assigns the node ID).
        _memberService.Save(contentMember);

        // Migrate group memberships: read external roles, assign to content member.
        IEnumerable<string> roles = await _repository.GetRolesAsync(externalMember.Key);
        var roleNames = roles.ToArray();
        if (roleNames.Length > 0)
        {
            _memberService.AssignRoles([contentMember.Id], roleNames);
        }

        // Delete the external member record and its group memberships.
        await _repository.DeleteAsync(externalMember.Key);

        scope.Complete();

        // Re-fetch to get the fully hydrated entity.
        IMember? result = _memberService.GetById(contentMember.Key);
        return Attempt.SucceedWithStatus(ExternalMemberOperationStatus.Success, result);
    }

    private async Task<ExternalMemberOperationStatus?> ValidateUsernameUniqueAsync(string username, Guid? excludeKey)
    {
        // Check external store.
        ExternalMemberIdentity? existingExternal = await _repository.GetByUsernameAsync(username);
        if (existingExternal is not null && existingExternal.Key != excludeKey)
        {
            return ExternalMemberOperationStatus.DuplicateUsername;
        }

        // Check content store.
        IMember? existingContent = _memberService.GetByUsername(username);
        if (existingContent is not null && existingContent.Key != excludeKey)
        {
            return ExternalMemberOperationStatus.DuplicateUsername;
        }

        return null;
    }

    private async Task<ExternalMemberOperationStatus?> ValidateEmailUniqueAsync(string email, Guid? excludeKey)
    {
        // Check external store.
        ExternalMemberIdentity? existingExternal = await _repository.GetByEmailAsync(email);
        if (existingExternal is not null && existingExternal.Key != excludeKey)
        {
            return ExternalMemberOperationStatus.DuplicateEmail;
        }

        // Check content store.
        IMember? existingContent = _memberService.GetByEmail(email);
        if (existingContent is not null && existingContent.Key != excludeKey)
        {
            return ExternalMemberOperationStatus.DuplicateEmail;
        }

        return null;
    }

    private int[] ResolveGroupIds(string[] roleNames)
    {
        var ids = new List<int>(roleNames.Length);
        foreach (var roleName in roleNames)
        {
            IMemberGroup? group = _memberGroupService.GetByName(roleName);
            if (group is not null)
            {
                ids.Add(group.Id);
            }
        }

        return ids.ToArray();
    }
}
