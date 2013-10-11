using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration.Provider;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)  
    /// </summary>
    internal class MembersMembershipProvider : MembershipProvider
    {
        #region Fields
        private string _applicationName;
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private int _maxInvalidPasswordAttempts;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private int _passwordAttemptWindow;

        private MembershipPasswordFormat _passwordFormat;

        private string _passwordStrengthRegularExpression;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;

        #endregion

        private IMembershipMemberService _memberService;

        protected IMembershipMemberService MemberService
        {
            get { return _memberService ?? (_memberService = ApplicationContext.Current.Services.MemberService); }
        }

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get { return _applicationName;  }
            set { _applicationName = value; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>        
        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>        
        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>        
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum length required for a password. </returns>        
        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>        
        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"></see> values indicating the format for storing passwords in the data store.</returns>        
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <value></value>
        /// <returns>A regular expression used to evaluate a password.</returns>        
        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <value></value>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>        
        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>        
        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        /// <summary>
        /// The default Umbraco member type alias to create when registration is performed using .net Membership
        /// </summary>
        /// <value></value>
        /// <returns>Member type alias</returns>        
        public string DefaultMemberTypeAlias { get; private set; }
        
        public string ProviderName {
            get { return "MembersMembershipProvider"; }
        }
        
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
            if (config == null) {throw new ArgumentNullException("config");}

            if (string.IsNullOrEmpty(name)) name = ProviderName;

            // Initialize base provider class
            base.Initialize(name, config);

            _applicationName = string.IsNullOrEmpty(config["applicationName"]) ? GetDefaultAppName() : config["applicationName"];

            _enablePasswordRetrieval = GetBooleanValue(config, "enablePasswordRetrieval", false);
            _enablePasswordReset = GetBooleanValue(config, "enablePasswordReset", false);
            _requiresQuestionAndAnswer = GetBooleanValue(config, "requiresQuestionAndAnswer", false);
            _requiresUniqueEmail = GetBooleanValue(config, "requiresUniqueEmail", true);
            _maxInvalidPasswordAttempts = GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
            _passwordAttemptWindow = GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            _minRequiredPasswordLength = GetIntValue(config, "minRequiredPasswordLength", 7, true, 0x80);
            _minRequiredNonAlphanumericCharacters = GetIntValue(config, "minRequiredNonalphanumericCharacters", 1, true, 0x80);
            _passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];

            // make sure password format is Hashed by default.
            var str = config["passwordFormat"] ?? "Hashed";

            LogHelper.Debug<MembersMembershipProvider>("Loaded membership provider properties");
            LogHelper.Debug<MembersMembershipProvider>(ToString());

            switch (str.ToLower())
            {
                case "clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                case "encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                default:
                    var e = new ProviderException("Provider bad password format");
                    LogHelper.Error<MembersMembershipProvider>(e.Message, e);
                    throw e;
            }

            if ((PasswordFormat == MembershipPasswordFormat.Hashed) && EnablePasswordRetrieval)
            {
                var e = new ProviderException("Provider can not retrieve hashed password");
                LogHelper.Error<MembersMembershipProvider>(e.Message, e);
                throw e;
            }

            // TODO: rationalise what happens when no member alias is specified....
            DefaultMemberTypeAlias = config["defaultMemberTypeAlias"];
            
            LogHelper.Debug<MembersMembershipProvider>("Finished initialising member ship provider " + GetType().FullName);
        }

        public override string ToString()
        {
            var result = base.ToString();

            result +=  "_applicationName =" + _applicationName + Environment.NewLine;
            result += "_enablePasswordReset=" + _enablePasswordReset + Environment.NewLine;
            result += "_enablePasswordRetrieval=" + _enablePasswordRetrieval + Environment.NewLine;
            result += "_maxInvalidPasswordAttempts=" + _maxInvalidPasswordAttempts + Environment.NewLine;
            result += "_minRequiredNonAlphanumericCharacters=" + _minRequiredNonAlphanumericCharacters + Environment.NewLine;
            result += "_minRequiredPasswordLength=" + _minRequiredPasswordLength + Environment.NewLine;
            result += "_passwordAttemptWindow=" + _passwordAttemptWindow + Environment.NewLine;

            result += "_passwordFormat=" + _passwordFormat + Environment.NewLine;

            result += "_passwordStrengthRegularExpression=" + _passwordStrengthRegularExpression + Environment.NewLine;
            result += "_requiresQuestionAndAnswer=" + _requiresQuestionAndAnswer + Environment.NewLine;
            result += "_requiresUniqueEmail=" + _requiresUniqueEmail + Environment.NewLine;
            result += "DefaultMemberTypeAlias=" + DefaultMemberTypeAlias + Environment.NewLine;

            return result;
        }

        /// <summary>
        /// Adds a new membership user to the data source with the specified member type
        /// </summary>
        /// <param name="memberType">A specific member type to create the member for</param>
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
        public MembershipUser CreateUser(string memberType, string username, string password, string email, string passwordQuestion, string passwordAnswer,
                                                  bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            LogHelper.Debug<MembersMembershipProvider>("Member signup requested: username -> " + username + ". email -> " + email);

            // Validate password
            if (IsPasswordValid(password) == false)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Validate email
            if (IsEmaiValid(email) == false)
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            // Make sure username isn't all whitespace
            if (string.IsNullOrWhiteSpace(username.Trim()))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            // Check password question
            if (string.IsNullOrWhiteSpace(passwordQuestion) && _requiresQuestionAndAnswer)
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            // Check password answer
            if (string.IsNullOrWhiteSpace(passwordAnswer) && _requiresQuestionAndAnswer)
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            // See if the user already exists
            if (MemberService.GetByUsername(username) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                LogHelper.Warn<MembersMembershipProvider>("Cannot create member as username already exists: " + username);
                return null;
            }

            // See if the email is unique
            if (MemberService.GetByEmail(email) != null && RequiresUniqueEmail)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                LogHelper.Warn<MembersMembershipProvider>(
                    "Cannot create member as a member with the same email address exists: " + email);
                return null;
            }

            var member = MemberService.CreateMember(email, username, password, memberType);

            member.IsApproved = isApproved;
            member.PasswordQuestion = passwordQuestion;
            member.PasswordAnswer = passwordAnswer;

            //encrypts/hashes the password depending on the settings
            member.Password = EncryptOrHashPassword(member.Password);

            MemberService.Save(member);

            status = MembershipCreateStatus.Success;
            return member.AsConcreteMembershipUser();
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
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
                                                  bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            return CreateUser(DefaultMemberTypeAlias, username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
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
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
                                                             string newPasswordAnswer)
        {
            if (_requiresQuestionAndAnswer == false)
            {
                throw new NotSupportedException("Updating the password Question and Answer is not available if requiresQuestionAndAnswer is not set in web.config");
            }

            if (ValidateUser(username, password) == false)
            {
                throw new MembershipPasswordException("Invalid username and password combinatio");
            }

            var member = MemberService.GetByUsername(username);
            var encodedPassword = EncryptOrHashPassword(password);

            if (member.Password == encodedPassword)
            {
                member.PasswordQuestion = newPasswordQuestion;
                member.PasswordAnswer = newPasswordAnswer;

                MemberService.Save(member);

                return true;
            }
            else
            {
                //TODO: Throw here? or just return false;
            }

            return false;
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        public override string GetPassword(string username, string answer)
        {
            if (_enablePasswordRetrieval == false)
                throw new ProviderException("Password Retrieval Not Enabled.");

            if (_passwordFormat == MembershipPasswordFormat.Hashed)
                throw new ProviderException("Cannot retrieve Hashed passwords.");

            var member = MemberService.GetByUsername(username);

            if (_requiresQuestionAndAnswer && member.PasswordAnswer != answer)
            {
                throw new ProviderException("Password retrieval answer doesn't match");
            } 

            return member.Password;
        }
        
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
            // Validate new password
            if (IsPasswordValid(newPassword) == false)
            {
                var e = new MembershipPasswordException("Change password canceled due to new password validation failure.");
                LogHelper.WarnWithException<MembersMembershipProvider>(e.Message, e);
                throw e;
            }

            var member = MemberService.GetByUsername(username);
            if (member == null) return false;
            
            var encodedPassword = EncryptOrHashPassword(oldPassword);

            if (member.Password == encodedPassword)
            {

                member.Password = EncryptOrHashPassword(newPassword);
                MemberService.Save(member);

                return true;
            }

            LogHelper.Warn<MembersMembershipProvider>("Can't change password as old password was incorrect");
            return false;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user (not used with Umbraco).</param>
        /// <returns>The new password for the specified user.</returns>
        public override string ResetPassword(string username, string answer)
        {
            if (_enablePasswordReset == false)
                throw new ProviderException("Password reset is Not Enabled.");

            var member = MemberService.GetByUsername(username);

            if (member == null)
                throw new ProviderException("The supplied user is not found");

            if(member.IsLockedOut)
                throw new ProviderException("The member is locked out.");

            if (_requiresQuestionAndAnswer == false || (_requiresQuestionAndAnswer && answer == member.PasswordAnswer))
            {
                member.Password =
                    EncryptOrHashPassword(Membership.GeneratePassword(_minRequiredPasswordLength,
                                                               _minRequiredNonAlphanumericCharacters));
                MemberService.Save(member);
            }
            else
            {
                throw new MembershipPasswordException("Incorrect password answer");
            }
            
            return null;
        }
        
        /// <summary>
        /// Updates e-mail and potentially approved status, lock status and comment on a user.
        /// Note: To automatically support lock, approve and comments you'll need to add references to the membertype properties in the 
        /// 'Member' element in web.config by adding their aliases to the 'umbracoApprovePropertyTypeAlias', 'umbracoLockPropertyTypeAlias' and 'umbracoCommentPropertyTypeAlias' attributes
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>      
        public override void UpdateUser(MembershipUser user)
        {
            var member = user.AsIMember();
            MemberService.Save(member);
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
            var member = MemberService.GetByUsername(username);

            if (member == null || member.IsApproved == false) return false;

            if (member.IsLockedOut)
                throw new ProviderException("The member is locked out.");

            var encodedPassword = EncryptOrHashPassword(password);
            
            var authenticated = (encodedPassword == member.Password);

            if (authenticated == false)
            {
                // TODO: Increment login attempts - lock if too many.

                //var count = member.GetValue<int>("loginAttempts");
                //count++;

                //if (count >= _maxInvalidPasswordAttempts)
                //{
                //    member.SetValue("loginAttempts", 0);
                //    member.IsLockedOut = true;
                //    throw new ProviderException("The member " + member.Username + " is locked out.");
                //}
                //else
                //{
                //    member.SetValue("loginAttempts", count);
                //}
            }
            else
            {
                // add this later - member.SetValue("loginAttempts", 0);
                member.LastLoginDate = DateTime.Now;
            }

            MemberService.Save(member);
            return authenticated;
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="username">The membership user to clear the lock status for.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        public override bool UnlockUser(string username)
        {
            var member = MemberService.GetByUsername(username);

            if(member == null)
                throw new ProviderException("Cannot find member " + username);

            // Non need to update
            if (member.IsLockedOut == false) return true;

            member.IsLockedOut = false;
            // TODO: add this later - member.SetValue("loginAttempts", 0);

            MemberService.Save(member);
            
            return true;
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
            var member = MemberService.GetById(providerUserKey);

            if (userIsOnline)
            {
                member.UpdateDate = DateTime.Now;
                MemberService.Save(member);
            }

            return member.AsConcreteMembershipUser();
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
            var member = MemberService.GetByUsername(username);

            if (userIsOnline)
            {
                member.UpdateDate = DateTime.Now;
                MemberService.Save(member);
            }

            return member.AsConcreteMembershipUser();
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
            var member = MemberService.GetByEmail(email);

            return member == null ? null : member.Username;
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
            var member = MemberService.GetByUsername(username);
            if (member == null) return false;

            MemberService.Delete(member);
            return true;
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
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
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
            var byEmail = MemberService.FindMembersByEmail(emailToMatch).ToArray();
            totalRecords = byEmail.Length;
            var pagedResult = new PagedResult<IMember>(totalRecords, pageIndex, pageSize);

            var collection = new MembershipUserCollection();
            foreach (var m in byEmail.Skip(pagedResult.SkipSize).Take(pageSize))
            {
                collection.Add(m.AsConcreteMembershipUser());
            }
            return collection;
        }

        #region Private methods

        private bool IsPasswordValid(string password)
        {
            if (_minRequiredNonAlphanumericCharacters > 0)
            {
                var nonAlphaNumeric = Regex.Replace(password, "[a-zA-Z0-9]", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (nonAlphaNumeric.Length < _minRequiredNonAlphanumericCharacters)
                {
                    return false;
                }
            }

            var valid = true;
            if(string.IsNullOrEmpty(_passwordStrengthRegularExpression) == false)
            {
                valid = Regex.IsMatch(password, _passwordStrengthRegularExpression, RegexOptions.Compiled);
            }

            return valid && password.Length >= _minRequiredPasswordLength;
        }

        private bool IsEmaiValid(string email)
        {
            var validator = new EmailAddressAttribute();
            
            return validator.IsValid(email);
        }
        
        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encoded password.</returns>
        private string EncryptOrHashPassword(string password)
        {
            var encodedPassword = password;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    var hash = new HMACSHA1 {Key = Encoding.Unicode.GetBytes(password)};
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return encodedPassword;
        }

        /// <summary>
        /// Gets the boolean value.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        private bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
        {
            bool flag;
            var str = config[valueName];
            if (str == null)
                return defaultValue;

            if (bool.TryParse(str, out flag) == false)
            {
                throw new ProviderException("Value must be boolean.");
            }
            return flag;
        }

        /// <summary>
        /// Gets the int value.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="zeroAllowed">if set to <c>true</c> [zero allowed].</param>
        /// <param name="maxValueAllowed">The max value allowed.</param>
        /// <returns></returns>
        private int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            int num;
            var s = config[valueName];
            if (s == null)
            {
                return defaultValue;
            }
            if (int.TryParse(s, out num) == false)
            {
                if (zeroAllowed)
                {
                    throw new ProviderException("Value must be non negative integer");
                }
                throw new ProviderException("Value must be positive integer");
            }
            if (zeroAllowed && (num < 0))
            {
                throw new ProviderException("Value must be non negativeinteger");
            }
            if (zeroAllowed == false && (num <= 0))
            {
                throw new ProviderException("Value must be positive integer");
            }
            if ((maxValueAllowed > 0) && (num > maxValueAllowed))
            {
                throw new ProviderException("Value too big");
            }
            return num;
        }


        /// <summary>
        /// Gets the name of the default app.
        /// </summary>
        /// <returns></returns>
        private string GetDefaultAppName()
        {
            try
            {
                var applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                return string.IsNullOrEmpty(applicationVirtualPath) ? "/" : applicationVirtualPath;
            }
            catch
            {
                return "/";
            }
        }

        #endregion
    }
}