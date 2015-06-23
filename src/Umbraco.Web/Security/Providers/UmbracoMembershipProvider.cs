using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Security.Providers
{
    

    /// <summary>
    /// Abstract Membership Provider that users any implementation of IMembershipMemberService{TEntity} service
    /// </summary>
    public abstract class UmbracoMembershipProvider<T, TEntity> : UmbracoMembershipProviderBase
        where T : IMembershipMemberService<TEntity>
        where TEntity : class, IMembershipUser
    {

        protected IMembershipMemberService<TEntity> MemberService { get; private set; }
        
        protected UmbracoMembershipProvider(IMembershipMemberService<TEntity> memberService)
        {
            MemberService = memberService;
        }

        public abstract string ProviderName { get; }

        protected abstract MembershipUser ConvertToMembershipUser(TEntity entity);

        private bool _allowManuallyChangingPassword = true;

        /// <summary>
        /// For backwards compatibility, this provider supports this option by default it is true
        /// </summary>
        public override bool AllowManuallyChangingPassword
        {
            get { return _allowManuallyChangingPassword; }
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

            _allowManuallyChangingPassword = config.GetValue("allowManuallyChangingPassword", true);
        }

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
            //NOTE: due to backwards compatibilty reasons (and UX reasons), this provider doesn't care about the old password and 
            // allows simply setting the password manually so we don't really care about the old password.
            // This is allowed based on the overridden AllowManuallyChangingPassword option.

            // in order to support updating passwords from the umbraco core, we can't validate the old password
            var m = MemberService.GetByUsername(username);
            if (m == null) return false;
            
            string salt;
            var encodedPassword = EncryptOrHashNewPassword(newPassword, out salt);

            m.RawPasswordValue = FormatPasswordForStorage(encodedPassword, salt);
            m.LastPasswordChangeDate = DateTime.Now;

            MemberService.Save(m);

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
            var member = MemberService.GetByUsername(username);
            if (member == null)
            {
                return false;
            }

            member.PasswordQuestion = newPasswordQuestion;
            member.RawPasswordAnswerValue = EncryptString(newPasswordAnswer);

            MemberService.Save(member);

            return true;
        }

        /// <summary>
        /// Adds a new membership user to the data source with the specified member type
        /// </summary>
        /// <param name="memberTypeAlias">A specific member type to create the member for</param>
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
            // See if the user already exists
            if (MemberService.Exists(username))
            {
                status = MembershipCreateStatus.DuplicateUserName;
                LogHelper.Warn<UmbracoMembershipProvider<T, TEntity>>("Cannot create member as username already exists: " + username);
                return null;
            }

            // See if the email is unique
            if (MemberService.GetByEmail(email) != null && RequiresUniqueEmail)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                LogHelper.Warn<UmbracoMembershipProvider<T, TEntity>>(
                    "Cannot create member as a member with the same email address exists: " + email);
                return null;
            }

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(password, out salt);

            var member = MemberService.CreateWithIdentity(
                username,
                email, 
                FormatPasswordForStorage(encodedPassword, salt), 
                memberTypeAlias);
            
            member.PasswordQuestion = passwordQuestion;
            member.RawPasswordAnswerValue = EncryptString(passwordAnswer);
            member.IsApproved = isApproved;
            member.LastLoginDate = DateTime.Now;
            member.LastPasswordChangeDate = DateTime.Now;

            MemberService.Save(member);

            status = MembershipCreateStatus.Success;
            return ConvertToMembershipUser(member);
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
            var member = MemberService.GetByUsername(username);
            if (member == null) return false;

            MemberService.Delete(member);
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
            var byEmail = MemberService.FindByEmail(emailToMatch, pageIndex, pageSize, out totalRecords, StringPropertyMatchType.Wildcard).ToArray();
            
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
            var byEmail = MemberService.FindByUsername(usernameToMatch, pageIndex, pageSize, out totalRecords, StringPropertyMatchType.Wildcard).ToArray();

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

            var pagedMembers = MemberService.GetAll(pageIndex, pageSize, out totalRecords);
            
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
        /// <remarks>
        /// The way this is done is the same way that it is done in the MS SqlMembershipProvider - We query for any members
        /// that have their last active date within the Membership.UserIsOnlineTimeWindow (which is in minutes). It isn't exact science
        /// but that is how MS have made theirs so we'll follow that principal.
        /// </remarks>
        public override int GetNumberOfUsersOnline()
        {
            return MemberService.GetCount(MemberCountType.Online);
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
            var m = MemberService.GetByUsername(username);
            if (m == null)
            {
                throw new ProviderException("The supplied user is not found");
            }

            var encAnswer = EncryptString(answer);

            if (RequiresQuestionAndAnswer && m.RawPasswordAnswerValue != encAnswer)
            {
                throw new ProviderException("Incorrect password answer");
            }

            var decodedPassword = DecryptPassword(m.RawPasswordValue);

            return decodedPassword;
        }

        internal string EncryptString(string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return "";
            }
            var bytes = Encoding.Unicode.GetBytes(str);
            var password = new byte[bytes.Length];
            Buffer.BlockCopy(bytes, 0, password, 0, bytes.Length);
            var encBytes = EncryptPassword(password, MembershipPasswordCompatibilityMode.Framework40);
            return Convert.ToBase64String(encBytes);
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
            if (member == null)
            {
                return null;
            }

            if (userIsOnline)
            {
                member.LastLoginDate = DateTime.Now;
                member.UpdateDate = DateTime.Now;
                //don't raise events for this! It just sets the member dates, if we do raise events this will
                // cause all distributed cache to execute - which will clear out some caches we don't want.
                // http://issues.umbraco.org/issue/U4-3451

                // when upgrating from 7.2 to 7.3 trying to save will throw
                if (UmbracoVersion.Current >= new Version(7, 3, 0, 0))
                    MemberService.Save(member, false);
            }

            return ConvertToMembershipUser(member);
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
            var member = MemberService.GetByProviderKey(providerUserKey);
            if (member == null)
            {
                return null;
            }

            if (userIsOnline)
            {
                member.LastLoginDate = DateTime.Now;
                member.UpdateDate = DateTime.Now;
                //don't raise events for this! It just sets the member dates, if we do raise events this will
                // cause all distributed cache to execute - which will clear out some caches we don't want.
                // http://issues.umbraco.org/issue/U4-3451
                MemberService.Save(member, false);
            }

            return ConvertToMembershipUser(member);
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
            
            var m = MemberService.GetByUsername(username);
            if (m == null)
            {
                throw new ProviderException("The supplied user is not found");
            }

            if (m.IsLockedOut)
            {
                throw new ProviderException("The member is locked out.");
            }

            var encAnswer = EncryptString(answer);

            if (RequiresQuestionAndAnswer && m.RawPasswordAnswerValue != encAnswer)
            {
                throw new ProviderException("Incorrect password answer");
            }

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(generatedPassword, out salt);
            m.RawPasswordValue = FormatPasswordForStorage(encodedPassword, salt);
            m.LastPasswordChangeDate = DateTime.Now;
            MemberService.Save(m);
            
            return generatedPassword;
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

            if (member == null)
            {
                throw new ProviderException(string.Format("No member with the username '{0}' found", username));
            }                

            // Non need to update
            if (member.IsLockedOut == false) return true;

            member.IsLockedOut = false;
            member.FailedPasswordAttempts = 0;

            MemberService.Save(member);

            return true;
        }

        /// <summary>
        /// Updates e-mail  approved status, lock status and comment on a user.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>      
        public override void UpdateUser(MembershipUser user)
        {
            var m = MemberService.GetByUsername(user.UserName);

            if (m == null)
            {
                throw new ProviderException(string.Format("No member with the username '{0}' found", user.UserName));
            }

            if (RequiresUniqueEmail && user.Email.Trim().IsNullOrWhiteSpace() == false)
            {
                int totalRecs;
                var byEmail = MemberService.FindByEmail(user.Email.Trim(), 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Exact);
                if (byEmail.Count(x => x.Id != m.Id) > 0)
                {
                    throw new ProviderException(string.Format("A member with the email '{0}' already exists", user.Email));
                }
            }
            m.Email = user.Email;
            
            m.IsApproved = user.IsApproved;
            m.IsLockedOut = user.IsLockedOut;
            if (user.IsLockedOut)
            {
                m.LastLockoutDate = DateTime.Now;
            }
            m.Comments = user.Comment;

            MemberService.Save(m);
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

            if (member == null)
            {
                LogHelper.Info<UmbracoMembershipProviderBase>(
                    string.Format(
                        "Login attempt failed for username {0} from IP address {1}, the user does not exist",
                        username,
                        GetCurrentRequestIpAddress()));

                return false;
            }

            if (member.IsApproved == false)
            {
                LogHelper.Info<UmbracoMembershipProviderBase>(
                    string.Format(
                        "Login attempt failed for username {0} from IP address {1}, the user is not approved",
                        username,
                        GetCurrentRequestIpAddress()));

                return false;
            }
            if (member.IsLockedOut)
            {
                LogHelper.Info<UmbracoMembershipProviderBase>(
                    string.Format(
                        "Login attempt failed for username {0} from IP address {1}, the user is locked",
                        username,
                        GetCurrentRequestIpAddress()));

                return false;
            }

            var authenticated = CheckPassword(password, member.RawPasswordValue);

            if (authenticated == false)
            {
                // TODO: Increment login attempts - lock if too many.

                var count = member.FailedPasswordAttempts;
                count++;
                member.FailedPasswordAttempts = count;

                if (count >= MaxInvalidPasswordAttempts)
                {
                    member.IsLockedOut = true;
                    member.LastLockoutDate = DateTime.Now;

                    LogHelper.Info<UmbracoMembershipProviderBase>(
                        string.Format(
                            "Login attempt failed for username {0} from IP address {1}, the user is now locked out, max invalid password attempts exceeded",
                            username,
                            GetCurrentRequestIpAddress()));
                }
                else
                {
                    LogHelper.Info<UmbracoMembershipProviderBase>(
                        string.Format(
                            "Login attempt failed for username {0} from IP address {1}",
                            username,
                            GetCurrentRequestIpAddress()));
                }
            }
            else
            {
                member.FailedPasswordAttempts = 0;
                member.LastLoginDate = DateTime.Now;

                LogHelper.Info<UmbracoMembershipProviderBase>(
                        string.Format(
                            "Login attempt succeeded for username {0} from IP address {1}",
                            username,
                            GetCurrentRequestIpAddress()));
            }

            //don't raise events for this! It just sets the member dates, if we do raise events this will
            // cause all distributed cache to execute - which will clear out some caches we don't want.
            // http://issues.umbraco.org/issue/U4-3451
            //TODO: In v8 we aren't going to have an overload to disable events, so we'll need to make a different method
            // for this type of thing (i.e. UpdateLastLogin or similar).

            // when upgrating from 7.2 to 7.3 trying to save will throw
            if (UmbracoVersion.Current >= new Version(7, 3, 0, 0))
                MemberService.Save(member, false);

            return authenticated;
        }



        

    }
}