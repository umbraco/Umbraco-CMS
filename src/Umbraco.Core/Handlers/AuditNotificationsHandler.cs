using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
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
    INotificationHandler<MemberDeletedNotification>,
    INotificationHandler<AssignedMemberRolesNotification>,
    INotificationHandler<RemovedMemberRolesNotification>,
    INotificationHandler<ExportedMemberNotification>,
    INotificationHandler<UserSavedNotification>,
    INotificationHandler<UserDeletedNotification>,
    INotificationHandler<UserGroupWithUsersSavedNotification>,
    INotificationHandler<AssignedUserGroupPermissionsNotification>
{
    private readonly IAuditService _auditService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly GlobalSettings _globalSettings;
    private readonly IIpResolver _ipResolver;
    private readonly IMemberService _memberService;
    private readonly IUserService _userService;

    public AuditNotificationsHandler(
        IAuditService auditService,
        IUserService userService,
        IEntityService entityService,
        IIpResolver ipResolver,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberService memberService)
    {
        _auditService = auditService;
        _userService = userService;
        _entityService = entityService;
        _ipResolver = ipResolver;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberService = memberService;
        _globalSettings = globalSettings.CurrentValue;
    }

    private IUser CurrentPerformingUser
    {
        get
        {
            IUser? identity = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            IUser? user = identity == null ? null : _userService.GetUserById(Convert.ToInt32(identity.Id));
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

    public void Handle(AssignedMemberRolesNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        var roles = string.Join(", ", notification.Roles);
        var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
        foreach (var id in notification.MemberIds)
        {
            members.TryGetValue(id, out IMember? member);
            _auditService.Write(
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

    public void Handle(AssignedUserGroupPermissionsNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<EntityPermission> perms = notification.EntityPermissions;
        foreach (EntityPermission perm in perms)
        {
            IUserGroup? group = _userService.GetUserGroupById(perm.UserGroupId);
            var assigned = string.Join(", ", perm.AssignedPermissions ?? Array.Empty<string>());
            IEntitySlim? entity = _entityService.Get(perm.EntityId);

            _auditService.Write(
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

    public void Handle(ExportedMemberNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        IMember member = notification.Member;

        _auditService.Write(
            performingUser.Id,
            $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}",
            PerformingIp,
            DateTime.UtcNow,
            -1,
            $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
            "umbraco/member/exported",
            "exported member data");
    }

    public void Handle(MemberDeletedNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IMember> members = notification.DeletedEntities;
        foreach (IMember member in members)
        {
            _auditService.Write(
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

    public void Handle(MemberSavedNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IMember> members = notification.SavedEntities;
        foreach (IMember member in members)
        {
            var dp = string.Join(", ", ((Member)member).GetWereDirtyProperties());

            _auditService.Write(
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

    public void Handle(RemovedMemberRolesNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        var roles = string.Join(", ", notification.Roles);
        var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
        foreach (var id in notification.MemberIds)
        {
            members.TryGetValue(id, out IMember? member);
            _auditService.Write(
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

    public void Handle(UserDeletedNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IUser> affectedUsers = notification.DeletedEntities;
        foreach (IUser affectedUser in affectedUsers)
        {
            _auditService.Write(
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

    public void Handle(UserGroupWithUsersSavedNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        foreach (UserGroupWithUsers groupWithUser in notification.SavedEntities)
        {
            IUserGroup group = groupWithUser.UserGroup;

            var dp = string.Join(", ", ((UserGroup)group).GetWereDirtyProperties());
            var sections = ((UserGroup)group).WasPropertyDirty("AllowedSections")
                ? string.Join(", ", group.AllowedSections)
                : null;
            var perms = ((UserGroup)group).WasPropertyDirty("Permissions") && group.Permissions is not null
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

            _auditService.Write(
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
                _auditService.Write(
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
                _auditService.Write(
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

    public void Handle(UserSavedNotification notification)
    {
        IUser performingUser = CurrentPerformingUser;
        IEnumerable<IUser> affectedUsers = notification.SavedEntities;
        foreach (IUser affectedUser in affectedUsers)
        {
            var groups = affectedUser.WasPropertyDirty("Groups")
                ? string.Join(", ", affectedUser.Groups.Select(x => x.Alias))
                : null;

            var dp = string.Join(", ", ((User)affectedUser).GetWereDirtyProperties());

            _auditService.Write(
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

    private string FormatEmail(IMember? member) =>
        member == null ? string.Empty : member.Email.IsNullOrWhiteSpace() ? string.Empty : $"<{member.Email}>";

    private string FormatEmail(IUser user) => user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? string.Empty : $"<{user.Email}>";
}
