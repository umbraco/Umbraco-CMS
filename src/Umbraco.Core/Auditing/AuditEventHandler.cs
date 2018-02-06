using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Core.Auditing
{
    internal class AuditEventHandler : ApplicationEventHandler
    {
        private IAuditService AuditService => ApplicationContext.Current.Services.AuditService;

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // fixme what shall we handle here exactly?
            // these are the events handled by the Perplex package
            // we also want
            // - updating user and member values
            // - giving and revoking permissions
            // - exporting forms stuff
            // - ?

            //BackOfficeUserManager.AccountLocked += ;
            //BackOfficeUserManager.AccountUnlocked += ;
            //BackOfficeUserManager.ForgotPasswordRequested += ;
            //BackOfficeUserManager.ForgotPasswordChangedSuccess += ;
            //BackOfficeUserManager.LoginFailed += ;
            //BackOfficeUserManager.LoginRequiresVerification += ;
            BackOfficeUserManager.LoginSuccess += OnLoginSuccess;
            BackOfficeUserManager.LogoutSuccess += OnLogoutSuccess;
            //BackOfficeUserManager.PasswordChanged += ;
            //BackOfficeUserManager.PasswordReset += ;
            //BackOfficeUserManager.ResetAccessFailedCount += ;
        }

        private void OnLoginSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var user = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                AuditService.Write(identityArgs.PerformingUser, $"User \"{user.Name}\" <{user.Email}>", identityArgs.IpAddress, DateTime.Now, 0, null, "user", "login success");
            }
        }

        private void OnLogoutSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var user = ApplicationContext.Current.Services.UserService.GetUserById(identityArgs.PerformingUser);
                AuditService.Write(identityArgs.PerformingUser, $"User \"{user.Name}\" <{user.Email}>", identityArgs.IpAddress, DateTime.Now, 0, null, "user", "logout success");
            }
        }
    }
}
