// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IExternalMemberService"/> for managing external-only members
///     that are not backed by the content system.
/// </summary>
internal sealed class ExternalMemberService : RepositoryService, IExternalMemberService
{
    private readonly IExternalMemberRepository _repository;
    private readonly IMemberService _memberService;
    private readonly IMemberGroupService _memberGroupService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IExternalLoginWithKeyRepository _externalLoginRepository;
    private readonly IOptionsMonitor<SecuritySettings> _securitySettings;
    private readonly ILogger<ExternalMemberService> _logger;

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
        IMemberTypeService memberTypeService,
        IExternalLoginWithKeyRepository externalLoginRepository,
        IOptionsMonitor<SecuritySettings> securitySettings)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _repository = repository;
        _memberService = memberService;
        _memberGroupService = memberGroupService;
        _memberTypeService = memberTypeService;
        _externalLoginRepository = externalLoginRepository;
        _securitySettings = securitySettings;
        _logger = loggerFactory.CreateLogger<ExternalMemberService>();
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
        // Cross-store uniqueness checks run in a separate read-only scope so they don't hold
        // SHARED table locks that would deadlock with concurrent write transactions on SQLite.
        // The unique indexes on the table are the ultimate guard against duplicates.
        using (ICoreScope readScope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            ExternalMemberOperationStatus? uniquenessResult = await ValidateUsernameUniqueAsync(member.UserName, null);
            if (uniquenessResult is not null)
            {
                return Attempt.FailWithStatus(uniquenessResult.Value, member);
            }

            if (_securitySettings.CurrentValue.MemberRequireUniqueEmail)
            {
                uniquenessResult = await ValidateEmailUniqueAsync(member.Email, null);
                if (uniquenessResult is not null)
                {
                    return Attempt.FailWithStatus(uniquenessResult.Value, member);
                }
            }
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Publish cancelable saving notification.
        var savingNotification = new ExternalMemberSavingNotification(member, evtMsgs);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(ExternalMemberOperationStatus.CancelledByNotification, member);
        }

        DateTime now = DateTime.UtcNow;
        if (member.CreateDate == default)
        {
            member.CreateDate = now;
        }

        member.UpdateDate = now;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "External member {MemberKey} created via full path (indexing will occur).",
                member.Key);
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
        // Cross-store uniqueness checks run in a separate read-only scope so they don't hold
        // SHARED table locks that would deadlock with concurrent write transactions on SQLite.
        using (ICoreScope readScope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            ExternalMemberOperationStatus? uniquenessResult = await ValidateUsernameUniqueAsync(member.UserName, member.Key);
            if (uniquenessResult is not null)
            {
                return Attempt.FailWithStatus(uniquenessResult.Value, member);
            }

            if (_securitySettings.CurrentValue.MemberRequireUniqueEmail)
            {
                uniquenessResult = await ValidateEmailUniqueAsync(member.Email, member.Key);
                if (uniquenessResult is not null)
                {
                    return Attempt.FailWithStatus(uniquenessResult.Value, member);
                }
            }
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Publish cancelable saving notification.
        var savingNotification = new ExternalMemberSavingNotification(member, evtMsgs);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(ExternalMemberOperationStatus.CancelledByNotification, member);
        }

        member.UpdateDate = DateTime.UtcNow;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "External member {MemberKey} updated via full path (indexing will occur).",
                member.Key);
        }

        await _repository.UpdateAsync(member);

        scope.Notifications.Publish(
            new ExternalMemberSavedNotification(member, evtMsgs).WithStateFrom(savingNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus>> UpdateLoginPropertiesAsync(ExternalMemberIdentity member)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "External member {MemberKey} login — lightweight update path (UpdateDate unchanged, no re-index).",
                member.Key);
        }

        // Login is not a member update: UpdateDate is intentionally left untouched, and the
        // IndexableFieldsChanged state flag tells the Examine indexing handler to skip the
        // re-index since no indexed field has changed. Mirror the content-member pattern
        // (MemberService.UpdateLoginPropertiesAsync) so downstream handlers see the same shape.
        var savingNotification = new ExternalMemberSavingNotification(member, evtMsgs);
        savingNotification.State.Add(Constants.Conventions.Member.LoginPropertiesOnlyStateKey, true);
        savingNotification.State.Add(Constants.Conventions.Member.IndexableFieldsChangedStateKey, false);

        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(ExternalMemberOperationStatus.CancelledByNotification, member);
        }

        await _repository.UpdateLoginPropertiesAsync(member);

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
    public async Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> AssignRolesAsync(Guid memberKey, string[] roleNames)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        ExternalMemberIdentity? member = await _repository.GetByKeyAsync(memberKey);
        if (member is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.NotFound, null);
        }

        var groupIds = ResolveGroupIds(roleNames);
        await _repository.AssignRolesAsync(member.Id, groupIds);

        scope.Notifications.Publish(new AssignedExternalMemberRolesNotification([memberKey], roleNames));

        scope.Complete();
        return Attempt.SucceedWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> RemoveRolesAsync(Guid memberKey, string[] roleNames)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        ExternalMemberIdentity? member = await _repository.GetByKeyAsync(memberKey);
        if (member is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.NotFound, null);
        }

        var groupIds = ResolveGroupIds(roleNames);
        await _repository.RemoveRolesAsync(member.Id, groupIds);

        scope.Notifications.Publish(new RemovedExternalMemberRolesNotification([memberKey], roleNames));

        scope.Complete();
        return Attempt.SucceedWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.Success, member);
    }

    /// <inheritdoc />
    public async Task<ExternalMemberOperationStatus> ValidateConvertToContentMemberAsync(Guid memberKey, string memberTypeAlias)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        (ExternalMemberOperationStatus status, _) = await ValidateConvertToContentMemberInternalAsync(memberKey, memberTypeAlias);
        return status;
    }

    /// <inheritdoc />
    public async Task<Attempt<IMember?, ExternalMemberOperationStatus>> ConvertToContentMemberAsync(Guid memberKey, string memberTypeAlias, Action<IMember, string?>? mapProfileData = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        (ExternalMemberOperationStatus status, ExternalMemberIdentity? externalMember) = await ValidateConvertToContentMemberInternalAsync(memberKey, memberTypeAlias);
        if (status != ExternalMemberOperationStatus.Success)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IMember?, ExternalMemberOperationStatus>(status, null);
        }

        // externalMember is guaranteed non-null when status is Success.
        ExternalMemberIdentity source = externalMember!;
        IMember contentMember = BuildContentMember(source, memberTypeAlias, mapProfileData);

        // Save the content member (this assigns the node ID).
        _memberService.Save(contentMember);

        // Migrate group memberships: read external roles, assign to content member.
        var roleNames = (await _repository.GetRolesAsync(source.Key)).ToArray();
        if (roleNames.Length > 0)
        {
            _memberService.AssignRoles([contentMember.Id], roleNames);
        }

        // Delete the external member record and its group memberships.
        await _repository.DeleteAsync(source.Key);

        scope.Complete();

        // Re-fetch to get the fully hydrated entity.
        IMember? result = _memberService.GetById(contentMember.Key);
        return Attempt.SucceedWithStatus(ExternalMemberOperationStatus.Success, result);
    }

    private IMember BuildContentMember(ExternalMemberIdentity externalMember, string memberTypeAlias, Action<IMember, string?>? mapProfileData)
    {
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

        return contentMember;
    }

    /// <inheritdoc />
    public async Task<ExternalMemberOperationStatus> ValidateConvertToExternalMemberAsync(Guid memberKey, bool requireExternalLogin = true)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        (ExternalMemberOperationStatus status, _) = await ValidateConvertToExternalMemberInternalAsync(memberKey, requireExternalLogin);
        return status;
    }

    /// <inheritdoc />
    public async Task<Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus>> ConvertToExternalMemberAsync(
        Guid memberKey,
        Action<ExternalMemberIdentity, IMember>? mapProfileData = null,
        bool requireExternalLogin = true)
    {
        static Attempt<ExternalMemberIdentity?, ExternalMemberOperationStatus> Fail(ExternalMemberOperationStatus status)
            => Attempt.FailWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(status, null);

        ExternalMemberIdentity identity;
        IReadOnlyCollection<IExternalLogin> capturedLogins;
        IReadOnlyCollection<IExternalLoginToken> capturedTokens;

        // Scope A — validate, capture state, delete the content member and persist the external identity.
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            (ExternalMemberOperationStatus status, IMember? member) = await ValidateConvertToExternalMemberInternalAsync(memberKey, requireExternalLogin);
            if (status != ExternalMemberOperationStatus.Success)
            {
                scope.Complete();
                return Fail(status);
            }

            // Capture group memberships and external login links before the member is deleted.
            var roleNames = _memberService.GetAllRoles(member!.Username).ToArray();
            capturedLogins = CaptureExternalLogins(memberKey);
            capturedTokens = CaptureExternalLoginTokens(memberKey);

            identity = BuildExternalIdentity(member, mapProfileData);

            EventMessages evtMsgs = EventMessagesFactory.Get();
            var savingNotification = new ExternalMemberSavingNotification(identity, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                // Roll back: leave the content member untouched.
                return Fail(ExternalMemberOperationStatus.CancelledByNotification);
            }

            // Delete the content member. This queues a deferred MemberDeletedNotification which, at the end
            // of the outermost scope, deletes the external login links by key — hence the re-link in scope B.
            if (_memberService.Delete(member).Success is false)
            {
                // Delete was cancelled by a notification handler; roll back without persisting the identity.
                return Fail(ExternalMemberOperationStatus.CancelledByNotification);
            }

            await PersistExternalIdentityAsync(identity, roleNames);

            scope.Notifications.Publish(
                new ExternalMemberSavedNotification(identity, evtMsgs).WithStateFrom(savingNotification));

            scope.Complete();
        }

        // Scope B — re-link external logins and tokens. The deferred delete handler from scope A has now
        // removed them, so they must be re-saved against the preserved member key.
        RelinkExternalLogins(memberKey, capturedLogins, capturedTokens);

        return Attempt.SucceedWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.Success, identity);
    }

    private IReadOnlyCollection<IExternalLogin> CaptureExternalLogins(Guid memberKey)
        => _externalLoginRepository
            .Get(Query<IIdentityUserLogin>().Where(x => x.Key == memberKey))
            .Select(x => (IExternalLogin)new ExternalLogin(x.LoginProvider, x.ProviderKey, x.UserData))
            .ToArray();

    private IReadOnlyCollection<IExternalLoginToken> CaptureExternalLoginTokens(Guid memberKey)
        => _externalLoginRepository
            .Get(Query<IIdentityUserToken>().Where(x => x.Key == memberKey))
            .Select(x => (IExternalLoginToken)new ExternalLoginToken(x.LoginProvider, x.Name, x.Value))
            .ToArray();

    private static ExternalMemberIdentity BuildExternalIdentity(IMember member, Action<ExternalMemberIdentity, IMember>? mapProfileData)
    {
        var identity = new ExternalMemberIdentity
        {
            // Preserve the Guid key so external login links continue to resolve.
            Key = member.Key,
            Email = member.Email,
            UserName = member.Username,
            Name = member.Name,
            IsApproved = member.IsApproved,
            IsLockedOut = member.IsLockedOut,
            CreateDate = member.CreateDate,
            UpdateDate = DateTime.UtcNow,

            // Invalidate active sessions by setting a new security stamp.
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        // Allow the caller to map content properties into the identity (e.g. profile data) before save.
        mapProfileData?.Invoke(identity, member);

        return identity;
    }

    private async Task PersistExternalIdentityAsync(ExternalMemberIdentity identity, string[] roleNames)
    {
        // The content row is deleted within this transaction, so the external store's unique indexes
        // (key/username/email) no longer collide.
        identity.Id = await _repository.CreateAsync(identity);

        if (roleNames.Length > 0)
        {
            await _repository.AssignRolesAsync(identity.Id, ResolveGroupIds(roleNames));
        }
    }

    private void RelinkExternalLogins(Guid memberKey, IReadOnlyCollection<IExternalLogin> logins, IReadOnlyCollection<IExternalLoginToken> tokens)
    {
        if (logins.Count == 0 && tokens.Count == 0)
        {
            return;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        if (logins.Count > 0)
        {
            _externalLoginRepository.Save(memberKey, logins);
        }

        if (tokens.Count > 0)
        {
            _externalLoginRepository.Save(memberKey, tokens);
        }

        scope.Complete();
    }

    private async Task<(ExternalMemberOperationStatus Status, ExternalMemberIdentity? Member)> ValidateConvertToContentMemberInternalAsync(Guid memberKey, string memberTypeAlias)
    {
        ExternalMemberIdentity? externalMember = await _repository.GetByKeyAsync(memberKey);
        if (externalMember is null)
        {
            return (ExternalMemberOperationStatus.NotFound, null);
        }

        if (_memberTypeService.Get(memberTypeAlias) is null)
        {
            return (ExternalMemberOperationStatus.InvalidMemberType, externalMember);
        }

        // The external member legitimately owns its username/email, so exclude it from the uniqueness
        // checks (which span both stores) — only a *different* record clashing is a real conflict. For
        // this direction the meaningful clash is a content member that already owns the username/email.
        ExternalMemberOperationStatus? uniquenessResult = await ValidateUsernameUniqueAsync(externalMember.UserName, externalMember.Key);
        if (uniquenessResult is not null)
        {
            return (uniquenessResult.Value, externalMember);
        }

        if (_securitySettings.CurrentValue.MemberRequireUniqueEmail)
        {
            uniquenessResult = await ValidateEmailUniqueAsync(externalMember.Email, externalMember.Key);
            if (uniquenessResult is not null)
            {
                return (uniquenessResult.Value, externalMember);
            }
        }

        return (ExternalMemberOperationStatus.Success, externalMember);
    }

    private async Task<(ExternalMemberOperationStatus Status, IMember? Member)> ValidateConvertToExternalMemberInternalAsync(Guid memberKey, bool requireExternalLogin)
    {
        IMember? member = _memberService.GetById(memberKey);
        if (member is null)
        {
            return (ExternalMemberOperationStatus.NotFound, null);
        }

        if (requireExternalLogin
            && _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.Key == memberKey)).Any() is false)
        {
            return (ExternalMemberOperationStatus.NoExternalLogin, member);
        }

        // The content member legitimately owns its username/email, so exclude it from the uniqueness
        // checks (which span both stores) — only a *different* record clashing is a real conflict.
        ExternalMemberOperationStatus? uniquenessResult = await ValidateUsernameUniqueAsync(member.Username, memberKey);
        if (uniquenessResult is not null)
        {
            return (uniquenessResult.Value, member);
        }

        if (_securitySettings.CurrentValue.MemberRequireUniqueEmail)
        {
            uniquenessResult = await ValidateEmailUniqueAsync(member.Email, memberKey);
            if (uniquenessResult is not null)
            {
                return (uniquenessResult.Value, member);
            }
        }

        return (ExternalMemberOperationStatus.Success, member);
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
