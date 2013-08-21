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
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;

using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
#endregion

namespace umbraco.providers.members
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)  
    /// </summary>

    public class UmbracoMembershipProvider : MembershipProvider
    {
        #region Fields
        private string m_ApplicationName;
        private bool m_EnablePasswordReset;
        private bool m_EnablePasswordRetrieval;
        private int m_MaxInvalidPasswordAttempts;
        private int m_MinRequiredNonAlphanumericCharacters;
        private int m_MinRequiredPasswordLength;
        private int m_PasswordAttemptWindow;
        private MembershipPasswordFormat m_PasswordFormat;
        private string m_PasswordStrengthRegularExpression;
        private bool m_RequiresQuestionAndAnswer;
        private bool m_RequiresUniqueEmail;
        private string m_DefaultMemberTypeAlias;
        private string m_LockPropertyTypeAlias;
        private string m_FailedPasswordAttemptsPropertyTypeAlias;
        private string m_ApprovedPropertyTypeAlias;
        private string m_CommentPropertyTypeAlias;
        private string m_LastLoginPropertyTypeAlias;
        private string m_PasswordRetrievalQuestionPropertyTypeAlias;
        private string m_PasswordRetrievalAnswerPropertyTypeAlias;
        private string m_providerName = Member.UmbracoMemberProviderName;
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
                return m_ApplicationName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ProviderException("ApplicationName cannot be empty.");

                if (value.Length > 0x100)
                    throw new ProviderException("Provider application name too long.");

                m_ApplicationName = value;
            }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get { return m_EnablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval
        {
            get { return m_EnablePasswordRetrieval; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return m_MaxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return m_MinRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength
        {
            get { return m_MinRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow
        {
            get { return m_PasswordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"></see> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return m_PasswordFormat; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <value></value>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return m_PasswordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <value></value>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return m_RequiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail
        {
            get { return m_RequiresUniqueEmail; }
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

            if (string.IsNullOrEmpty(name)) name = "UmbracoMembershipProvider";
            // Initialize base provider class
            base.Initialize(name, config);
            m_providerName = name;

            this.m_EnablePasswordRetrieval = SecUtility.GetBooleanValue(config, "enablePasswordRetrieval", false);
            this.m_EnablePasswordReset = SecUtility.GetBooleanValue(config, "enablePasswordReset", false);
            this.m_RequiresQuestionAndAnswer = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", false);
            this.m_RequiresUniqueEmail = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", true);
            this.m_MaxInvalidPasswordAttempts = SecUtility.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
            this.m_PasswordAttemptWindow = SecUtility.GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            this.m_MinRequiredPasswordLength = SecUtility.GetIntValue(config, "minRequiredPasswordLength", 7, true, 0x80);
            this.m_MinRequiredNonAlphanumericCharacters = SecUtility.GetIntValue(config, "minRequiredNonalphanumericCharacters", 1, true, 0x80);
            this.m_PasswordStrengthRegularExpression = config["passwordStrengthRegularExpression"];

            this.m_ApplicationName = config["applicationName"];
            if (string.IsNullOrEmpty(this.m_ApplicationName))
                this.m_ApplicationName = SecUtility.GetDefaultAppName();

            // make sure password format is clear by default.
            string str = config["passwordFormat"];
            if (str == null) str = "Clear";

            switch (str.ToLower())
            {
                case "clear":
                    this.m_PasswordFormat = MembershipPasswordFormat.Clear;
                    break;

                case "encrypted":
                    this.m_PasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;

                case "hashed":
                    this.m_PasswordFormat = MembershipPasswordFormat.Hashed;
                    break;

                default:
                    throw new ProviderException("Provider bad password format");
            }

            if ((this.PasswordFormat == MembershipPasswordFormat.Hashed) && this.EnablePasswordRetrieval)
                throw new ProviderException("Provider can not retrieve hashed password");

            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultMemberTypeAlias"] != null)
                m_DefaultMemberTypeAlias = config["defaultMemberTypeAlias"];
            else if (MemberType.GetAll.Length == 1)
                m_DefaultMemberTypeAlias = MemberType.GetAll[0].Alias;
            else
                throw new ProviderException("No default MemberType alias is specified in the web.config string. Please add a 'defaultMemberTypeAlias' to the add element in the provider declaration in web.config");

            // test for approve status
            if (config["umbracoApprovePropertyTypeAlias"] != null)
            {
                m_ApprovedPropertyTypeAlias = config["umbracoApprovePropertyTypeAlias"];
            }
            // test for lock attempts
            if (config["umbracoLockPropertyTypeAlias"] != null)
            {
                m_LockPropertyTypeAlias = config["umbracoLockPropertyTypeAlias"];
            }
            if (config["umbracoFailedPasswordAttemptsPropertyTypeAlias"] != null)
            {
                m_FailedPasswordAttemptsPropertyTypeAlias = config["umbracoFailedPasswordAttemptsPropertyTypeAlias"];
            }
            // comment property
            if (config["umbracoCommentPropertyTypeAlias"] != null)
            {
                m_CommentPropertyTypeAlias = config["umbracoCommentPropertyTypeAlias"];
            }
            // last login date
            if (config["umbracoLastLoginPropertyTypeAlias"] != null)
            {
                m_LastLoginPropertyTypeAlias = config["umbracoLastLoginPropertyTypeAlias"];
            }
            // password retrieval
            if (config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"] != null)
            {
                m_PasswordRetrievalQuestionPropertyTypeAlias = config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"];
            }
            if (config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"] != null)
            {
                m_PasswordRetrievalAnswerPropertyTypeAlias = config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"];
            }

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
            // in order to support updating passwords from the umbraco core, we can't validate the old password
            Member m = Member.GetMemberFromLoginNameAndPassword(username, oldPassword);
            if (m == null) return false;

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
            }
            string encodedPassword = EncodePassword(newPassword);
            m.ChangePassword(encodedPassword);

            return (m.Password == encodedPassword) ? true : false;
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
            if (!String.IsNullOrEmpty(m_PasswordRetrievalQuestionPropertyTypeAlias) && !String.IsNullOrEmpty(m_PasswordRetrievalAnswerPropertyTypeAlias))
            {
                if (ValidateUser(username, password))
                {
                    Member m = Member.GetMemberFromLoginName(username);
                    if (m != null)
                    {
                        UpdateMemberProperty(m, m_PasswordRetrievalQuestionPropertyTypeAlias, newPasswordQuestion);
                        UpdateMemberProperty(m, m_PasswordRetrievalAnswerPropertyTypeAlias, newPasswordAnswer);
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
            if (Member.GetMemberFromLoginName(username) != null)
                status = MembershipCreateStatus.DuplicateUserName;
            else if (Member.GetMemberFromEmail(email) != null && RequiresUniqueEmail)
                status = MembershipCreateStatus.DuplicateEmail;
            else
            {
                Member m = Member.MakeNew(username, email, MemberType.GetByAlias(m_DefaultMemberTypeAlias), User.GetUser(0));
                m.Password = password;

                MembershipUser mUser =
                    ConvertToMembershipUser(m);

                // custom fields
                if (!String.IsNullOrEmpty(m_PasswordRetrievalQuestionPropertyTypeAlias))
                    UpdateMemberProperty(m, m_PasswordRetrievalQuestionPropertyTypeAlias, passwordQuestion);
                
                if (!String.IsNullOrEmpty(m_PasswordRetrievalAnswerPropertyTypeAlias))
                    UpdateMemberProperty(m, m_PasswordRetrievalAnswerPropertyTypeAlias, passwordAnswer);
                
                if (!String.IsNullOrEmpty(m_ApprovedPropertyTypeAlias))
                    UpdateMemberProperty(m, m_ApprovedPropertyTypeAlias, isApproved);
                
                if (!String.IsNullOrEmpty(m_LastLoginPropertyTypeAlias)) {
                    mUser.LastActivityDate = DateTime.Now;
                    UpdateMemberProperty(m, m_LastLoginPropertyTypeAlias, mUser.LastActivityDate);
                }

                // save
                m.Save();

                status = MembershipCreateStatus.Success;

                return mUser;
            }
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
            Member m = Member.GetMemberFromLoginName(username);
            if (m == null) return false;
            else
            {
                m.delete();
                return true;
            }
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
            throw new Exception("The method or operation is not implemented.");
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
            MembershipUserCollection membersList = new MembershipUserCollection();
            Member[] memberArray = Member.GetMemberByName(usernameToMatch, false);
            totalRecords = memberArray.Length;

            foreach (Member m in memberArray)
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
            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;
            MembershipUserCollection membersList = new MembershipUserCollection();
            Member[] memberArray = Member.GetAll;
            totalRecords = memberArray.Length;

            foreach (Member m in memberArray)
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
            if (!EnablePasswordRetrieval)
                throw new ProviderException("Password Retrieval Not Enabled.");

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
                throw new ProviderException("Cannot retrieve Hashed passwords.");

            Member m = Member.GetMemberFromLoginName(username);
            if (m != null)
            {
                if (RequiresQuestionAndAnswer)
                {
                    // check if password answer property alias is set
                    if (!String.IsNullOrEmpty(m_PasswordRetrievalAnswerPropertyTypeAlias))
                    {
                        // check if user is locked out
                        if (!String.IsNullOrEmpty(m_LockPropertyTypeAlias))
                        {
                            bool isLockedOut = false;
                            bool.TryParse(GetMemberProperty(m, m_LockPropertyTypeAlias, true), out isLockedOut);
                            if (isLockedOut)
                            {
                                throw new MembershipPasswordException("The supplied user is locked out");
                            }
                        }

                        // match password answer
                        if (GetMemberProperty(m, m_PasswordRetrievalAnswerPropertyTypeAlias, false) != answer)
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
            else
            {
                return m.Password;
            }
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
            Member m = new Member(Convert.ToInt32(providerUserKey));
            if (m == null) return null;
            else return ConvertToMembershipUser(m);
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
            if (m == null) return null;
            else return m.LoginName;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user (not used with Umbraco).</param>
        /// <returns>The new password for the specified user.</returns>
        public override string ResetPassword(string username, string answer)
        {

            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not supported");
            }

            Member m = Member.GetMemberFromLoginName(username);
            if (m == null)
                throw new MembershipPasswordException("The supplied user is not found");
            else
            {
                if (RequiresQuestionAndAnswer)
                {
                    // check if password answer property alias is set
                    if (!String.IsNullOrEmpty(m_PasswordRetrievalAnswerPropertyTypeAlias))
                    {
                        // check if user is locked out
                        if (!String.IsNullOrEmpty(m_LockPropertyTypeAlias))
                        {
                            bool isLockedOut = false;
                            bool.TryParse(GetMemberProperty(m, m_LockPropertyTypeAlias, true), out isLockedOut);
                            if (isLockedOut)
                            {
                                throw new MembershipPasswordException("The supplied user is locked out");
                            }
                        }

                        // match password answer
                        if (GetMemberProperty(m, m_PasswordRetrievalAnswerPropertyTypeAlias, false) != answer)
                        {
                            throw new MembershipPasswordException("Incorrect password answer");
                        }
                    }
                    else
                    {
                        throw new ProviderException("Password retrieval answer property alias is not set! To automatically support password question/answers you'll need to add references to the membertype properties in the 'Member' element in web.config by adding their aliases to the 'umbracoPasswordRetrievalQuestionPropertyTypeAlias' and 'umbracoPasswordRetrievalAnswerPropertyTypeAlias' attributes");
                    }
                }
                string newPassword = Membership.GeneratePassword(MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);
                m.Password = newPassword;
                return newPassword;
            }
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
            if (!String.IsNullOrEmpty(m_LockPropertyTypeAlias))
            {
                Member m = Member.GetMemberFromLoginName(userName);
                if (m != null)
                {
                    UpdateMemberProperty(m, m_LockPropertyTypeAlias, false);
                    return true;
                }
                else
                {
                    throw new Exception(String.Format("No member with the username '{0}' found", userName));
                }
            }
            else
            {
                throw new ProviderException("To enable lock/unlocking, you need to add a 'bool' property on your membertype and add the alias of the property in the 'umbracoLockPropertyTypeAlias' attribute of the membership element in the web.config.");
            }
        }

        /// <summary>
        /// Updates e-mail and potentially approved status, lock status and comment on a user.
        /// Note: To automatically support lock, approve and comments you'll need to add references to the membertype properties in the 
        /// 'Member' element in web.config by adding their aliases to the 'umbracoApprovePropertyTypeAlias', 'umbracoLockPropertyTypeAlias' and 'umbracoCommentPropertyTypeAlias' attributes
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser(MembershipUser user)
        {
            Member m = Member.GetMemberFromLoginName(user.UserName);
            m.Email = user.Email;

            // if supported, update approve status
            UpdateMemberProperty(m, m_ApprovedPropertyTypeAlias, user.IsApproved);

            // if supported, update lock status
            UpdateMemberProperty(m, m_LockPropertyTypeAlias, user.IsLockedOut);

            // if supported, update comment
            UpdateMemberProperty(m, m_CommentPropertyTypeAlias, user.Comment);

            m.Save();
        }

        private static void UpdateMemberProperty(Member m, string propertyAlias, object propertyValue)
        {
            if (!String.IsNullOrEmpty(propertyAlias))
            {
                if (m.getProperty(propertyAlias) != null)
                {
                    m.getProperty(propertyAlias).Value =
                        propertyValue;
                }
            }
        }

        private static string GetMemberProperty(Member m, string propertyAlias, bool isBool)
        {
            if (!String.IsNullOrEmpty(propertyAlias))
            {
                if (m.getProperty(propertyAlias) != null &&
                    m.getProperty(propertyAlias).Value != null)
                {
                    if (isBool)
                    {
                        // Umbraco stored true as 1, which means it can be bool.tryParse'd
                        return m.getProperty(propertyAlias).Value.ToString().Replace("1", "true").Replace("0", "false");
                    }
                    else
                    return m.getProperty(propertyAlias).Value.ToString();
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
            Member m = Member.GetMemberFromLoginAndEncodedPassword(username, EncodePassword(password));
            if (m != null)
            {
                // check for lock status. If locked, then set the member property to null
                if (!String.IsNullOrEmpty(m_LockPropertyTypeAlias))
                {
                    string lockedStatus = GetMemberProperty(m, m_LockPropertyTypeAlias, true);
                    if (!String.IsNullOrEmpty(lockedStatus))
                    {
                        bool isLocked = false;
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
                if (m != null && !String.IsNullOrEmpty(m_LastLoginPropertyTypeAlias))
                {
                    UpdateMemberProperty(m, m_LastLoginPropertyTypeAlias, DateTime.Now);
                }

                // maybe reset password attempts
                if (m != null && !String.IsNullOrEmpty(m_FailedPasswordAttemptsPropertyTypeAlias))
                {
                    UpdateMemberProperty(m, m_FailedPasswordAttemptsPropertyTypeAlias, 0);
                }

                // persist data
                if (m != null)
                    m.Save();
            }
            else if (!String.IsNullOrEmpty(m_LockPropertyTypeAlias)
                && !String.IsNullOrEmpty(m_FailedPasswordAttemptsPropertyTypeAlias))
            {
                Member updateMemberDataObject = Member.GetMemberFromLoginName(username);
                // update fail rate if it's approved
                if (updateMemberDataObject != null && CheckApproveStatus(updateMemberDataObject))
                {
                    int failedAttempts = 0;
                    int.TryParse(GetMemberProperty(updateMemberDataObject, m_FailedPasswordAttemptsPropertyTypeAlias, false), out failedAttempts);
                    failedAttempts = failedAttempts+1;
                    UpdateMemberProperty(updateMemberDataObject, m_FailedPasswordAttemptsPropertyTypeAlias, failedAttempts);

                    // lock user?
                    if (failedAttempts >= MaxInvalidPasswordAttempts)
                    {
                        UpdateMemberProperty(updateMemberDataObject, m_LockPropertyTypeAlias, true);
                    }
                    updateMemberDataObject.Save();
                }

            }
            return (m != null);
        }

        private bool CheckApproveStatus(Member m)
        {
            bool isApproved = false;
            if (!String.IsNullOrEmpty(m_ApprovedPropertyTypeAlias))
            {
                if (m != null)
                {
                    string approveStatus = GetMemberProperty(m, m_ApprovedPropertyTypeAlias, true);
                    if (!String.IsNullOrEmpty(approveStatus))
                    {
                        bool.TryParse(approveStatus, out isApproved);
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
                    throw new ProviderException("Cannot decrypt a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return password;
        }

        /// <summary>
        /// Converts to membership user.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        private MembershipUser ConvertToMembershipUser(Member m)
        {
            if (m == null) return null;
            else
            {
                DateTime lastLogin = DateTime.Now;
                bool isApproved = true;
                bool isLocked = false;
                string comment = "";
                string passwordQuestion = "";

                // last login
                if (!String.IsNullOrEmpty(m_LastLoginPropertyTypeAlias))
                {
                    DateTime.TryParse(GetMemberProperty(m, m_LastLoginPropertyTypeAlias, false), out lastLogin);
                }
                // approved
                if (!String.IsNullOrEmpty(m_ApprovedPropertyTypeAlias))
                {
                    bool.TryParse(GetMemberProperty(m, m_ApprovedPropertyTypeAlias, true), out isApproved);
                }
                // locked
                if (!String.IsNullOrEmpty(m_LockPropertyTypeAlias))
                {
                    bool.TryParse(GetMemberProperty(m, m_LockPropertyTypeAlias, true), out isLocked);
                }
                // comment
                if (!String.IsNullOrEmpty(m_CommentPropertyTypeAlias))
                {
                    comment = GetMemberProperty(m, m_CommentPropertyTypeAlias, false);
                }
                // password question
                if (!String.IsNullOrEmpty(m_PasswordRetrievalQuestionPropertyTypeAlias))
                {
                    passwordQuestion = GetMemberProperty(m, m_PasswordRetrievalQuestionPropertyTypeAlias, false);
                }

                return new MembershipUser(m_providerName, m.LoginName, m.Id, m.Email, passwordQuestion, comment, isApproved, isLocked, m.CreateDateTime, lastLogin,
                  DateTime.Now, DateTime.Now, DateTime.Now);
            }
        }
        #endregion
    }
}
