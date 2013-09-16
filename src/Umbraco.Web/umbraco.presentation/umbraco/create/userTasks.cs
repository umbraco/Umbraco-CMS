using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

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
            get { return DefaultApps.users.ToString(); }
        }

        public override bool PerformSave()
        {
            // Hot damn HACK > user is allways UserType with id  = 1  = administrator ???
            // temp password deleted by NH
            //BusinessLogic.User.MakeNew(Alias, Alias, "", BusinessLogic.UserType.GetUserType(1));
            //return true;

            var status = MembershipCreateStatus.ProviderError;
            try
            {
                // Password is auto-generated. They are they required to change the password by editing the user information.
                var u = Membership.Providers[UmbracoConfiguration.Current.UmbracoSettings.Providers.Users.DefaultBackOfficeProvider].CreateUser(Alias,
                    Membership.GeneratePassword(
                    Membership.Providers[UmbracoConfiguration.Current.UmbracoSettings.Providers.Users.DefaultBackOfficeProvider].MinRequiredPasswordLength,
                    Membership.Providers[UmbracoConfiguration.Current.UmbracoSettings.Providers.Users.DefaultBackOfficeProvider].MinRequiredNonAlphanumericCharacters),
                    "", "", "", true, null, out status);

                _returnUrl = string.Format("users/EditUser.aspx?id={0}", u.ProviderUserKey);

                return status == MembershipCreateStatus.Success;
            }
            catch (Exception ex)
            {
                LogHelper.Error<userTasks>(string.Format("Failed to create the user. Error from provider: {0}", status.ToString()), ex);                
                return false;
            }
        }

        public override bool PerformDelete()
        {
            var u = User.GetUser(ParentID);
            u.disable();
            return true;
        }
    }
}
