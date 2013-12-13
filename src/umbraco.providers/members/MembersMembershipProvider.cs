#region namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using umbraco.BusinessLogic;
using System.Security.Cryptography;
using System.Web.Util;
using System.Collections.Specialized;
using System.Configuration.Provider;
using umbraco.cms.businesslogic;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using Member = umbraco.cms.businesslogic.member.Member;
using MemberType = umbraco.cms.businesslogic.member.MemberType;

#endregion

namespace umbraco.providers.members
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)  
    /// </summary>

    public class UmbracoMembershipProvider : MembershipProviderBase
    {
        #region Fields

        //Set the defaults!
        private string _defaultMemberTypeAlias = "Member";               
        private string _lockPropertyTypeAlias = Constants.Conventions.Member.IsLockedOut;
        private string _lastLockedOutPropertyTypeAlias = Constants.Conventions.Member.LastLockoutDate;
        private string _failedPasswordAttemptsPropertyTypeAlias = Constants.Conventions.Member.FailedPasswordAttempts;
        private string _approvedPropertyTypeAlias = Constants.Conventions.Member.IsApproved;
        private string _commentPropertyTypeAlias = Constants.Conventions.Member.Comments;
        private string _lastLoginPropertyTypeAlias = Constants.Conventions.Member.LastLoginDate;
        private string _lastPasswordChangedPropertyTypeAlias = Constants.Conventions.Member.LastPasswordChangeDate;
        private string _passwordRetrievalQuestionPropertyTypeAlias = Constants.Conventions.Member.PasswordQuestion;
        private string _passwordRetrievalAnswerPropertyTypeAlias = Constants.Conventions.Member.PasswordAnswer;
        
        private string _providerName = Member.UmbracoMemberProviderName;
        
        #endregion
        
        /// <summary>
        /// Override to maintain backwards compatibility with 0 required non-alphanumeric chars
        /// </summary>
        protected override int DefaultMinNonAlphanumericChars
        {
            get { return 0; }
        }

        /// <summary>
        /// Override to maintain backwards compatibility with only 4 required length
        /// </summary>
        protected override int DefaultMinPasswordLength
        {
            get { return 4; }
        }

        /// <summary>
        /// Override to maintain backwards compatibility
        /// </summary>
        protected override bool DefaultUseLegacyEncoding
        {
            get { return true; }
        }

        /// <summary>
        /// For backwards compatibility, this provider supports this option
        /// </summary>
        internal override bool AllowManuallyChangingPassword
        {
            get { return true; }
        }

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

            if (string.IsNullOrEmpty(name)) name = "UmbracoMembershipProvider";
            
            base.Initialize(name, config);
            
            _providerName = name;
            
            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultMemberTypeAlias"] != null)
                _defaultMemberTypeAlias = config["defaultMemberTypeAlias"];
            else if (MemberType.GetAll.Length == 1)
                _defaultMemberTypeAlias = MemberType.GetAll[0].Alias;
            else
                throw new ProviderException("No default MemberType alias is specified in the web.config string. Please add a 'defaultMemberTypeAlias' to the add element in the provider declaration in web.config");

            // test for approve status
            if (config["umbracoApprovePropertyTypeAlias"] != null)
            {
                _approvedPropertyTypeAlias = config["umbracoApprovePropertyTypeAlias"];
            }
            // test for lock attempts
            if (config["umbracoLockPropertyTypeAlias"] != null)
            {
                _lockPropertyTypeAlias = config["umbracoLockPropertyTypeAlias"];
            }
            if (config["umbracoLastLockedPropertyTypeAlias"] != null)
            {
                _lastLockedOutPropertyTypeAlias = config["umbracoLastLockedPropertyTypeAlias"];
            }
            if (config["umbracoLastPasswordChangedPropertyTypeAlias"] != null)
            {
                _lastPasswordChangedPropertyTypeAlias = config["umbracoLastPasswordChangedPropertyTypeAlias"];
            }
            if (config["umbracoFailedPasswordAttemptsPropertyTypeAlias"] != null)
            {
                _failedPasswordAttemptsPropertyTypeAlias = config["umbracoFailedPasswordAttemptsPropertyTypeAlias"];
            }
            // comment property
            if (config["umbracoCommentPropertyTypeAlias"] != null)
            {
                _commentPropertyTypeAlias = config["umbracoCommentPropertyTypeAlias"];
            }
            // last login date
            if (config["umbracoLastLoginPropertyTypeAlias"] != null)
            {
                _lastLoginPropertyTypeAlias = config["umbracoLastLoginPropertyTypeAlias"];
            }
            // password retrieval
            if (config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"] != null)
            {
                _passwordRetrievalQuestionPropertyTypeAlias = config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"];
            }
            if (config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"] != null)
            {
                _passwordRetrievalAnswerPropertyTypeAlias = config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"];
            }

        }
        #endregion

        #region Methods

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">This property is ignore for this provider</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        protected override bool PerformChangePassword(string username, string oldPassword, string newPassword)
        {
            //NOTE: due to backwards compatibilty reasons, this provider doesn't care about the old password and 
            // allows simply setting the password manually so we don't really care about the old password.
            // This is allowed based on the overridden AllowManuallyChangingPassword option.

            // in order to support updating passwords from the umbraco core, we can't validate the old password
            var m = Member.GetMemberFromLoginName(username);            
            if (m == null) return false;

            var args = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                throw new MembershipPasswordException("Change password canceled due to password validation failure.");
            }

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(newPassword, out salt);
            m.ChangePassword(
                FormatPasswordForStorage(encodedPassword, salt));

            UpdateMemberProperty(m, _lastPasswordChangedPropertyTypeAlias, DateTime.Now);

            m.Save();

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
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!String.IsNullOrEmpty(_passwordRetrievalQuestionPropertyTypeAlias) && !String.IsNullOrEmpty(_passwordRetrievalAnswerPropertyTypeAlias))
            {
                if (ValidateUser(username, password))
                {
                    Member m = Member.GetMemberFromLoginName(username);
                    if (m != null)
                    {
                        UpdateMemberProperty(m, _passwordRetrievalQuestionPropertyTypeAlias, newPasswordQuestion);
                        UpdateMemberProperty(m, _passwordRetrievalAnswerPropertyTypeAlias, newPasswordAnswer);
                        m.Save();
                        return true;
                    }
                    else
                    {
                        throw new MembershipPasswordException("The supplied user is not found!");
                    }
                }
                else {
                    throw new MembershipPasswordException("Invalid user/password combo");
                }

            }
            else
            {
                throw new NotSupportedException("Updating the password Question and Answer is not valid if the properties aren't set in the config file");
            }
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="memberTypeAlias"></param>
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
        public MembershipUser CreateUser(string memberTypeAlias, string username, string password, string email, string passwordQuestion,
                                                  string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {

            var args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (Member.GetMemberFromLoginName(username) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }
            else if (Member.GetMemberFromEmail(email) != null && RequiresUniqueEmail)
            {
                status = MembershipCreateStatus.DuplicateEmail;
            }
            else
            {
                var memberType = MemberType.GetByAlias(memberTypeAlias);
                if (memberType == null)
                {
                    throw new InvalidOperationException("Could not find a member type with alias " + memberTypeAlias + ". Ensure your membership provider configuration is up to date and that the default member type exists.");
                }

                var m = Member.MakeNew(username, email, memberType, User.GetUser(0));

                string salt;
                var encodedPassword = EncryptOrHashNewPassword(password, out salt);
                //set the password on the member
                m.ChangePassword(
                    FormatPasswordForStorage(encodedPassword, salt));
                
                var mUser = ConvertToMembershipUser(m);

                // custom fields
                if (string.IsNullOrEmpty(_passwordRetrievalQuestionPropertyTypeAlias) == false)
                    UpdateMemberProperty(m, _passwordRetrievalQuestionPropertyTypeAlias, passwordQuestion);

                if (string.IsNullOrEmpty(_passwordRetrievalAnswerPropertyTypeAlias) == false)
                    UpdateMemberProperty(m, _passwordRetrievalAnswerPropertyTypeAlias, passwordAnswer);

                if (string.IsNullOrEmpty(_approvedPropertyTypeAlias) == false)
                    UpdateMemberProperty(m, _approvedPropertyTypeAlias, isApproved ? 1 : 0);

                if (string.IsNullOrEmpty(_lastLoginPropertyTypeAlias) == false)
                {
                    mUser.LastActivityDate = DateTime.Now;
                    UpdateMemberProperty(m, _lastLoginPropertyTypeAlias, mUser.LastActivityDate);
                }

                if (string.IsNullOrEmpty(_lastPasswordChangedPropertyTypeAlias) == false)
                {
                    UpdateMemberProperty(m, _lastPasswordChangedPropertyTypeAlias, DateTime.Now);
                }

                // save
                m.Save();

                status = MembershipCreateStatus.Success;

                return mUser;
            }
            return null;
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
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion,
            string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            return CreateUser(_defaultMemberTypeAlias, username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
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
            var m = Member.GetMemberFromLoginName(username);
            if (m == null) return false;
            m.delete();
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
            var byEmail = ApplicationContext.Current.Services.MemberService.FindMembersByEmail(emailToMatch).ToArray();
            totalRecords = byEmail.Length;
            var pagedResult = new PagedResult<IMember>(totalRecords, pageIndex, pageSize);

            var collection = new MembershipUserCollection();            
            foreach (var m in byEmail.Skip(pagedResult.SkipSize).Take(pageSize))
            {
                collection.Add(ConvertToMembershipUser(m));
            }
            return collection;
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
            var membersList = new MembershipUserCollection();
            var memberArray = Member.GetMemberByName(usernameToMatch, false);
            totalRecords = memberArray.Length;

            foreach (var m in memberArray)
            {
                if (counter >= startIndex)
                    membersList.Add(ConvertToMembershipUser(m));
                if (counter >= endIndex) break;
                counter++;
            }
            return membersList;
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
            var membersList = new MembershipUserCollection();
            var memberArray = Member.GetAll;
            totalRecords = memberArray.Length;

            foreach (var m in memberArray)
            {
                if (counter >= startIndex)
                    membersList.Add(ConvertToMembershipUser(m));
                if (counter >= endIndex) break;
                counter++;
            }
            return membersList;

        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            return Member.CachedMembers().Count;
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
            if (EnablePasswordRetrieval == false)
                throw new ProviderException("Password Retrieval Not Enabled.");

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
                throw new ProviderException("Cannot retrieve Hashed passwords.");

            var m = Member.GetMemberFromLoginName(username);
            if (m != null)
            {
                if (RequiresQuestionAndAnswer)
                {
                    // check if password answer property alias is set
                    if (string.IsNullOrEmpty(_passwordRetrievalAnswerPropertyTypeAlias) == false)
                    {
                        // check if user is locked out
                        if (string.IsNullOrEmpty(_lockPropertyTypeAlias) == false)
                        {
                            var isLockedOut = false;
                            bool.TryParse(GetMemberProperty(m, _lockPropertyTypeAlias, true), out isLockedOut);
                            if (isLockedOut)
                            {
                                throw new MembershipPasswordException("The supplied user is locked out");
                            }
                        }

                        // match password answer
                        if (GetMemberProperty(m, _passwordRetrievalAnswerPropertyTypeAlias, false) != answer)
                        {
                            throw new MembershipPasswordException("Incorrect password answer");
                        }
                    }
                    else
                    {
                        throw new ProviderException("Password retrieval answer property alias is not set! To automatically support password question/answers you'll need to add references to the membertype properties in the 'Member' element in web.config by adding their aliases to the 'umbracoPasswordRetrievalQuestionPropertyTypeAlias' and 'umbracoPasswordRetrievalAnswerPropertyTypeAlias' attributes");
                    }
                }
            }
            if (m == null)
            {
                throw new MembershipPasswordException("The supplied user is not found");
            }
            return m.GetPassword();
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
            if (String.IsNullOrEmpty(username))
                return null;
            Member m = Member.GetMemberFromLoginName(username);
            if (m == null) return null;
            else return ConvertToMembershipUser(m);
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
            if (String.IsNullOrEmpty(providerUserKey.ToString()))
                return null;
            var m = new Member(Convert.ToInt32(providerUserKey));            
            return ConvertToMembershipUser(m);
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
            Member m = Member.GetMemberFromEmail(email);
            return m == null ? null : m.LoginName;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user (not used with Umbraco).</param>
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
            
            var m = Member.GetMemberFromLoginName(username);
            if (m == null)
                throw new MembershipPasswordException("The supplied user is not found");

            if (RequiresQuestionAndAnswer)
            {
                // check if password answer property alias is set
                if (string.IsNullOrEmpty(_passwordRetrievalAnswerPropertyTypeAlias) == false)
                {
                    // check if user is locked out
                    if (string.IsNullOrEmpty(_lockPropertyTypeAlias) == false)
                    {
                        var isLockedOut = false;
                        bool.TryParse(GetMemberProperty(m, _lockPropertyTypeAlias, true), out isLockedOut);
                        if (isLockedOut)
                        {
                            throw new MembershipPasswordException("The supplied user is locked out");
                        }
                    }

                    // match password answer
                    if (GetMemberProperty(m, _passwordRetrievalAnswerPropertyTypeAlias, false) != answer)
                    {
                        throw new MembershipPasswordException("Incorrect password answer");
                    }
                }
                else
                {
                    throw new ProviderException("Password retrieval answer property alias is not set! To automatically support password question/answers you'll need to add references to the membertype properties in the 'Member' element in web.config by adding their aliases to the 'umbracoPasswordRetrievalQuestionPropertyTypeAlias' and 'umbracoPasswordRetrievalAnswerPropertyTypeAlias' attributes");
                }
            }

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(newPassword, out salt);
            //set the password on the member
            m.ChangePassword(
                FormatPasswordForStorage(encodedPassword, salt));

            if (string.IsNullOrEmpty(_lastPasswordChangedPropertyTypeAlias) == false)
            {
                UpdateMemberProperty(m, _lastPasswordChangedPropertyTypeAlias, DateTime.Now);
            }

            m.Save();

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
            if (string.IsNullOrEmpty(_lockPropertyTypeAlias) == false)
            {
                var m = Member.GetMemberFromLoginName(userName);
                if (m != null)
                {
                    UpdateMemberProperty(m, _lockPropertyTypeAlias, 0);
                    m.Save();
                    return true;
                }
                throw new Exception(String.Format("No member with the username '{0}' found", userName));
            }
            throw new ProviderException("To enable lock/unlocking, you need to add a 'bool' property on your membertype and add the alias of the property in the 'umbracoLockPropertyTypeAlias' attribute of the membership element in the web.config.");
        }

        /// <summary>
        /// Updates e-mail and potentially approved status, lock status and comment on a user.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser(MembershipUser user)
        {
            var m = Member.GetMemberFromLoginName(user.UserName);
            m.Email = user.Email;

            // if supported, update approve status
            UpdateMemberProperty(m, _approvedPropertyTypeAlias, user.IsApproved ? 1 : 0);

            // if supported, update lock status
            UpdateMemberProperty(m, _lockPropertyTypeAlias, user.IsLockedOut ? 1 : 0);
            if (user.IsLockedOut)
            {
                UpdateMemberProperty(m, _lastLockedOutPropertyTypeAlias, DateTime.Now);
            }

            // if supported, update comment
            UpdateMemberProperty(m, _commentPropertyTypeAlias, user.Comment);

            m.Save();
        }

        private static void UpdateMemberProperty(Member m, string propertyAlias, object propertyValue)
        {
            if (string.IsNullOrEmpty(propertyAlias) == false)
            {
                if (m.getProperty(propertyAlias) != null)
                {
                    m.getProperty(propertyAlias).Value = propertyValue;
                }
            }
        }

        private static string GetMemberProperty(Member m, string propertyAlias, bool isBool)
        {
            if (string.IsNullOrEmpty(propertyAlias) == false)
            {
                if (m.getProperty(propertyAlias) != null &&
                    m.getProperty(propertyAlias).Value != null)
                {
                    if (isBool)
                    {
                        // Umbraco stored true as 1, which means it can be bool.tryParse'd
                        return m.getProperty(propertyAlias).Value.ToString().Replace("1", "true").Replace("0", "false");
                    }
                    return m.getProperty(propertyAlias).Value.ToString();
                }
            }

            return null;
        }

        private static string GetMemberProperty(IMember m, string propertyAlias, bool isBool)
        {
            if (string.IsNullOrEmpty(propertyAlias) == false)
            {
                if (m.Properties[propertyAlias] != null &&
                    m.Properties[propertyAlias].Value != null)
                {
                    if (isBool)
                    {
                        // Umbraco stored true as 1, which means it can be bool.tryParse'd
                        return m.Properties[propertyAlias].Value.ToString().Replace("1", "true").Replace("0", "false");
                    }
                    return m.Properties[propertyAlias].Value.ToString();
                }
            }

            return null;
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
            var m = Member.GetMemberFromLoginAndEncodedPassword(username, EncryptOrHashExistingPassword(password));
            if (m != null)
            {
                // check for lock status. If locked, then set the member property to null
                if (string.IsNullOrEmpty(_lockPropertyTypeAlias) == false)
                {
                    string lockedStatus = GetMemberProperty(m, _lockPropertyTypeAlias, true);
                    if (string.IsNullOrEmpty(lockedStatus) == false)
                    {
                        var isLocked = false;
                        if (bool.TryParse(lockedStatus, out isLocked))
                        {
                            if (isLocked)
                            {
                                m = null;
                            }
                        }
                    }
                }

                // check for approve status. If not approved, then set the member property to null
                if (!CheckApproveStatus(m)) {
                    m = null;
                }

                // maybe update login date
                if (m != null && string.IsNullOrEmpty(_lastLoginPropertyTypeAlias) == false)
                {
                    UpdateMemberProperty(m, _lastLoginPropertyTypeAlias, DateTime.Now);
                }

                // maybe reset password attempts
                if (m != null && string.IsNullOrEmpty(_failedPasswordAttemptsPropertyTypeAlias) == false)
                {
                    UpdateMemberProperty(m, _failedPasswordAttemptsPropertyTypeAlias, 0);
                }

                // persist data
                if (m != null)
                    m.Save();
            }
            else if (string.IsNullOrEmpty(_lockPropertyTypeAlias) == false
                && string.IsNullOrEmpty(_failedPasswordAttemptsPropertyTypeAlias) == false)
            {
                var updateMemberDataObject = Member.GetMemberFromLoginName(username);
                // update fail rate if it's approved
                if (updateMemberDataObject != null && CheckApproveStatus(updateMemberDataObject))
                {
                    int failedAttempts = 0;
                    int.TryParse(GetMemberProperty(updateMemberDataObject, _failedPasswordAttemptsPropertyTypeAlias, false), out failedAttempts);
                    failedAttempts = failedAttempts+1;
                    UpdateMemberProperty(updateMemberDataObject, _failedPasswordAttemptsPropertyTypeAlias, failedAttempts);

                    // lock user?
                    if (failedAttempts >= MaxInvalidPasswordAttempts)
                    {
                        UpdateMemberProperty(updateMemberDataObject, _lockPropertyTypeAlias, 1);
                        UpdateMemberProperty(updateMemberDataObject, _lastLockedOutPropertyTypeAlias, DateTime.Now);
                    }
                    updateMemberDataObject.Save();
                }

            }
            return (m != null);
        }

        private bool CheckApproveStatus(Member m)
        {
            var isApproved = false;
            if (string.IsNullOrEmpty(_approvedPropertyTypeAlias) == false)
            {
                if (m != null)
                {
                    var approveStatus = GetMemberProperty(m, _approvedPropertyTypeAlias, true);
                    if (string.IsNullOrEmpty(approveStatus) == false)
                    {
                        //try parsing as bool first (just in case)
                        if (bool.TryParse(approveStatus, out isApproved) == false)
                        {
                            int intStatus;
                            //if that fails, try parsing as int (since its normally stored as 0 or 1)
                            if (int.TryParse(approveStatus, out intStatus))
                            {
                                isApproved = intStatus != 0;
                            }
                        }
                    }
                }
            }
            else {
                // if we don't use approve statuses
                isApproved = true;
            }
            return isApproved;
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
            return LegacyEncodePassword(password);
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
        /// <param name="m">The m.</param>
        /// <returns></returns>
        private MembershipUser ConvertToMembershipUser(Member m)
        {
            if (m == null) return null;

            var lastLogin = DateTime.Now;
            var lastLocked = DateTime.MinValue;
            var isApproved = true;
            var isLocked = false;
            var comment = "";
            var passwordQuestion = "";

            // last login
            if (string.IsNullOrEmpty(_lastLoginPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, _lastLoginPropertyTypeAlias, false), out lastLogin);
            }
            // approved
            if (string.IsNullOrEmpty(_approvedPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, _approvedPropertyTypeAlias, true), out isApproved);
            }
            // locked
            if (string.IsNullOrEmpty(_lockPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, _lockPropertyTypeAlias, true), out isLocked);
            }
            // last locked
            if (string.IsNullOrEmpty(_lastLockedOutPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, _lastLockedOutPropertyTypeAlias, false), out lastLocked);
            }
            // comment
            if (string.IsNullOrEmpty(_commentPropertyTypeAlias) == false)
            {
                comment = GetMemberProperty(m, _commentPropertyTypeAlias, false);
            }
            // password question
            if (string.IsNullOrEmpty(_passwordRetrievalQuestionPropertyTypeAlias) == false)
            {
                passwordQuestion = GetMemberProperty(m, _passwordRetrievalQuestionPropertyTypeAlias, false);
            }

            return new MembershipUser(_providerName, m.LoginName, m.Id, m.Email, passwordQuestion, comment, isApproved, isLocked, m.CreateDateTime, lastLogin,
                                      DateTime.Now, DateTime.Now, lastLocked);
        }

        /// <summary>
        /// Converts to membership user.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        private MembershipUser ConvertToMembershipUser(IMember m)
        {
            if (m == null) return null;

            var lastLogin = DateTime.Now;
            var lastLocked = DateTime.MinValue;
            var isApproved = true;
            var isLocked = false;
            var comment = "";
            var passwordQuestion = "";

            // last login
            if (string.IsNullOrEmpty(_lastLoginPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, _lastLoginPropertyTypeAlias, false), out lastLogin);
            }
            // approved
            if (string.IsNullOrEmpty(_approvedPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, _approvedPropertyTypeAlias, true), out isApproved);
            }
            // locked
            if (string.IsNullOrEmpty(_lockPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, _lockPropertyTypeAlias, true), out isLocked);
            }
            // last locked
            if (string.IsNullOrEmpty(_lastLockedOutPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, _lastLockedOutPropertyTypeAlias, false), out lastLocked);
            }
            // comment
            if (string.IsNullOrEmpty(_commentPropertyTypeAlias) == false)
            {
                comment = GetMemberProperty(m, _commentPropertyTypeAlias, false);
            }
            // password question
            if (string.IsNullOrEmpty(_passwordRetrievalQuestionPropertyTypeAlias) == false)
            {
                passwordQuestion = GetMemberProperty(m, _passwordRetrievalQuestionPropertyTypeAlias, false);
            }

            return new MembershipUser(_providerName, m.Username, m.Id, m.Email, passwordQuestion, comment, isApproved, isLocked, m.CreateDate, lastLogin,
                                      DateTime.Now, DateTime.Now, lastLocked);
        }

        #endregion
    }

    //TODO: We need to re-enable this in 6.2, but need to back port most of the membership provider changes (still in a custom branch atm)

    ///// <summary>
    ///// Adds some event handling
    ///// </summary>
    //public class MembershipEventHandler : ApplicationEventHandler
    //{
    //    protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
    //    {
    //        Member.New += Member_New;
    //    }

    //    void Member_New(Member sender, NewEventArgs e)
    //    {
    //        //This is a bit of a hack to ensure that the member is approved when created since many people will be using
    //        // this old api to create members on the front-end and they need to be approved - which is based on whether or not 
    //        // the Umbraco membership provider is configured.
    //        var provider = Membership.Provider as UmbracoMembershipProvider;
    //        if (provider != null)
    //        {
    //            var approvedField = provider.ApprovedPropertyTypeAlias;
    //            var property = sender.getProperty(approvedField);
    //            if (property != null)
    //            {
    //                property.Value = 1;
    //            }
    //        }            
    //    }
    //}
}
