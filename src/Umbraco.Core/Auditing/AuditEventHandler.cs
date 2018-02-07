using System;
using System.Linq;
using System.Web;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Core.Auditing
{
    internal class AuditEventHandler : ApplicationEventHandler
    {
        private IAuditService AuditService => ApplicationContext.Current.Services.AuditService;

        private IUser PerformingUser
        {
            get
            {
                if (HttpContext.Current == null) return new User { Id = 0, Name = "(no user)", Email = "" };

                var httpContext = new HttpContextWrapper(HttpContext.Current);
                var identity = httpContext.GetCurrentIdentity(false);
                if (identity == null) return new User { Id = 0, Name = "(no user)", Email = "" };

                return ApplicationContext.Current.Services.UserService.GetUserById(Convert.ToInt32(identity.Id));
            }
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

            UserService.SavedUserGroup += OnSavedUserGroup;

            UserService.SavedUser += OnSavedUser;
            UserService.DeletedUser += OnDeletedUser;
            UserService.UserGroupPermissionsAssigned += UserGroupPermissionAssigned;

            MemberService.Saved += OnSavedMember;
            MemberService.Deleted += OnDeletedMember;
        }

        private void OnSavedUserGroup(IUserService sender, SaveEventArgs<IUserGroup> saveEventArgs)
        {
            var performingUser = PerformingUser;
            var groups = saveEventArgs.SavedEntities;
            foreach (var group in groups)
            {
                //var dp = string.Join(", ", member.Properties.Where(x => x.WasDirty()).Select(x => x.Alias));
                var dp = string.Join(", ", ((UserGroup) group).GetWereDirtyProperties());
                var sections = string.Join(", ", group.AllowedSections);
                var perms = string.Join(", ", group.Permissions);

                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", PerformingIp,
                    DateTime.Now,
                    0, null,
                    "umbraco/user", $"save group id:{group.Id}:{group.Alias} \"{group.Name}\", updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}, sections: {sections}, perms: {perms}");
            }
        }

        private void UserGroupPermissionAssigned(IUserService sender, SaveEventArgs<EntityPermission> saveEventArgs)
        {
            var performingUser = PerformingUser;
            var perms = saveEventArgs.SavedEntities;
            foreach (var perm in perms)
            {
                var group = sender.GetUserGroupById(perm.UserGroupId);
                var assigned = string.Join(", ", perm.AssignedPermissions);
                var entity = ApplicationContext.Current.Services.EntityService.Get(perm.EntityId);

                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", PerformingIp,
                    DateTime.Now,
                    0, null,
                    "umbraco/user", $"assign group {(perm.IsDefaultPermissions ? "default " : "")}perms id:{group.Id}:{group.Alias} \"{group.Name}\", assigning {(string.IsNullOrWhiteSpace(assigned) ? "(nothing)" : assigned)} on id:{perm.EntityId} \"{entity.Name}\"");
            }
        }

        private void OnSavedMember(IMemberService sender, SaveEventArgs<IMember> saveEventArgs)
        {
            var performingUser = PerformingUser;
            var members = saveEventArgs.SavedEntities;
            foreach (var member in members)
            {
                //var dp = string.Join(", ", member.Properties.Where(x => x.WasDirty()).Select(x => x.Alias));
                var dp = string.Join(", ", ((Member) member).GetWereDirtyProperties());

                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", PerformingIp,
                    DateTime.Now,
                    0, null,
                    "umbraco/member", $"save member id:{member.Id} \"{member.Name}\" <{member.Email}>, updating {(string.IsNullOrWhiteSpace(dp) ? "(nothing)" : dp)}");
            }
        }

        private void OnDeletedMember(IMemberService sender, DeleteEventArgs<IMember> deleteEventArgs)
        {
            var performingUser = PerformingUser;
            var members = deleteEventArgs.DeletedEntities;
            foreach (var member in members)
            {
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", PerformingIp,
                    DateTime.Now,
                    0, null,
                    "umbraco/member", $"delete member id:{member.Id} \"{member.Name}\" <{member.Email}>");
            }
        }

        private void OnSavedUser(IUserService sender, SaveEventArgs<IUser> saveEventArgs)
        {
            var performingUser = PerformingUser;
            var affectedUsers = saveEventArgs.SavedEntities;
            foreach (var affectedUser in affectedUsers)
            {
                var sections = affectedUser.WasPropertyDirty("AllowedSections")
                    ? string.Join(", ", affectedUser.AllowedSections)
                    : null;
                var groups = affectedUser.WasPropertyDirty("Groups")
                    ? string.Join(", ", affectedUser.Groups.Select(x => x.Alias))
                    : null;

                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", PerformingIp,
                    DateTime.Now,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" <{affectedUser.Email}>",
                    "umbraco/user", $"save user{(sections == null ? "" : (", sections: " + sections))}{(groups == null ? "" : (", groups: " + groups))}");
            }
        }

        private void OnDeletedUser(IUserService sender, DeleteEventArgs<IUser> deleteEventArgs)
        {
            var performingUser = PerformingUser;
            var affectedUsers = deleteEventArgs.DeletedEntities;
            foreach (var affectedUser in affectedUsers)
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", PerformingIp,
                    DateTime.Now,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" <{affectedUser.Email}>",
                    "umbraco/user", "delete user");
        }

        private void OnLoginSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", identityArgs.IpAddress,
                    DateTime.Now,
                    0, null,
                    "umbraco/user", "login success");
            }
        }

        private void OnLogoutSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", identityArgs.IpAddress,
                    DateTime.Now,
                    0, null,
                    "umbraco/user", "logout success");
            }
        }

        private void OnPasswordReset(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                var affectedUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.AffectedUser);
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", identityArgs.IpAddress,
                    DateTime.Now,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" <{affectedUser.Email}>",
                    "umbraco/user", "password reset");
            }
        }

        private void OnPasswordChanged(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                var affectedUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.AffectedUser);
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", identityArgs.IpAddress,
                    DateTime.Now,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" <{affectedUser.Email}>",
                    "umbraco/user", "password change");
            }
        }

        private void OnLoginFailed(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", identityArgs.IpAddress,
                    DateTime.Now,
                    0, null,
                    "umbraco/user", "login failed");
            }
        }

        private void OnForgotPasswordChange(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                var affectedUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.AffectedUser);
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", identityArgs.IpAddress,
                    DateTime.Now,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" <{affectedUser.Email}>",
                    "umbraco/user", "password forgot/change");
            }
        }

        private void OnForgotPasswordRequest(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                var affectedUser = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.AffectedUser);
                AuditService.Write(performingUser.Id, $"User \"{performingUser.Name}\" <{performingUser.Email}>", identityArgs.IpAddress,
                    DateTime.Now,
                    affectedUser.Id, $"User \"{affectedUser.Name}\" <{affectedUser.Email}>",
                    "umbraco/user", "password forgot/request");
            }
        }
    }
}
