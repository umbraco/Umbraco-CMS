using System;
using System.Data;
using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class userTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

        public int UserId
        {
            set { _userID = value; }
        }
        public int TypeID
        {
            set { _typeID = value; }
            get { return _typeID; }
        }


        public string Alias
        {
            set { _alias = value; }
            get { return _alias; }
        }

        public int ParentID
        {
            set { _parentID = value; }
            get { return _parentID; }
        }

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

            MembershipCreateStatus status = MembershipCreateStatus.ProviderError;
            try
            {
                // Password is auto-generated. They are they required to change the password by editing the user information.
                MembershipUser u = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].CreateUser(Alias,
                    Membership.GeneratePassword(
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].MinRequiredPasswordLength,
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].MinRequiredNonAlphanumericCharacters),
                    "", "", "", true, null, out status);

                _returnUrl = string.Format("users/EditUser.aspx?id={0}", u.ProviderUserKey.ToString());

                return status == MembershipCreateStatus.Success;
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, ParentID, String.Format("Failed to create the user. Error from provider: {0}", status.ToString()));
                Log.Add(LogTypes.Debug, ParentID, ex.Message);
                return false;
            }
        }

        public bool Delete()
        {
            BusinessLogic.User u = BusinessLogic.User.GetUser(ParentID);
            u.disable();
            return true;
        }

        public userTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }
}
