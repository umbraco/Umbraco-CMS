#region namespace
using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Security;
using umbraco.BusinessLogic;
using System.Web.Util;
using System.Configuration.Provider;
using System.Linq;
#endregion

namespace umbraco.providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Users (User authentication for Umbraco Backend CMS)  
    /// </summary>
    public class UsersMembershipProvider : MembershipProviderBase
    {
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(name)) name = "UsersMembershipProvider";

            base.Initialize(name, config);
        }

        #region Methods

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        /// During installation the application will not be configured, if this is the case and the 'default' password 
        /// is stored in the database then we will validate the user - this will allow for an admin password reset if required
        /// </remarks>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (ApplicationContext.Current.IsConfigured == false && oldPassword == "default"
                || ValidateUser(username, oldPassword))
            {
                var args = new ValidatePasswordEventArgs(username, newPassword, false);
                OnValidatingPassword(args);

                if (args.Cancel)
                {
                    if (args.FailureInformation != null)
                        throw args.FailureInformation;
                    throw new MembershipPasswordException("Change password canceled due to password validation failure.");
                }

                var user = new User(username);
                //encrypt/hash the new one
                string salt;
                var encodedPassword = EncryptOrHashNewPassword(newPassword, out salt);

                //Yes, it's true, this actually makes a db call to set the password
                user.Password = FormatPasswordForStorage(encodedPassword, salt);
                //call this just for fun.
                user.Save();

                return true;    
            }

            return false;

        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"></see> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the information for the newly created user.
        /// </returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // TODO: Does umbraco allow duplicate emails??
            //if (RequiresUniqueEmail && !string.IsNullOrEmpty(GetUserNameByEmail(email)))
            //{
            //    status = MembershipCreateStatus.DuplicateEmail;
            //    return null;
            //}

            var u = GetUser(username, false) as UsersMembershipUser;
            if (u == null)
            {
                try
                {
                    // Get the usertype of the current user
                    var ut = UserType.GetUserType(1);
                    if (BasePages.UmbracoEnsuredPage.CurrentUser != null)
                    {
                        ut = BasePages.UmbracoEnsuredPage.CurrentUser.UserType;
                    }

                    //ensure the password is encrypted/hashed
                    string salt;
                    var encodedPass = EncryptOrHashNewPassword(password, out salt);

                    User.MakeNew(username, username, FormatPasswordForStorage(encodedPass, salt), ut);

                    status = MembershipCreateStatus.Success;
                }
                catch (Exception)
                {
                    status = MembershipCreateStatus.ProviderError;
                }
                return GetUser(username, false);
            }

            status = MembershipCreateStatus.DuplicateUserName;
            return null;
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            try
            {
                User user = new User(username);
                user.delete();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var counter = 0;
            var startIndex = pageSize * pageIndex;
            var endIndex = startIndex + pageSize - 1;
            var usersList = new MembershipUserCollection();
            var usersArray = User.getAllByEmail(emailToMatch);
            totalRecords = usersArray.Length;

            foreach (var user in usersArray)
            {
                if (counter >= startIndex)
                    usersList.Add(ConvertToMembershipUser(user));
                if (counter >= endIndex) break;
                counter++;
            }
            return usersList;
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var counter = 0;
            var startIndex = pageSize * pageIndex;
            var endIndex = startIndex + pageSize - 1;
            var usersList = new MembershipUserCollection();
            var usersArray = User.GetAllByLoginName(usernameToMatch, true).ToArray();
            totalRecords = usersArray.Length;

            foreach (var user in usersArray)
            {
                if (counter >= startIndex)
                    usersList.Add(ConvertToMembershipUser(user));
                if (counter >= endIndex) break;
                counter++;
            }
            return usersList;
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var counter = 0;
            var startIndex = pageSize * pageIndex;
            var endIndex = startIndex + pageSize - 1;
            var usersList = new MembershipUserCollection();
            var usersArray = User.getAll();
            totalRecords = usersArray.Length;

            foreach (User user in usersArray)
            {
                if (counter >= startIndex)
                    usersList.Add(ConvertToMembershipUser(user));
                if (counter >= endIndex) break;
                counter++;
            }
            return usersList;
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source. This is - for security - not
        /// supported for Umbraco Users and an exception will be thrown
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        public override string GetPassword(string username, string answer)
        {
            throw new ProviderException("Password Retrieval Not Enabled.");
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var userId = User.getUserId(username);
            return (userId != -1) ? ConvertToMembershipUser(new User(userId)) : null;
        }

        /// <summary>
        /// Gets information from the data source for a user based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return ConvertToMembershipUser(new User(Convert.ToInt32(providerUserKey)));
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        public override string GetUserNameByEmail(string email)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>The new password for the specified user.</returns>
        public override string ResetPassword(string username, string answer)
        {
            if (EnablePasswordReset == false)
            {
                throw new NotSupportedException("Password reset is not supported");
            }

            //TODO: This should be here - but how do we update failure count in this provider??
            //if (answer == null && RequiresQuestionAndAnswer)
            //{
            //    UpdateFailureCount(username, "passwordAnswer");

            //    throw new ProviderException("Password answer required for password reset.");
            //}

            var newPassword = Membership.GeneratePassword(MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);

            var args = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                throw new MembershipPasswordException("Reset password canceled due to password validation failure.");
            }

            var found = User.GetAllByLoginName(username, false);
            if (found == null || found.Any() == false)
                throw new MembershipPasswordException("The supplied user is not found");

            var user = found.First();

            //Yes, it's true, this actually makes a db call to set the password
            string salt;
            var encPass = EncryptOrHashNewPassword(newPassword, out salt);
            user.Password = FormatPasswordForStorage(encPass, salt);
            //call this just for fun.
            user.Save();

            return newPassword;
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The membership user to clear the lock status for.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        public override bool UnlockUser(string userName)
        {
            try
            {
                var user = new User(userName)
                    {
                        Disabled = false
                    };
                user.Save();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser(MembershipUser user)
        {
            var umbracoUser = user as UsersMembershipUser;
            var userID = 0;

            if (int.TryParse(umbracoUser.ProviderUserKey.ToString(), out userID) == false) return;

            try
            {
                User.Update(userID, umbracoUser.FullName, umbracoUser.UserName, umbracoUser.Email, umbracoUser.UserType);
            }
            catch (Exception)
            {
                throw new ProviderException("User cannot be updated.");
            }
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        public override bool ValidateUser(string username, string password)
        {
            // we need to wrap this in a try/catch as passing a non existing 
            // user will throw an exception
            try
            {
                var user = new User(username);
                if (user.Id != -1)
                {
                    return user.Disabled == false && user.ValidatePassword(EncryptOrHashExistingPassword(password));
                }
                return false;
            }
            catch
            {
                //the user doesn't exist
                return false;
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="dbPassword">The dbPassword.</param>
        /// <returns></returns>
        internal bool CheckPassword(string password, string dbPassword)
        {
            string pass1 = password;
            string pass2 = dbPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = DecodePassword(dbPassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncryptOrHashExistingPassword(password);
                    break;
                default:
                    break;
            }
            return (pass1 == pass2) ? true : false;
        }


        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encoded password.</returns>
        [Obsolete("Do not use this, it is the legacy way to encode a password")]
        public string EncodePassword(string password)
        {
            return base.LegacyEncodePassword(password);
        }

        /// <summary>
        /// Unencode password.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns>The unencoded password.</returns>
        [Obsolete("Do not use this, it is the legacy way to decode a password")]
        public string UnEncodePassword(string encodedPassword)
        {
            return LegacyUnEncodePassword(encodedPassword);
        }

        /// <summary>
        /// Converts to membership user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private UsersMembershipUser ConvertToMembershipUser(User user)
        {
            if (user == null) return null;
            return new UsersMembershipUser(base.Name, user.LoginName, user.Id, user.Email,
                                           string.Empty, string.Empty, true, user.Disabled,
                                           DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now,
                                           DateTime.Now, user.Name, user.Language, user.UserType);
        }

        #endregion
    }
}
