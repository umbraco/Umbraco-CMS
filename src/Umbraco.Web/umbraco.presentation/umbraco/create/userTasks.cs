using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class userTasks : interfaces.ITaskReturnUrl
    {
        private string _returnUrl = "";

        public int UserId
        {
            set { }
        }

        public int TypeID { get; set; }


        public string Alias { get; set; }

        public int ParentID { get; set; }

        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public bool Save()
        {
            // Hot damn HACK > user is allways UserType with id  = 1  = administrator ???
            // temp password deleted by NH
            //BusinessLogic.User.MakeNew(Alias, Alias, "", BusinessLogic.UserType.GetUserType(1));
            //return true;

            var status = MembershipCreateStatus.ProviderError;
            try
            {
                // Password is auto-generated. They are they required to change the password by editing the user information.
                var u = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].CreateUser(Alias,
                    Membership.GeneratePassword(
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].MinRequiredPasswordLength,
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].MinRequiredNonAlphanumericCharacters),
                    "", "", "", true, null, out status);

                _returnUrl = string.Format("users/EditUser.aspx?id={0}", u.ProviderUserKey.ToString());

                return status == MembershipCreateStatus.Success;
            }
            catch (Exception ex)
            {
                LogHelper.Error<userTasks>(string.Format("Failed to create the user. Error from provider: {0}", status.ToString()), ex);                
                return false;
            }
        }

        public bool Delete()
        {
            var u = User.GetUser(ParentID);
            u.disable();
            return true;
        }
    }
}
