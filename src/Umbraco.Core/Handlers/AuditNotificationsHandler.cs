using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Handlers;

public sealed class AuditNotificationsHandler :
    INotificationHandler<MemberSavedNotification>,
    INotificationAsyncHandler<MemberSavedNotification>,
    INotificationHandler<MemberDeletedNotification>,
    INotificationAsyncHandler<MemberDeletedNotification>,
    INotificationHandler<AssignedMemberRolesNotification>,
    INotificationAsyncHandler<AssignedMemberRolesNotification>,
    INotificationHandler<RemovedMemberRolesNotification>,
    INotificationAsyncHandler<RemovedMemberRolesNotification>,
    INotificationHandler<ExportedMemberNotification>,
    INotificationAsyncHandler<ExportedMemberNotification>,
    INotificationHandler<UserSavedNotification>,
    INotificationAsyncHandler<UserSavedNotification>,
    INotificationHandler<UserDeletedNotification>,
    INotificationAsyncHandler<UserDeletedNotification>,
    INotificationHandler<UserGroupWithUsersSavedNotification>,
    INotificationAsyncHandler<UserGroupWithUsersSavedNotification>,
    INotificationHandler<AssignedUserGroupPermissionsNotification>,
    INotificationAsyncHandler<AssignedUserGroupPermissionsNotification>
{
    private readonly IAuditEntryService _auditEntryService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly IIpResolver _ipResolver;
    private readonly IMemberService _memberService;
    private readonly IUserGroupService _userGroupService;
    private readonly IUserService _userService;

    public AuditNotificationsHandler(
        IAuditEntryService auditEntryService,
        IUserService userService,
        IEntityService entityService,
        IIpResolver ipResolver,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberService memberService,
        IUserGroupService userGroupService)
    {
        _auditEntryService = auditEntryService;
        _userService = userService;
        _entityService = entityService;
        _ipResolver = ipResolver;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberService = memberService;
        _userGroupService = userGroupService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in V19.")]
    public AuditNotificationsHandler(
        IAuditService auditService,
        IUserService userService,
        IEntityService entityService,
        IIpResolver ipResolver,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberService memberService,
        IUserGroupService userGroupService)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IAuditEntryService>(),
            userService,
            entityService,
            ipResolver,
            backOfficeSecurityAccessor,
            memberService,
            userGroupService)
    {
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in V19.")]
    public AuditNotificationsHandler(
        IAuditEntryService auditEntryService,
        IAuditService auditService,
        IUserService userService,
        IEntityService entityService,
        IIpResolver ipResolver,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberService memberService,
        IUserGroupService userGroupService)
        : this(
            auditEntryService,
            userService,
            entityService,
            ipResolver,
            backOfficeSecurityAccessor,
            memberService,
            userGroupService)
    {
    }

    private async Task<IUser?> GetCurrentPerformingUser() =>
        _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser is { } identity
            ? await _userService.GetAsync(identity.Key)
            : null;

    private string PerformingIp => _ipResolver.GetCurrentRequestIpAddress();

    /// <inheritdoc />
    public async Task HandleAsync(AssignedMemberRolesNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        var roles = string.Join(", ", notification.Roles);
        var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
        foreach (var id in notification.MemberIds)
        {
            members.TryGetValue(id, out IMember? member);

            await Audit(
                performingUser,
                null,
                affectedDetails: FormatDetails(id, member, appendType: true),
                "umbraco/member/roles/assigned",
                $"roles modified, assigned {roles}");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(AssignedMemberRolesNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(
        AssignedUserGroupPermissionsNotification notification,
        CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        IEnumerable<EntityPermission> perms = notification.EntityPermissions;
        foreach (EntityPermission perm in perms)
        {
            IUserGroup? group = _userGroupService.GetAsync(perm.UserGroupId).Result;
            var assigned = string.Join(", ", perm.AssignedPermissions);
            IEntitySlim? entity = _entityService.Get(perm.EntityId);

            await Audit(
                performingUser,
                null,
                $"User Group {group?.Id} \"{group?.Name}\" ({group?.Alias})",
                "umbraco/user-group/permissions-change",
                $"assigning {(string.IsNullOrWhiteSpace(assigned) ? "(nothing)" : assigned)} on id:{perm.EntityId} \"{entity?.Name}\"");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(AssignedUserGroupPermissionsNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(ExportedMemberNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        IMember member = notification.Member;

        await Audit(
            performingUser,
            null,
            affectedDetails: FormatDetails(member, appendType: true),
            "umbraco/member/exported",
            "exported member data");
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(ExportedMemberNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        IEnumerable<IMember> members = notification.DeletedEntities;
        foreach (IMember member in members)
        {
            await Audit(
                performingUser,
                null,
                affectedDetails: FormatDetails(member, appendType: true),
                "umbraco/member/delete",
                $"delete member id:{FormatDetails(member)}");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(MemberDeletedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        IEnumerable<IMember> members = notification.SavedEntities;
        foreach (IMember member in members)
        {
            var dp = string.Join(", ", ((Member)member).GetWereDirtyProperties());

            await Audit(
                performingUser,
                null,
                affectedDetails: FormatDetails(member, appendType: true),
                "umbraco/member/save",
                $"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(MemberSavedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(RemovedMemberRolesNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        var roles = string.Join(", ", notification.Roles);
        var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
        foreach (var id in notification.MemberIds)
        {
            members.TryGetValue(id, out IMember? member);

            await Audit(
                performingUser,
                null,
                affectedDetails: FormatDetails(id, member, appendType: true),
                "umbraco/member/roles/removed",
                $"roles modified, removed {roles}");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(RemovedMemberRolesNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(UserDeletedNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        IEnumerable<IUser> affectedUsers = notification.DeletedEntities;
        foreach (IUser affectedUser in affectedUsers)
        {
            await Audit(
                performingUser,
                affectedUser,
                null,
                "umbraco/user/delete",
                "delete user");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(UserDeletedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    public async Task HandleAsync(UserGroupWithUsersSavedNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        foreach (UserGroupWithUsers groupWithUser in notification.SavedEntities)
        {
            IUserGroup group = groupWithUser.UserGroup;

            var dp = string.Join(", ", ((UserGroup)group).GetWereDirtyProperties());
            var sections = ((UserGroup)group).WasPropertyDirty(nameof(group.AllowedSections))
                ? string.Join(", ", group.AllowedSections)
                : null;
            var perms = ((UserGroup)group).WasPropertyDirty(nameof(group.Permissions))
                ? string.Join(", ", group.Permissions)
                : null;

            var sb = new StringBuilder();
            sb.Append($"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)};");
            if (sections != null)
            {
                sb.Append($", assigned sections: {sections}");
            }

            if (perms != null)
            {
                if (sections != null)
                {
                    sb.Append(", ");
                }

                sb.Append($"default perms: {perms}");
            }

            await Audit(
                performingUser,
                null,
                $"User Group {FormatDetails(group)}",
                "umbraco/user-group/save",
                $"{sb}");

            // now audit the users that have changed
            foreach (IUser user in groupWithUser.RemovedUsers)
            {
                await Audit(
                    performingUser,
                    user,
                    null,
                    "umbraco/user-group/save",
                    $"Removed user {FormatDetails(user)} from group {FormatDetails(group)}");
            }

            foreach (IUser user in groupWithUser.AddedUsers)
            {
                await Audit(
                    performingUser,
                    user,
                    null,
                    "umbraco/user-group/save",
                    $"Added user {FormatDetails(user)} to group {FormatDetails(group)}");
            }
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(UserGroupWithUsersSavedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken)
    {
        IUser? performingUser = await GetCurrentPerformingUser();
        IEnumerable<IUser> affectedUsers = notification.SavedEntities;
        foreach (IUser affectedUser in affectedUsers)
        {
            var groups = affectedUser.WasPropertyDirty("Groups")
                ? string.Join(", ", affectedUser.Groups.Select(x => x.Alias))
                : null;

            var dp = string.Join(", ", ((User)affectedUser).GetWereDirtyProperties());

            await Audit(
                performingUser,
                affectedUser,
                null,
                "umbraco/user/save",
                $"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}{(groups == null ? string.Empty : "; groups assigned: " + groups)}");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(UserSavedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    private async Task Audit(
        IUser? performingUser,
        IUser? affectedUser,
        string? affectedDetails,
        string eventType,
        string eventDetails)
    {
        affectedDetails ??= affectedUser is null ? string.Empty : FormatDetails(affectedUser, appendType: true);
        await _auditEntryService.WriteAsync(
            performingUser?.Key,
            FormatDetails(performingUser, appendType: true),
            PerformingIp,
            DateTime.UtcNow,
            affectedUser?.Key,
            affectedDetails,
            eventType,
            eventDetails);
    }

    private static string FormatDetails(IUser? user, bool appendType = false)
    {
        var userName = user?.Name ?? Constants.Security.UnknownUserName;
        var details = appendType ? $"User \"{userName}\"" : $"\"{userName}\"";
        var email = FormatEmail(user?.Email);

        return email is not null
            ? $"{details} {email}"
            : details;
    }

    private static string FormatDetails(IMember member, bool appendType = false)
        => FormatDetails(member.Id, member, appendType);

    private static string FormatDetails(int id, IMember? member, bool appendType = false)
    {
        var userName = member?.Name ?? "(unknown)";
        var details = appendType ? $"Member {id} \"{userName}\"" : $"{id} \"{userName}\"";
        var email = FormatEmail(member?.Email);

        return email is not null
            ? $"{details} {email}"
            : details;
    }

    private static string FormatDetails(IUserGroup group) => $"{group.Id} \"{group.Name}\" ({group.Alias})";

    private static string? FormatEmail(string? email) => !email.IsNullOrWhiteSpace() ? $"<{email}>" : null;
}
