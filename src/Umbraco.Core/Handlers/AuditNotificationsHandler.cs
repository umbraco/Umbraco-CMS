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
    private readonly GlobalSettings _globalSettings;
    private readonly IIpResolver _ipResolver;
    private readonly IMemberService _memberService;
    private readonly IUserGroupService _userGroupService;
    private readonly IUserService _userService;

    public AuditNotificationsHandler(
        IAuditEntryService auditEntryService,
        IUserService userService,
        IEntityService entityService,
        IIpResolver ipResolver,
        IOptionsMonitor<GlobalSettings> globalSettings,
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
        _globalSettings = globalSettings.CurrentValue;
    }

    [Obsolete("Use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public AuditNotificationsHandler(
        IAuditService auditService,
        IUserService userService,
        IEntityService entityService,
        IIpResolver ipResolver,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberService memberService,
        IUserGroupService userGroupService)
    {
        _auditEntryService = StaticServiceProvider.Instance.GetRequiredService<IAuditEntryService>();
        _userService = userService;
        _entityService = entityService;
        _ipResolver = ipResolver;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberService = memberService;
        _userGroupService = userGroupService;
        _globalSettings = globalSettings.CurrentValue;
    }

    private IUser CurrentPerformingUser
    {
        get
        {
            IUser? identity = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            IUser? user = identity == null ? null : _userService.GetAsync(identity.Key).GetAwaiter().GetResult();
            return user ?? UnknownUser(_globalSettings);
        }
    }

    private string PerformingIp => _ipResolver.GetCurrentRequestIpAddress();

    public static IUser UnknownUser(GlobalSettings globalSettings) => new User(globalSettings)
    {
        Id = Constants.Security.UnknownUserId,
        Name = Constants.Security.UnknownUserName,
        Email = string.Empty,
    };

    /// <inheritdoc />
    public async Task HandleAsync(AssignedMemberRolesNotification notification, CancellationToken cancellationToken)
    {
        IUser performingUser = CurrentPerformingUser;
        var roles = string.Join(", ", notification.Roles);
        var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
        foreach (var id in notification.MemberIds)
        {
            members.TryGetValue(id, out IMember? member);
            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                -1,
                $"Member {id} \"{member?.Name ?? "(unknown)"}\" {FormatEmail(member)}",
                "umbraco/member/roles/assigned",
                $"roles modified, assigned {roles}");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(AssignedMemberRolesNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(AssignedUserGroupPermissionsNotification notification, CancellationToken cancellationToken)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<EntityPermission> perms = notification.EntityPermissions;
        foreach (EntityPermission perm in perms)
        {
            IUserGroup? group = _userGroupService.GetAsync(perm.UserGroupId).Result;
            var assigned = string.Join(", ", perm.AssignedPermissions);
            IEntitySlim? entity = _entityService.Get(perm.EntityId);

            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                -1,
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
        IUser performingUser = CurrentPerformingUser;
        IMember member = notification.Member;

        await _auditEntryService.WriteAsync(
            performingUser.Id,
            $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
            PerformingIp,
            DateTime.UtcNow,
            -1,
            $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
            "umbraco/member/exported",
            "exported member data");
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(ExportedMemberNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IMember> members = notification.DeletedEntities;
        foreach (IMember member in members)
        {
            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                -1,
                $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
                "umbraco/member/delete",
                $"delete member id:{member.Id} \"{member.Name}\" {FormatEmail(member)}");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(MemberDeletedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IMember> members = notification.SavedEntities;
        foreach (IMember member in members)
        {
            var dp = string.Join(", ", ((Member)member).GetWereDirtyProperties());

            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                -1,
                $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
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
        IUser performingUser = CurrentPerformingUser;
        var roles = string.Join(", ", notification.Roles);
        var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
        foreach (var id in notification.MemberIds)
        {
            members.TryGetValue(id, out IMember? member);
            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                -1,
                $"Member {id} \"{member?.Name ?? "(unknown)"}\" {FormatEmail(member)}",
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
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IUser> affectedUsers = notification.DeletedEntities;
        foreach (IUser affectedUser in affectedUsers)
        {
            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                affectedUser.Id,
                $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}",
                "umbraco/user/delete",
                "delete user");
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(UserDeletedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    public async Task HandleAsync(UserGroupWithUsersSavedNotification notification, CancellationToken cancellationToken)
    {
        IUser performingUser = CurrentPerformingUser;
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

            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                -1,
                $"User Group {group.Id} \"{group.Name}\" ({group.Alias})",
                "umbraco/user-group/save",
                $"{sb}");

            // now audit the users that have changed
            foreach (IUser user in groupWithUser.RemovedUsers)
            {
                await _auditEntryService.WriteAsync(
                    performingUser.Id,
                    $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                    PerformingIp,
                    DateTime.UtcNow,
                    user.Id,
                    $"User \"{user.Name}\" {FormatEmail(user)}",
                    "umbraco/user-group/save",
                    $"Removed user \"{user.Name}\" {FormatEmail(user)} from group {group.Id} \"{group.Name}\" ({group.Alias})");
            }

            foreach (IUser user in groupWithUser.AddedUsers)
            {
                await _auditEntryService.WriteAsync(
                    performingUser.Id,
                    $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                    PerformingIp,
                    DateTime.UtcNow,
                    user.Id,
                    $"User \"{user.Name}\" {FormatEmail(user)}",
                    "umbraco/user-group/save",
                    $"Added user \"{user.Name}\" {FormatEmail(user)} to group {group.Id} \"{group.Name}\" ({group.Alias})");
            }
        }
    }

    [Obsolete("Use HandleAsync() instead. Scheduled for removal in V19.")]
    public void Handle(UserGroupWithUsersSavedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IUser> affectedUsers = notification.SavedEntities;
        foreach (IUser affectedUser in affectedUsers)
        {
            var groups = affectedUser.WasPropertyDirty("Groups")
                ? string.Join(", ", affectedUser.Groups.Select(x => x.Alias))
                : null;

            var dp = string.Join(", ", ((User)affectedUser).GetWereDirtyProperties());

            await _auditEntryService.WriteAsync(
                performingUser.Id,
                $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
                PerformingIp,
                DateTime.UtcNow,
                affectedUser.Id,
                $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}",
                "umbraco/user/save",
                $"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}{(groups == null ? string.Empty : "; groups assigned: " + groups)}");
        }
    }

    public void Handle(UserSavedNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    private string FormatEmail(IMember? member) =>
        member == null ? string.Empty : member.Email.IsNullOrWhiteSpace() ? string.Empty : $"<{member.Email}>";

    private string FormatEmail(IUser user) => user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? string.Empty : $"<{user.Email}>";
}
