#region namespace
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using System.Configuration;
using umbraco.BusinessLogic;
using System.Security.Cryptography;
using System.Web.Util;
using System.Collections.Specialized;
using System.Configuration.Provider;
#endregion

namespace umbraco.providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Users (User authentication for Umbraco Backend CMS)  
    /// </summary>
    public class UsersMembershipProvider : MembershipProvider
    {
        #region Fields
        private string _ApplicationName;
        private bool _EnablePasswordReset;
        private bool _EnablePasswordRetrieval;
        private int _MaxInvalidPasswordAttempts;
        private int _MinRequiredNonAlphanumericCharacters;
        private int _MinRequiredPasswordLength;
        private int _PasswordAttemptWindow;
        private MembershipPasswordFormat _PasswordFormat;
        private string _PasswordStrengthRegularExpression;
        private bool _RequiresQuestionAndAnswer;
        private bool _RequiresUniqueEmail;        
        #endregion

        #region Properties
        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get
            {
                return _ApplicationName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))                
                    throw new ProviderException("ApplicationName cannot be empty.");
                
                if (value.Length > 0x100)                
                    throw new ProviderException("Provider application name too long.");
                
                _ApplicationName = value;
            }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get { return _EnablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval
        {
            get { return _EnablePasswordRetrieval; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return _MaxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _MinRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength
        {
            get { return _MinRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow
        {
            get { return _PasswordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"></see> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _PasswordFormat; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <value></value>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return _PasswordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <value></value>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return _RequiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail
        {
            get { return _RequiresUniqueEmail; }
        }

        #endregion

        #region Initialization Method
        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call 
        /// <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider 
        /// has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Intialize values from web.config
            if (config == null) throw new ArgumentNullException("config");
            
            if (string.IsNullOrEmpty(name)) name = "UsersMembershipProvider";            
            // Initialize base provider class
            base.Initialize(name, config);

            this._EnablePasswordRetrieval = SecUtility.GetBooleanValue(config, "enablePasswordRetrieval", false);
            this._EnablePasswordReset = SecUtility.GetBooleanValue(config, "enablePasswordReset", false);
            this._RequiresQuestionAndAnswer = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", false);
            this._RequiresUniqueEmail = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", false);
            this._MaxInvalidPasswordAttempts = SecUtility.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
            this._PasswordAttemptWindow = SecUtility.GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            this._MinRequiredPasswordLength = SecUtility.GetIntValue(config, "minRequiredPasswordLength", 7, true, 0x80);
            this._MinRequiredNonAlphanumericCharacters= SecUtility.GetIntValue(config, "minRequiredNonalphanumericCharacters", 1, true, 0x80);
            this._PasswordStrengthRegularExpression = config["passwordStrengthRegularExpression"];

            this._ApplicationName = config["applicationName"];
            if (string.IsNullOrEmpty(this._ApplicationName))            
                this._ApplicationName = SecUtility.GetDefaultAppName();

            // make sure password format is clear by default.
            string str = config["passwordFormat"];
            if (str == null) str = "Clear";
            
            switch (str.ToLower())
            {
                case "clear":
                    this._PasswordFormat = MembershipPasswordFormat.Clear;
                    break;

                case "encrypted":
                    this._PasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;

                case "hashed":
                    this._PasswordFormat = MembershipPasswordFormat.Hashed;
                    break;

                default:
                    throw new ProviderException("Provider bad password format");
            }

            if ((this.PasswordFormat == MembershipPasswordFormat.Hashed) && this.EnablePasswordRetrieval)            
                throw new ProviderException("Provider can not retrieve hashed password");
            
        }
        #endregion

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
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!User.validateCredentials(username, oldPassword))            
                return false;

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");

            User user = new User(username);
            string encodedPassword = EncodePassword(newPassword);            
            user.Password = encodedPassword;            
            return (user.ValidatePassword(encodedPassword)) ? true : false;
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
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Does umbraco allow duplicate emails??
            //if (RequiresUniqueEmail && !string.IsNullOrEmpty(GetUserNameByEmail(email)))
            //{
            //    status = MembershipCreateStatus.DuplicateEmail;
            //    return null;
            //}

            UsersMembershipUser u = GetUser(username, false) as UsersMembershipUser;
            if (u == null)
            {
                try
                {
                    // Get the usertype of the current user
                    BusinessLogic.UserType ut = BusinessLogic.UserType.GetUserType(1);
                    if (umbraco.BasePages.UmbracoEnsuredPage.CurrentUser != null)
                    {
                        ut = umbraco.BasePages.UmbracoEnsuredPage.CurrentUser.UserType;
                    }
                    User.MakeNew(username, username, password, ut);
                    status = MembershipCreateStatus.Success;
                } 
                catch(Exception)
                {
                    status = MembershipCreateStatus.ProviderError;
                }
                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }
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
            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;
            MembershipUserCollection usersList = new MembershipUserCollection();
            User[] usersArray = User.getAllByEmail(emailToMatch);
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
            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;
            MembershipUserCollection usersList = new MembershipUserCollection();
            User[] usersArray = User.getAllByLoginName(usernameToMatch);
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
            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;
            MembershipUserCollection usersList = new MembershipUserCollection();
            User[] usersArray = User.getAll();
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
            int userId = User.getUserId(username);
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
            
            int userId = User.getUserId(username);
            return (userId == -1) ? null : Membership.GeneratePassword(
                    MinRequiredPasswordLength, 
                    MinRequiredNonAlphanumericCharacters);   
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
                User user = new User(userName);
                user.Disabled = false;
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
            UsersMembershipUser umbracoUser = user as UsersMembershipUser;
            int userID = 0;

            if (int.TryParse(umbracoUser.ProviderUserKey.ToString(), out userID))
            {
                try
                {                                        
                    User.Update(userID, umbracoUser.FullName, umbracoUser.UserName, umbracoUser.Email, umbracoUser.UserType);
                }
                catch (Exception)
                {
                    throw new ProviderException("User cannot be updated.");
                }
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
                User user = new User(username);
                if (user != null && user.Id != -1)
                {
                    if (user.Disabled) return false;
                    else return user.ValidatePassword(EncodePassword(password));
                }
            }
            catch
            {
                // nothing to catch here - move on
            }

            return false;
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
                    pass2 = UnEncodePassword(dbPassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
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
        public string EncodePassword(string password)
        {           
            string encodedPassword = password;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = Encoding.Unicode.GetBytes(password);
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return encodedPassword;
        }

        /// <summary>
        /// Unencode password.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns>The unencoded password.</returns>
        public string UnEncodePassword(string encodedPassword)
        {           
            string password = encodedPassword;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password = Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return password;
        }
        
        /// <summary>
        /// Converts to membership user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private UsersMembershipUser ConvertToMembershipUser(User user)
        {
            if (user == null) return null;
            else
            {                
                return new UsersMembershipUser(base.Name, user.LoginName, user.Id, user.Email,
                       string.Empty, string.Empty, true, user.Disabled,
                       DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now,
                       DateTime.Now, user.Name, user.Language, user.UserType);
            }
        }
        #endregion
    }
}
