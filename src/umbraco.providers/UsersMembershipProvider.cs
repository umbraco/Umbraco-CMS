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
using Umbraco.Core.Logging;

#endregion

namespace umbraco.providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Users (User authentication for Umbraco Backend CMS)  
    /// </summary>
    [Obsolete("This has been superceded by Umbraco.Web.Security.Providers.UsersMembershipProvider")]
    public class UsersMembershipProvider : MembershipProviderBase, IUsersMembershipProvider
    {
        
        /// <summary>
        /// Override to maintain backwards compatibility with 0 required non-alphanumeric chars
        /// </summary>
        public override int DefaultMinNonAlphanumericChars
        {
            get { return 0; }
        }

        /// <summary>
        /// Override to maintain backwards compatibility with only 4 required length
        /// </summary>
        public override int DefaultMinPasswordLength
        {
            get { return 4; }
        }

        /// <summary>
        /// Override to maintain backwards compatibility
        /// </summary>
        public override bool DefaultUseLegacyEncoding
        {
            get { return true; }
        }

        /// <summary>
        /// For backwards compatibility, this provider supports this option
        /// </summary>
        public override bool AllowManuallyChangingPassword
        {
            get { return true; }
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) 
        {
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(name)) name = UmbracoSettings.DefaultBackofficeProvider;

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
        protected override bool PerformChangePassword(string username, string oldPassword, string newPassword)
        {
            //NOTE: due to backwards compatibilty reasons (and UX reasons), this provider doesn't care about the old password and 
            // allows simply setting the password manually so we don't really care about the old password.
            // This is allowed based on the overridden AllowManuallyChangingPassword option.

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
        protected override bool PerformChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
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
        protected override MembershipUser PerformCreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {

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

                    User.MakeNew(username, username, FormatPasswordForStorage(encodedPass, salt), email, ut);

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
            var fromDate = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
            return Log.Instance.GetLogItems(LogTypes.Login, fromDate).Count;
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
        protected override string PerformGetPassword(string username, string answer)
        {
            var found = User.GetAllByLoginName(username, false).ToArray();
            if (found == null || found.Any() == false)
            {
                throw new MembershipPasswordException("The supplied user is not found");
            }

            // check if user is locked out
            if (found.First().NoConsole)
            {
                throw new MembershipPasswordException("The supplied user is locked out");
            }

            if (RequiresQuestionAndAnswer)
            {
                throw new NotImplementedException("Question/answer is not supported with this membership provider");
            }

            var decodedPassword = DecryptPassword(found.First().GetPassword());

            return decodedPassword;
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
            if (userId == -1)
            {
                return null;
            }

            try
            {
                var user = new User(userId);

                //We need to log this since it's the only way we can determine the number of users online
                Log.Add(LogTypes.Login, user, -1, "User " + username + " has logged in");

                return (userId != -1) ? ConvertToMembershipUser(user) : null;
            }
            catch (Exception)
            {
                return null;
            }            
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
            var user = new User(Convert.ToInt32(providerUserKey));
            //We need to log this since it's the only way we can determine the number of users online
            Log.Add(LogTypes.Login, user, -1, "User " + user.LoginName + " has logged in");
            return ConvertToMembershipUser(user);
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
            var found = User.getAllByEmail(email.Trim().ToLower(), true);
            if (found == null || found.Any() == false)
            {
                return null;
            }
            return found.First().LoginName;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>The new password for the specified user.</returns>
        protected override string PerformResetPassword(string username, string answer, string generatedPassword)
        {            
            //TODO: This should be here - but how do we update failure count in this provider??
            //if (answer == null && RequiresQuestionAndAnswer)
            //{
            //    UpdateFailureCount(username, "passwordAnswer");

            //    throw new ProviderException("Password answer required for password reset.");
            //}

            var found = User.GetAllByLoginName(username, false).ToArray();
            if (found == null || found.Any() == false)
                throw new MembershipPasswordException("The supplied user is not found");

            var user = found.First();

            //Yes, it's true, this actually makes a db call to set the password
            string salt;
            var encPass = EncryptOrHashNewPassword(generatedPassword, out salt);
            user.Password = FormatPasswordForStorage(encPass, salt);
            //call this just for fun.
            user.Save();

            return generatedPassword;
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
                        NoConsole = false
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
            var found = User.GetAllByLoginName(user.UserName, false).ToArray();
            if (found == null || found.Any() == false)
            {
                throw new ProviderException("The supplied user is not found");
            }

            var m = found.First();
            if (RequiresUniqueEmail && user.Email.Trim().IsNullOrWhiteSpace() == false)
            {
                var byEmail = User.getAllByEmail(user.Email, true);
                if (byEmail.Count(x => x.Id != m.Id) > 0)
                {
                    throw new ProviderException(string.Format("A member with the email '{0}' already exists", user.Email));
                }
            }
            
            var typedUser = user as UsersMembershipUser;
            if (typedUser == null)
            {
                // update approve status            
                // update lock status
                // TODO: Update last lockout time            
                // TODO: update comment
                User.Update(m.Id, user.Email, user.IsApproved == false, user.IsLockedOut);
            }
            else
            {
                //This keeps compatibility - even though this logic to update name and user type  should not exist here
                User.Update(m.Id, typedUser.FullName.Trim(), typedUser.UserName, typedUser.Email, user.IsApproved == false, user.IsLockedOut, typedUser.UserType);
            }
            
            m.Save();
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
            var userId = User.getUserId(username);
            if (userId != -1)
            {
                var user = User.GetUser(userId);
                if (user != null)
                {
                    if (user.Disabled)
                    {
                        LogHelper.Info<UsersMembershipProvider>(
                            string.Format(
                                "Login attempt failed for username {0} from IP address {1}, the user is locked",
                                username,
                                GetCurrentRequestIpAddress()));

                        return false;
                    }

                    var result = CheckPassword(password, user.Password);
                    if (result == false)
                    {
                        LogHelper.Info<UsersMembershipProvider>(
                            string.Format(
                                "Login attempt failed for username {0} from IP address {1}",
                                username,
                                GetCurrentRequestIpAddress()));
                    }
                    else
                    {
                        LogHelper.Info<UsersMembershipProvider>(
                        string.Format(
                            "Login attempt succeeded for username {0} from IP address {1}",
                            username,
                            GetCurrentRequestIpAddress()));
                    }
                    return result;
                }
            }
            return false;
        }
        #endregion

        #region Helper Methods
       

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
