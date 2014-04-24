#region namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using System.Security.Cryptography;
using System.Web.Util;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using Member = umbraco.cms.businesslogic.member.Member;
using MemberType = umbraco.cms.businesslogic.member.MemberType;
using User = umbraco.BusinessLogic.User;

#endregion

namespace umbraco.providers.members
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)  
    /// </summary>
    [Obsolete("This has been superceded by Umbraco.Web.Security.Providers.MembersMembershipProvider")]
    public class UmbracoMembershipProvider : UmbracoMembershipProviderBase, IUmbracoMemberTypeMembershipProvider
    {
        public UmbracoMembershipProvider()

        {
            LockPropertyTypeAlias = Constants.Conventions.Member.IsLockedOut;
            LastLockedOutPropertyTypeAlias = Constants.Conventions.Member.LastLockoutDate;
            FailedPasswordAttemptsPropertyTypeAlias = Constants.Conventions.Member.FailedPasswordAttempts;
            ApprovedPropertyTypeAlias = Constants.Conventions.Member.IsApproved;
            CommentPropertyTypeAlias = Constants.Conventions.Member.Comments;
            LastLoginPropertyTypeAlias = Constants.Conventions.Member.LastLoginDate;
            LastPasswordChangedPropertyTypeAlias = Constants.Conventions.Member.LastPasswordChangeDate;
            PasswordRetrievalQuestionPropertyTypeAlias = Constants.Conventions.Member.PasswordQuestion;
            PasswordRetrievalAnswerPropertyTypeAlias = Constants.Conventions.Member.PasswordAnswer;
        }

        #region Fields

        private string _defaultMemberTypeAlias = "Member";
        private string _providerName = Member.UmbracoMemberProviderName;
        private volatile bool _hasDefaultMember = false;
        private static readonly object Locker = new object();

        #endregion

        public string LockPropertyTypeAlias { get; protected set; }
        public string LastLockedOutPropertyTypeAlias { get; protected set; }
        public string FailedPasswordAttemptsPropertyTypeAlias { get; protected set; }
        public string ApprovedPropertyTypeAlias { get; protected set; }
        public string CommentPropertyTypeAlias { get; protected set; }
        public string LastLoginPropertyTypeAlias { get; protected set; }
        public string LastPasswordChangedPropertyTypeAlias { get; protected set; }
        public string PasswordRetrievalQuestionPropertyTypeAlias { get; protected set; }
        public string PasswordRetrievalAnswerPropertyTypeAlias { get; protected set; }

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

            if (string.IsNullOrEmpty(name)) name = Constants.Conventions.Member.UmbracoMemberProviderName;
            
            base.Initialize(name, config);
            
            _providerName = name;
            
            // test for membertype (if not specified, choose the first member type available)
            if (config["defaultMemberTypeAlias"] != null)
            {
                _defaultMemberTypeAlias = config["defaultMemberTypeAlias"];
                if (_defaultMemberTypeAlias.IsNullOrWhiteSpace())
                {
                    throw new ProviderException("No default user type alias is specified in the web.config string. Please add a 'defaultUserTypeAlias' to the add element in the provider declaration in web.config");
                }
                _hasDefaultMember = true;
            }

            // test for approve status
            if (config["umbracoApprovePropertyTypeAlias"] != null)
            {
                ApprovedPropertyTypeAlias = config["umbracoApprovePropertyTypeAlias"];
            }
            // test for lock attempts
            if (config["umbracoLockPropertyTypeAlias"] != null)
            {
                LockPropertyTypeAlias = config["umbracoLockPropertyTypeAlias"];
            }
            if (config["umbracoLastLockedPropertyTypeAlias"] != null)
            {
                LastLockedOutPropertyTypeAlias = config["umbracoLastLockedPropertyTypeAlias"];
            }
            if (config["umbracoLastPasswordChangedPropertyTypeAlias"] != null)
            {
                LastPasswordChangedPropertyTypeAlias = config["umbracoLastPasswordChangedPropertyTypeAlias"];
            }
            if (config["umbracoFailedPasswordAttemptsPropertyTypeAlias"] != null)
            {
                FailedPasswordAttemptsPropertyTypeAlias = config["umbracoFailedPasswordAttemptsPropertyTypeAlias"];
            }
            // comment property
            if (config["umbracoCommentPropertyTypeAlias"] != null)
            {
                CommentPropertyTypeAlias = config["umbracoCommentPropertyTypeAlias"];
            }
            // last login date
            if (config["umbracoLastLoginPropertyTypeAlias"] != null)
            {
                LastLoginPropertyTypeAlias = config["umbracoLastLoginPropertyTypeAlias"];
            }
            // password retrieval
            if (config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"] != null)
            {
                PasswordRetrievalQuestionPropertyTypeAlias = config["umbracoPasswordRetrievalQuestionPropertyTypeAlias"];
            }
            if (config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"] != null)
            {
                PasswordRetrievalAnswerPropertyTypeAlias = config["umbracoPasswordRetrievalAnswerPropertyTypeAlias"];
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
            
            string salt;
            var encodedPassword = EncryptOrHashNewPassword(newPassword, out salt);
            m.ChangePassword(
                FormatPasswordForStorage(encodedPassword, salt));

            UpdateMemberProperty(m, LastPasswordChangedPropertyTypeAlias, DateTime.Now);

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
        protected override bool PerformChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (string.IsNullOrEmpty(PasswordRetrievalQuestionPropertyTypeAlias) || string.IsNullOrEmpty(PasswordRetrievalAnswerPropertyTypeAlias))
            {
                throw new NotSupportedException("Updating the password Question and Answer is not valid if the properties aren't set in the config file");
            }

            var m = Member.GetMemberFromLoginName(username);
            if (m == null)
            {
                return false;
            }

            UpdateMemberProperty(m, PasswordRetrievalQuestionPropertyTypeAlias, newPasswordQuestion);
            UpdateMemberProperty(m, PasswordRetrievalAnswerPropertyTypeAlias, newPasswordAnswer);
            m.Save();
            return true;
        }

        public override string DefaultMemberTypeAlias
        {
            get
            {
                if (_hasDefaultMember == false)
                {
                    lock (Locker)
                    {
                        if (_hasDefaultMember == false)
                        {
                            var types = MemberType.GetAll;
                            if (types.Length == 1)
                                _defaultMemberTypeAlias = types[0].Alias;
                            else
                                throw new ProviderException("No default MemberType alias is specified in the web.config string. Please add a 'defaultMemberTypeAlias' to the add element in the provider declaration in web.config");

                            _hasDefaultMember = true;
                        }
                    }
                }
                return _defaultMemberTypeAlias;
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
        protected override MembershipUser PerformCreateUser(string memberTypeAlias, string username, string password, string email, string passwordQuestion,
                                                    string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (Member.GetMemberFromLoginName(username) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                LogHelper.Warn<UmbracoMembershipProvider>("Cannot create member as username already exists: " + username);
                return null;
            }
            
            if (Member.GetMemberFromEmail(email) != null && RequiresUniqueEmail)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                LogHelper.Warn<UmbracoMembershipProvider>(
                    "Cannot create member as a member with the same email address exists: " + email);
                return null;
            }
            
            var memberType = MemberType.GetByAlias(memberTypeAlias);
            if (memberType == null)
            {
                throw new InvalidOperationException("Could not find a member type with alias " + memberTypeAlias + ". Ensure your membership provider configuration is up to date and that the default member type exists.");
            }

            var m = Member.MakeNew(username, email, memberType, User.GetUser(0));

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(password, out salt);
            
            //set the password on the member
            m.ChangePassword(FormatPasswordForStorage(encodedPassword, salt));
                
            // custom fields
            if (string.IsNullOrEmpty(PasswordRetrievalQuestionPropertyTypeAlias) == false)
            {
                UpdateMemberProperty(m, PasswordRetrievalQuestionPropertyTypeAlias, passwordQuestion);
            }

            if (string.IsNullOrEmpty(PasswordRetrievalAnswerPropertyTypeAlias) == false)
            {
                UpdateMemberProperty(m, PasswordRetrievalAnswerPropertyTypeAlias, passwordAnswer);                    
            }   

            if (string.IsNullOrEmpty(ApprovedPropertyTypeAlias) == false)
            {
                UpdateMemberProperty(m, ApprovedPropertyTypeAlias, isApproved ? 1 : 0);                 
            }

            if (string.IsNullOrEmpty(LastLoginPropertyTypeAlias) == false)
            {
                UpdateMemberProperty(m, LastLoginPropertyTypeAlias, DateTime.Now);
            }

            if (string.IsNullOrEmpty(LastPasswordChangedPropertyTypeAlias) == false)
            {
                UpdateMemberProperty(m, LastPasswordChangedPropertyTypeAlias, DateTime.Now);
            }

            var mUser = ConvertToMembershipUser(m);

            // save
            m.Save();

            status = MembershipCreateStatus.Success;

            return mUser;
        }


        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">
        /// TODO: This setting currently has no effect
        /// </param>
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
            var byEmail = ApplicationContext.Current.Services.MemberService.FindByEmail(emailToMatch, pageIndex, pageSize, out totalRecords, StringPropertyMatchType.Wildcard).ToArray();
            
            var collection = new MembershipUserCollection();                        
            foreach (var m in byEmail)
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
            var byEmail = ApplicationContext.Current.Services.MemberService.FindByUsername(usernameToMatch, pageIndex, pageSize, out totalRecords, StringPropertyMatchType.Wildcard).ToArray();
            
            var collection = new MembershipUserCollection();            
            foreach (var m in byEmail)
            {
                collection.Add(ConvertToMembershipUser(m));
            }
            return collection;
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
            var membersList = new MembershipUserCollection();

            var pagedMembers = ApplicationContext.Current.Services.MemberService.GetAll(pageIndex, pageSize, out totalRecords);

            foreach (var m in pagedMembers)
            {
                membersList.Add(ConvertToMembershipUser(m));
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
            return ApplicationContext.Current.Services.MemberService.GetCount(MemberCountType.Online);
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        protected override string PerformGetPassword(string username, string answer)
        {
            var m = Member.GetMemberFromLoginName(username);
            if (m == null)
            {
                throw new MembershipPasswordException("The supplied user is not found");
            }

            // check if user is locked out
            if (string.IsNullOrEmpty(LockPropertyTypeAlias) == false)
            {
                var isLockedOut = false;
                bool.TryParse(GetMemberProperty(m, LockPropertyTypeAlias, true), out isLockedOut);
                if (isLockedOut)
                {
                    throw new MembershipPasswordException("The supplied user is locked out");
                }
            }

            if (RequiresQuestionAndAnswer)
            {
                // check if password answer property alias is set
                if (string.IsNullOrEmpty(PasswordRetrievalAnswerPropertyTypeAlias) == false)
                {
                    // match password answer
                    if (GetMemberProperty(m, PasswordRetrievalAnswerPropertyTypeAlias, false) != answer)
                    {
                        throw new MembershipPasswordException("Incorrect password answer");
                    }
                }
                else
                {
                    throw new ProviderException("Password retrieval answer property alias is not set! To automatically support password question/answers you'll need to add references to the membertype properties in the 'Member' element in web.config by adding their aliases to the 'umbracoPasswordRetrievalQuestionPropertyTypeAlias' and 'umbracoPasswordRetrievalAnswerPropertyTypeAlias' attributes");
                }
            }

            var decodedPassword = DecryptPassword(m.GetPassword());

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
            if (string.IsNullOrEmpty(username))
                return null;
            var m = Member.GetMemberFromLoginName(username);
            
            if (m == null)
            {
                return null;
            }

            if (userIsOnline && LastLoginPropertyTypeAlias.IsNullOrWhiteSpace() == false)
            {
                UpdateMemberProperty(m, LastLoginPropertyTypeAlias, DateTime.Now);

                //don't raise events for this! It just sets the member dates, if we do raise events this will
                // cause all distributed cache to execute - which will clear out some caches we don't want.
                // http://issues.umbraco.org/issue/U4-3451
                m.Save(false);
            }

            return ConvertToMembershipUser(m);
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
            var asGuid = providerUserKey.TryConvertTo<Guid>();
            if (asGuid.Success)
            {
                var m = new Member(asGuid.Result);
                if (userIsOnline && LastLoginPropertyTypeAlias.IsNullOrWhiteSpace() == false)
                {
                    UpdateMemberProperty(m, LastLoginPropertyTypeAlias, DateTime.Now);
                    //don't raise events for this! It just sets the member dates, if we do raise events this will
                    // cause all distributed cache to execute - which will clear out some caches we don't want.
                    // http://issues.umbraco.org/issue/U4-3451
                    m.Save(false);
                }
                return ConvertToMembershipUser(m);    
            }
            var asInt = providerUserKey.TryConvertTo<int>();
            if (asInt.Success)
            {
                var m = new Member(asInt.Result);
                if (userIsOnline && LastLoginPropertyTypeAlias.IsNullOrWhiteSpace() == false)
                {
                    UpdateMemberProperty(m, LastLoginPropertyTypeAlias, DateTime.Now);
                    //don't raise events for this! It just sets the member dates, if we do raise events this will
                    // cause all distributed cache to execute - which will clear out some caches we don't want.
                    // http://issues.umbraco.org/issue/U4-3451
                    m.Save(false);
                }
                return ConvertToMembershipUser(m);    
            }

            throw new InvalidOperationException("The " + GetType() + " provider only supports GUID or Int as a providerUserKey");

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
            var m = Member.GetMemberFromEmail(email);
            return m == null ? null : m.LoginName;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user (not used with Umbraco).</param>
        /// <param name="generatedPassword"></param>
        /// <returns>The new password for the specified user.</returns>
        protected override string PerformResetPassword(string username, string answer, string generatedPassword)
        {
            //TODO: This should be here - but how do we update failure count in this provider??
            //if (answer == null && RequiresQuestionAndAnswer)
            //{
            //    UpdateFailureCount(username, "passwordAnswer");

            //    throw new ProviderException("Password answer required for password reset.");
            //}
            
            var m = Member.GetMemberFromLoginName(username);
            if (m == null)
            {
                throw new ProviderException("The supplied user is not found");
            }

            // check if user is locked out
            if (string.IsNullOrEmpty(LockPropertyTypeAlias) == false)
            {
                var isLockedOut = false;
                bool.TryParse(GetMemberProperty(m, LockPropertyTypeAlias, true), out isLockedOut);
                if (isLockedOut)
                {
                    throw new ProviderException("The member is locked out.");
                }
            }

            if (RequiresQuestionAndAnswer)
            {
                // check if password answer property alias is set
                if (string.IsNullOrEmpty(PasswordRetrievalAnswerPropertyTypeAlias) == false)
                {
                    // match password answer
                    if (GetMemberProperty(m, PasswordRetrievalAnswerPropertyTypeAlias, false) != answer)
                    {
                        throw new ProviderException("Incorrect password answer");
                    }
                }
                else
                {
                    throw new ProviderException("Password retrieval answer property alias is not set! To automatically support password question/answers you'll need to add references to the membertype properties in the 'Member' element in web.config by adding their aliases to the 'umbracoPasswordRetrievalQuestionPropertyTypeAlias' and 'umbracoPasswordRetrievalAnswerPropertyTypeAlias' attributes");
                }
            }

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(generatedPassword, out salt);
            //set the password on the member
            m.ChangePassword(FormatPasswordForStorage(encodedPassword, salt));

            if (string.IsNullOrEmpty(LastPasswordChangedPropertyTypeAlias) == false)
            {
                UpdateMemberProperty(m, LastPasswordChangedPropertyTypeAlias, DateTime.Now);
            }

            m.Save();

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
            if (string.IsNullOrEmpty(LockPropertyTypeAlias) == false)
            {
                var m = Member.GetMemberFromLoginName(userName);
                if (m != null)
                {
                    UpdateMemberProperty(m, LockPropertyTypeAlias, 0);
                    if (string.IsNullOrEmpty(FailedPasswordAttemptsPropertyTypeAlias) == false)
                    {
                        UpdateMemberProperty(m, FailedPasswordAttemptsPropertyTypeAlias, 0);
                    }
                    m.Save();
                    return true;
                }
                throw new ProviderException(string.Format("No member with the username '{0}' found", userName));
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

            if (m == null)
            {
                throw new ProviderException(string.Format("No member with the username '{0}' found", user.UserName));
            }                

            m.Email = user.Email;

            // if supported, update approve status
            UpdateMemberProperty(m, ApprovedPropertyTypeAlias, user.IsApproved ? 1 : 0);

            // if supported, update lock status
            UpdateMemberProperty(m, LockPropertyTypeAlias, user.IsLockedOut ? 1 : 0);
            if (user.IsLockedOut)
            {
                UpdateMemberProperty(m, LastLockedOutPropertyTypeAlias, DateTime.Now);
            }

            // if supported, update comment
            UpdateMemberProperty(m, CommentPropertyTypeAlias, user.Comment);

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
            var m = Member.GetMemberFromLoginName(username);
            if (m == null) return false;
            var authenticated = CheckPassword(password, m.GetPassword());
            
            if (authenticated)
            {
                // check for lock status. If locked, then set the member property to null
                if (string.IsNullOrEmpty(LockPropertyTypeAlias) == false)
                {
                    string lockedStatus = GetMemberProperty(m, LockPropertyTypeAlias, true);
                    if (string.IsNullOrEmpty(lockedStatus) == false)
                    {
                        var isLocked = false;
                        if (bool.TryParse(lockedStatus, out isLocked))
                        {
                            if (isLocked)
                            {
                                LogHelper.Info<UmbracoMembershipProvider>("Cannot validate member " + username + " because they are currently locked out");
                                return false;
                            }
                        }
                    }
                }

                //check for approve status. If not approved, then set the member property to null
                if (CheckApproveStatus(m) == false)
                {
                    LogHelper.Info<UmbracoMembershipProvider>("Cannot validate member " + username + " because they are not approved");
                    return false;
                }

                // maybe update login date
                if (string.IsNullOrEmpty(LastLoginPropertyTypeAlias) == false)
                {
                    UpdateMemberProperty(m, LastLoginPropertyTypeAlias, DateTime.Now);
                }

                // maybe reset password attempts
                if (string.IsNullOrEmpty(FailedPasswordAttemptsPropertyTypeAlias) == false)
                {
                    UpdateMemberProperty(m, FailedPasswordAttemptsPropertyTypeAlias, 0);
                }

                // persist data

                //don't raise events for this! It just sets the member dates, if we do raise events this will
                // cause all distributed cache to execute - which will clear out some caches we don't want.
                // http://issues.umbraco.org/issue/U4-3451
                m.Save(false);

                return true;
            }


            // update fail rate if it's approved
            if (string.IsNullOrEmpty(LockPropertyTypeAlias) == false
                && string.IsNullOrEmpty(FailedPasswordAttemptsPropertyTypeAlias) == false)
            {                                
                if (CheckApproveStatus(m))
                {
                    var failedAttempts = 0;
                    int.TryParse(GetMemberProperty(m, FailedPasswordAttemptsPropertyTypeAlias, false), out failedAttempts);
                    failedAttempts = failedAttempts + 1;
                    UpdateMemberProperty(m, FailedPasswordAttemptsPropertyTypeAlias, failedAttempts);

                    // lock user?
                    if (failedAttempts >= MaxInvalidPasswordAttempts)
                    {
                        UpdateMemberProperty(m, LockPropertyTypeAlias, 1);
                        UpdateMemberProperty(m, LastLockedOutPropertyTypeAlias, DateTime.Now);
                        LogHelper.Info<UmbracoMembershipProvider>("Member " + username + " is now locked out, max invalid password attempts exceeded");
                    }

                    //don't raise events for this! It just sets the member dates, if we do raise events this will
                    // cause all distributed cache to execute - which will clear out some caches we don't want.
                    // http://issues.umbraco.org/issue/U4-3451
                    m.Save(false);
                }

            }

            return false;
        }

        private static void UpdateMemberProperty(Member m, string propertyTypeAlias, object propertyValue)
        {
            if (string.IsNullOrEmpty(propertyTypeAlias) == false)
            {
                if (m.getProperty(propertyTypeAlias) != null)
                {
                    m.getProperty(propertyTypeAlias).Value = propertyValue;
                }
            }
        }

        private static string GetMemberProperty(Member m, string propertyTypeAlias, bool isBool)
        {
            if (string.IsNullOrEmpty(propertyTypeAlias) == false)
            {
                if (m.getProperty(propertyTypeAlias) != null &&
                    m.getProperty(propertyTypeAlias).Value != null)
                {
                    if (isBool)
                    {
                        // Umbraco stored true as 1, which means it can be bool.tryParse'd
                        return m.getProperty(propertyTypeAlias).Value.ToString().Replace("1", "true").Replace("0", "false");
                    }
                    return m.getProperty(propertyTypeAlias).Value.ToString();
                }
            }

            return null;
        }

        private static string GetMemberProperty(IMember m, string propertyTypeAlias, bool isBool)
        {
            if (string.IsNullOrEmpty(propertyTypeAlias) == false)
            {
                if (m.Properties.Contains(propertyTypeAlias) && 
                    m.Properties[propertyTypeAlias] != null &&
                    m.Properties[propertyTypeAlias].Value != null)
                {
                    if (isBool)
                    {
                        // Umbraco stored true as 1, which means it can be bool.tryParse'd
                        return m.Properties[propertyTypeAlias].Value.ToString().Replace("1", "true").Replace("0", "false");
                    }
                    return m.Properties[propertyTypeAlias].Value.ToString();
                }
            }

            return null;
        }
        
        private bool CheckApproveStatus(Member m)
        {
            var isApproved = false;
            if (string.IsNullOrEmpty(ApprovedPropertyTypeAlias) == false)
            {
                if (m != null)
                {
                    var approveStatus = GetMemberProperty(m, ApprovedPropertyTypeAlias, true);
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
                    else
                    {
                        //There is no property so we shouldn't use the approve status
                        isApproved = true;
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
        /// Encodes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encoded password.</returns>
        [Obsolete("Do not use this, it is the legacy way to encode a password - use the base class EncryptOrHashExistingPassword instead")]
        public string EncodePassword(string password)
        {
            return LegacyEncodePassword(password);
        }

        /// <summary>
        /// Unencode password.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns>The unencoded password.</returns>
        [Obsolete("Do not use this, it is the legacy way to decode a password - use the base class DecodePassword instead")]
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
            if (string.IsNullOrEmpty(LastLoginPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, LastLoginPropertyTypeAlias, false), out lastLogin);
            }
            // approved
            if (string.IsNullOrEmpty(ApprovedPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, ApprovedPropertyTypeAlias, true), out isApproved);
            }
            // locked
            if (string.IsNullOrEmpty(LockPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, LockPropertyTypeAlias, true), out isLocked);
            }
            // last locked
            if (string.IsNullOrEmpty(LastLockedOutPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, LastLockedOutPropertyTypeAlias, false), out lastLocked);
            }
            // comment
            if (string.IsNullOrEmpty(CommentPropertyTypeAlias) == false)
            {
                comment = GetMemberProperty(m, CommentPropertyTypeAlias, false);
            }
            // password question
            if (string.IsNullOrEmpty(PasswordRetrievalQuestionPropertyTypeAlias) == false)
            {
                passwordQuestion = GetMemberProperty(m, PasswordRetrievalQuestionPropertyTypeAlias, false);
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
            if (string.IsNullOrEmpty(LastLoginPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, LastLoginPropertyTypeAlias, false), out lastLogin);
            }
            // approved
            if (string.IsNullOrEmpty(ApprovedPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, ApprovedPropertyTypeAlias, true), out isApproved);
            }
            // locked
            if (string.IsNullOrEmpty(LockPropertyTypeAlias) == false)
            {
                bool.TryParse(GetMemberProperty(m, LockPropertyTypeAlias, true), out isLocked);
            }
            // last locked
            if (string.IsNullOrEmpty(LastLockedOutPropertyTypeAlias) == false)
            {
                DateTime.TryParse(GetMemberProperty(m, LastLockedOutPropertyTypeAlias, false), out lastLocked);
            }
            // comment
            if (string.IsNullOrEmpty(CommentPropertyTypeAlias) == false)
            {
                comment = GetMemberProperty(m, CommentPropertyTypeAlias, false);
            }
            // password question
            if (string.IsNullOrEmpty(PasswordRetrievalQuestionPropertyTypeAlias) == false)
            {
                passwordQuestion = GetMemberProperty(m, PasswordRetrievalQuestionPropertyTypeAlias, false);
            }

            return new MembershipUser(_providerName, m.Username, m.Id, m.Email, passwordQuestion, comment, isApproved, isLocked, m.CreateDate, lastLogin,
                                      DateTime.Now, DateTime.Now, lastLocked);
        }

        #endregion
    }
}
