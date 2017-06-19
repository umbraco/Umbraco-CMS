using System;
using System.Web.Security;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.UI;
using MembershipProviderExtensions = Umbraco.Core.Security.MembershipProviderExtensions;

namespace umbraco
{
    public class userTasks : LegacyDialogTask
    {
        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return Constants.Applications.Users.ToString(); }
        }

        public override bool PerformSave()
        {
            // Hot damn HACK > user is allways UserType with id  = 1  = administrator ???
            // temp password deleted by NH
            //BusinessLogic.User.MakeNew(Alias, Alias, "", BusinessLogic.UserType.GetUserType(1));
            //return true;

            var provider = MembershipProviderExtensions.GetUsersMembershipProvider();

            var status = MembershipCreateStatus.ProviderError;
            try
            {
                // Password is auto-generated. They are they required to change the password by editing the user information.

                var password = Membership.GeneratePassword(
                    provider.MinRequiredPasswordLength,
                    provider.MinRequiredNonAlphanumericCharacters);

                var parts = Alias.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    return false;
                }
                var login = parts[0];
                var email = parts[1];

                var u = provider.CreateUser(
                    login, password, email.Trim().ToLower(), "", "", true, null, out status);

                if (u == null)
                {
                    return false;
                }

                _returnUrl = string.Format("users/EditUser.aspx?id={0}", u.ProviderUserKey);

                return status == MembershipCreateStatus.Success;
            }
            catch (Exception ex)
            {
                Current.Logger.Error<userTasks>(string.Format("Failed to create the user. Error from provider: {0}", status.ToString()), ex);
                return false;
            }
        }

        public override bool PerformDelete()
        {
            var u = Current.Services.UserService.GetUserById(ParentID);
            u.IsApproved = false;
            Current.Services.UserService.Save(u);
            return true;
        }
    }
}
