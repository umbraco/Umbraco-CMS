using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Handlers
{
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
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;
        private readonly IIpResolver _ipResolver;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly GlobalSettings _globalSettings;
        private readonly IMemberService _memberService;

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
                var identity = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
                var user = identity == null ? null : _userService.GetUserById(Convert.ToInt32(identity.Id));
                return user ?? UnknownUser(_globalSettings);
            }
        }

        public static IUser UnknownUser(GlobalSettings globalSettings) => new User(globalSettings) { Id = Constants.Security.UnknownUserId, Name = Constants.Security.UnknownUserName, Email = "" };

        private string PerformingIp => _ipResolver.GetCurrentRequestIpAddress();

        private string FormatEmail(IMember? member) => member == null ? string.Empty : member.Email.IsNullOrWhiteSpace() ? "" : $"<{member.Email}>";

        private string FormatEmail(IUser user) => user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? "" : $"<{user.Email}>";

        public void Handle(MemberSavedNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var members = notification.SavedEntities;
            foreach (var member in members)
            {
                var dp = string.Join(", ", ((Member)member).GetWereDirtyProperties());

                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
                    "umbraco/member/save", $"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}");
            }
        }

        public void Handle(MemberDeletedNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var members = notification.DeletedEntities;
            foreach (var member in members)
            {
                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
                    "umbraco/member/delete", $"delete member id:{member.Id} \"{member.Name}\" {FormatEmail(member)}");
            }
        }

        public void Handle(AssignedMemberRolesNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var roles = string.Join(", ", notification.Roles);
            var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
            foreach (var id in notification.MemberIds)
            {
                members.TryGetValue(id, out var member);
                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {id} \"{member?.Name ?? "(unknown)"}\" {FormatEmail(member)}",
                    "umbraco/member/roles/assigned", $"roles modified, assigned {roles}");
            }
        }

        public void Handle(RemovedMemberRolesNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var roles = string.Join(", ", notification.Roles);
            var members = _memberService.GetAllMembers(notification.MemberIds).ToDictionary(x => x.Id, x => x);
            foreach (var id in notification.MemberIds)
            {
                members.TryGetValue(id, out var member);
                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {id} \"{member?.Name ?? "(unknown)"}\" {FormatEmail(member)}",
                    "umbraco/member/roles/removed", $"roles modified, removed {roles}");
            }
        }

        public void Handle(ExportedMemberNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var member = notification.Member;

            _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                DateTime.UtcNow,
                -1, $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
                "umbraco/member/exported", "exported member data");
        }

        public void Handle(UserSavedNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var affectedUsers = notification.SavedEntities;
            foreach (var affectedUser in affectedUsers)
            {
                var groups = affectedUser.WasPropertyDirty("Groups")
                    ? string.Join(", ", affectedUser.Groups.Select(x => x.Alias))
                    : null;

                var dp = string.Join(", ", ((User)affectedUser).GetWereDirtyProperties());

                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}",
                    "umbraco/user/save", $"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}{(groups == null ? "" : "; groups assigned: " + groups)}");
            }
        }

        public void Handle(UserDeletedNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var affectedUsers = notification.DeletedEntities;
            foreach (var affectedUser in affectedUsers)
                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}",
                    "umbraco/user/delete", "delete user");
        }

        public void Handle(UserGroupWithUsersSavedNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            foreach (var groupWithUser in notification.SavedEntities)
            {
                var group = groupWithUser.UserGroup;

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
                    sb.Append($", assigned sections: {sections}");
                if (perms != null)
                {
                    if (sections != null)
                        sb.Append(", ");
                    sb.Append($"default perms: {perms}");
                }

                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"User Group {group.Id} \"{group.Name}\" ({group.Alias})",
                    "umbraco/user-group/save", $"{sb}");

                // now audit the users that have changed

                foreach (var user in groupWithUser.RemovedUsers)
                {
                    _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                        DateTime.UtcNow,
                        user.Id, $"User \"{user.Name}\" {FormatEmail(user)}",
                        "umbraco/user-group/save", $"Removed user \"{user.Name}\" {FormatEmail(user)} from group {group.Id} \"{group.Name}\" ({group.Alias})");
                }

                foreach (var user in groupWithUser.AddedUsers)
                {
                    _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                        DateTime.UtcNow,
                        user.Id, $"User \"{user.Name}\" {FormatEmail(user)}",
                        "umbraco/user-group/save", $"Added user \"{user.Name}\" {FormatEmail(user)} to group {group.Id} \"{group.Name}\" ({group.Alias})");
                }
            }
        }

        public void Handle(AssignedUserGroupPermissionsNotification notification)
        {
            var performingUser = CurrentPerformingUser;
            var perms = notification.EntityPermissions;
            foreach (EntityPermission perm in perms)
            {
                var group = _userService.GetUserGroupById(perm.UserGroupId);
                var assigned = string.Join(", ", perm.AssignedPermissions ?? Array.Empty<string>());
                var entity = _entityService.Get(perm.EntityId);

                _auditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"User Group {group?.Id} \"{group?.Name}\" ({group?.Alias})",
                    "umbraco/user-group/permissions-change", $"assigning {(string.IsNullOrWhiteSpace(assigned) ? "(nothing)" : assigned)} on id:{perm.EntityId} \"{entity?.Name}\"");
            }
        }
    }
}
