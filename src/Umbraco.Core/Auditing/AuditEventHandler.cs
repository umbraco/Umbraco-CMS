using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Core.Auditing
{
    public sealed class AuditEventHandler : ApplicationEventHandler
    {
        private IAuditService _auditServiceInstance;
        private IUserService _userServiceInstance;
        private IEntityService _entityServiceInstance;

        private IUser CurrentPerformingUser
        {
            get
            {
                var identity = Thread.CurrentPrincipal?.GetUmbracoIdentity();
                return identity == null
                    ? new User { Id = 0, Name = "SYSTEM", Email = "" }
                    : _userServiceInstance.GetUserById(Convert.ToInt32(identity.Id));
            }
        }

        private IUser GetPerformingUser(int userId)
        {
            var found = userId >= 0 ? _userServiceInstance.GetUserById(userId) : null;
            return found ?? new User {Id = 0, Name = "SYSTEM", Email = ""};
        }

        private string PerformingIp
        {
            get
            {
                var httpContext = HttpContext.Current == null ? (HttpContextBase) null : new HttpContextWrapper(HttpContext.Current);
                var ip = httpContext.GetCurrentRequestIpAddress();
                if (ip.ToLowerInvariant().StartsWith("unknown")) ip = "";
                return ip;
            }
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _auditServiceInstance = applicationContext.Services.AuditService;
            _userServiceInstance = applicationContext.Services.UserService;
            _entityServiceInstance = applicationContext.Services.EntityService;

            //BackOfficeUserManager.AccountLocked += ;
            //BackOfficeUserManager.AccountUnlocked += ;
            BackOfficeUserManager.ForgotPasswordRequested += OnForgotPasswordRequest;
            BackOfficeUserManager.ForgotPasswordChangedSuccess += OnForgotPasswordChange;
            BackOfficeUserManager.LoginFailed += OnLoginFailed;
            //BackOfficeUserManager.LoginRequiresVerification += ;
            BackOfficeUserManager.LoginSuccess += OnLoginSuccess;
            BackOfficeUserManager.LogoutSuccess += OnLogoutSuccess;
            BackOfficeUserManager.PasswordChanged += OnPasswordChanged;
            BackOfficeUserManager.PasswordReset += OnPasswordReset;
            //BackOfficeUserManager.ResetAccessFailedCount += ;

            UserService.SavedUserGroup2 += OnSavedUserGroupWithUsers;

            UserService.SavedUser += OnSavedUser;
            UserService.DeletedUser += OnDeletedUser;
            UserService.UserGroupPermissionsAssigned += UserGroupPermissionAssigned;

            MemberService.Saved += OnSavedMember;
            MemberService.Deleted += OnDeletedMember;
            MemberService.AssignedRoles += OnAssignedRoles;
            MemberService.RemovedRoles += OnRemovedRoles;
            MemberService.Exported += OnMemberExported;
        }

        private string FormatEmail(IMember member)
        {
            return member == null ? string.Empty : member.Email.IsNullOrWhiteSpace() ? "" : $"<{member.Email}>";
        }

        private string FormatEmail(IUser user)
        {
            return user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? "" : $"<{user.Email}>";
        }

        private void OnRemovedRoles(IMemberService sender, RolesEventArgs args)
        {
            var performingUser = CurrentPerformingUser;
            var roles = string.Join(", ", args.Roles);
            var members = sender.GetAllMembers(args.MemberIds).ToDictionary(x => x.Id, x => x);
            foreach (var id in args.MemberIds)
            {
                members.TryGetValue(id, out var member);
                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {id} \"{member?.Name ?? "(unknown)"}\" {FormatEmail(member)}",
                    "umbraco/member/roles/removed", $"roles modified, removed {roles}");
            }
        }

        private void OnAssignedRoles(IMemberService sender, RolesEventArgs args)
        {
            var performingUser = CurrentPerformingUser;
            var roles = string.Join(", ", args.Roles);
            var members = sender.GetAllMembers(args.MemberIds).ToDictionary(x => x.Id, x => x);
            foreach (var id in args.MemberIds)
            {
                members.TryGetValue(id, out var member);
                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {id} \"{member?.Name ?? "(unknown)"}\" {FormatEmail(member)}",
                    "umbraco/member/roles/assigned", $"roles modified, assigned {roles}");
            }
        }

        private void OnMemberExported(IMemberService sender, ExportedMemberEventArgs exportedMemberEventArgs)
        {
            var performingUser = CurrentPerformingUser;
            var member = exportedMemberEventArgs.Member;

            _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                DateTime.UtcNow,
                -1, $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
                "umbraco/member/exported", "exported member data");
        }

        private void OnSavedUserGroupWithUsers(IUserService sender, SaveEventArgs<UserGroupWithUsers> saveEventArgs)
        {
            var performingUser = CurrentPerformingUser;
            foreach (var groupWithUser in saveEventArgs.SavedEntities)
            {
                var group = groupWithUser.UserGroup;

                var dp = string.Join(", ", ((UserGroup)group).GetPreviouslyDirtyProperties());
                var sections = ((UserGroup)group).WasPropertyDirty("AllowedSections")
                    ? string.Join(", ", group.AllowedSections)
                    : null;
                var perms = ((UserGroup)group).WasPropertyDirty("Permissions")
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

                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"User Group {group.Id} \"{group.Name}\" ({group.Alias})",
                    "umbraco/user-group/save", $"{sb}");

                // now audit the users that have changed

                foreach (var user in groupWithUser.RemovedUsers)
                {
                    _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                        DateTime.UtcNow,
                        user.Id, $"User \"{user.Name}\" {FormatEmail(user)}",
                        "umbraco/user-group/save", $"Removed user \"{user.Name}\" {FormatEmail(user)} from group {group.Id} \"{group.Name}\" ({group.Alias})");
                }

                foreach (var user in groupWithUser.AddedUsers)
                {
                    _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                        DateTime.UtcNow,
                        user.Id, $"User \"{user.Name}\" {FormatEmail(user)}",
                        "umbraco/user-group/save", $"Added user \"{user.Name}\" {FormatEmail(user)} to group {group.Id} \"{group.Name}\" ({group.Alias})");
                }
            }
        }

        private void UserGroupPermissionAssigned(IUserService sender, SaveEventArgs<EntityPermission> saveEventArgs)
        {
            var performingUser = CurrentPerformingUser;
            var perms = saveEventArgs.SavedEntities;
            foreach (var perm in perms)
            {
                var group = sender.GetUserGroupById(perm.UserGroupId);
                var assigned = string.Join(", ", perm.AssignedPermissions);
                var entity = _entityServiceInstance.Get(perm.EntityId);

                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"User Group {group.Id} \"{group.Name}\" ({group.Alias})",
                    "umbraco/user-group/permissions-change", $"assigning {(string.IsNullOrWhiteSpace(assigned) ? "(nothing)" : assigned)} on id:{perm.EntityId} \"{entity.Name}\"");
            }
        }

        private void OnSavedMember(IMemberService sender, SaveEventArgs<IMember> saveEventArgs)
        {
            var performingUser = CurrentPerformingUser;
            var members = saveEventArgs.SavedEntities;
            foreach (var member in members)
            {
                var dp = string.Join(", ", ((Member) member).GetPreviouslyDirtyProperties());

                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
                    "umbraco/member/save", $"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}");
            }
        }

        private void OnDeletedMember(IMemberService sender, DeleteEventArgs<IMember> deleteEventArgs)
        {
            var performingUser = CurrentPerformingUser;
            var members = deleteEventArgs.DeletedEntities;
            foreach (var member in members)
            {
                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    -1, $"Member {member.Id} \"{member.Name}\" {FormatEmail(member)}",
                    "umbraco/member/delete", $"delete member id:{member.Id} \"{member.Name}\" {FormatEmail(member)}");
            }
        }

        private void OnSavedUser(IUserService sender, SaveEventArgs<IUser> saveEventArgs)
        {
            var performingUser = CurrentPerformingUser;
            var affectedUsers = saveEventArgs.SavedEntities;
            foreach (var affectedUser in affectedUsers)
            {
                var groups = affectedUser.WasPropertyDirty("Groups")
                    ? string.Join(", ", affectedUser.Groups.Select(x => x.Alias))
                    : null;

                var dp = string.Join(", ", ((User)affectedUser).GetPreviouslyDirtyProperties());

                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}",
                    "umbraco/user/save", $"updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}{(groups == null ? "" : "; groups assigned: " + groups)}");
            }
        }

        private void OnDeletedUser(IUserService sender, DeleteEventArgs<IUser> deleteEventArgs)
        {
            var performingUser = CurrentPerformingUser;
            var affectedUsers = deleteEventArgs.DeletedEntities;
            foreach (var affectedUser in affectedUsers)
                _auditServiceInstance.Write(performingUser.Id, $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}", PerformingIp,
                    DateTime.UtcNow,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}",
                    "umbraco/user/delete", "delete user");
        }

        private void OnLoginSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = GetPerformingUser(identityArgs.PerformingUser);
                WriteAudit(performingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/sign-in/login", "login success");
            }
        }

        private void OnLogoutSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = GetPerformingUser(identityArgs.PerformingUser);
                WriteAudit(performingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/sign-in/logout", "logout success");
            }
        }

        private void OnPasswordReset(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/reset", "password reset");
            }
        }

        private void OnPasswordChanged(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/change", "password change");
            }
        }

        private void OnLoginFailed(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, 0, identityArgs.IpAddress, "umbraco/user/sign-in/failed", "login failed", affectedDetails: "");
            }
        }

        private void OnForgotPasswordChange(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/forgot/change", "password forgot/change");
            }
        }

        private void OnForgotPasswordRequest(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/forgot/request", "password forgot/request");
            }
        }

        private void WriteAudit(int performingId, int affectedId, string ipAddress, string eventType, string eventDetails, string affectedDetails = null)
        {
            var performingUser = _userServiceInstance.GetUserById(performingId);

            var performingDetails = performingUser == null
                ? $"User UNKNOWN:{performingId}"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";

            WriteAudit(performingId, performingDetails, affectedId, ipAddress, eventType, eventDetails, affectedDetails);
        }

        private void WriteAudit(IUser performingUser, int affectedId, string ipAddress, string eventType, string eventDetails)
        {
            var performingDetails = performingUser == null
                ? $"User UNKNOWN"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";

            WriteAudit(performingUser?.Id ?? 0, performingDetails, affectedId, ipAddress, eventType, eventDetails);
        }

        private void WriteAudit(int performingId, string performingDetails, int affectedId, string ipAddress, string eventType, string eventDetails, string affectedDetails = null)
        {
            if (affectedDetails == null)
            {
                var affectedUser = _userServiceInstance.GetUserById(affectedId);
                affectedDetails = affectedUser == null
                    ? $"User UNKNOWN:{affectedId}"
                    : $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}";
            }

            _auditServiceInstance.Write(performingId, performingDetails,
                ipAddress,
                DateTime.UtcNow,
                affectedId, affectedDetails,
                eventType, eventDetails);
        }
    }
}
