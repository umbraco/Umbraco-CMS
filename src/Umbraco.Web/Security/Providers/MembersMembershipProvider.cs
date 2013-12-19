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
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Custom Membership Provider for Umbraco Members (User authentication for Frontend applications NOT umbraco CMS)  
    /// </summary>
    internal class MembersMembershipProvider : UmbracoMembershipProviderBase
    {
        private IMembershipMemberService _memberService;

        protected IMembershipMemberService MemberService
        {
            get { return _memberService ?? (_memberService = ApplicationContext.Current.Services.MemberService); }
        }

        public string ProviderName 
        {
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

            //// test for membertype (if not specified, choose the first member type available)
            //if (config["defaultMemberTypeAlias"] != null)
            //    _defaultMemberTypeAlias = config["defaultMemberTypeAlias"];
            //else if (MemberType.GetAll.Length == 1)
            //    _defaultMemberTypeAlias = MemberType.GetAll[0].Alias;
            //else
            //    throw new ProviderException("No default MemberType alias is specified in the web.config string. Please add a 'defaultMemberTypeAlias' to the add element in the provider declaration in web.config");

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
            var m = _memberService.GetByUsername(username);
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

            m.Password = FormatPasswordForStorage(encodedPassword, salt);
            m.LastPasswordChangeDate = DateTime.Now;
            
            _memberService.Save(m);

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
            member.PasswordAnswer = newPasswordAnswer;

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

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(password, out salt);

            var member = MemberService.CreateMember(email, username, encodedPassword, memberTypeAlias);
            
            member.PasswordQuestion = passwordQuestion;
            member.PasswordAnswer = passwordAnswer;
            member.IsApproved = isApproved;
            member.LastLoginDate = DateTime.Now;
            member.LastPasswordChangeDate = DateTime.Now;

            MemberService.Save(member);

            status = MembershipCreateStatus.Success;
            return member.AsConcreteMembershipUser();
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
            var byEmail = MemberService.FindMembersByEmail(emailToMatch, StringPropertyMatchType.Wildcard).ToArray();
            totalRecords = byEmail.Length;
            var pagedResult = new PagedResult<IMember>(totalRecords, pageIndex, pageSize);

            var collection = new MembershipUserCollection();
            foreach (var m in byEmail.Skip(pagedResult.SkipSize).Take(pageSize))
            {
                collection.Add(m.AsConcreteMembershipUser());
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
            var byEmail = MemberService.FindMembersByUsername(usernameToMatch, StringPropertyMatchType.Wildcard).ToArray();
            totalRecords = byEmail.Length;
            var pagedResult = new PagedResult<IMember>(totalRecords, pageIndex, pageSize);

            var collection = new MembershipUserCollection();
            foreach (var m in byEmail.Skip(pagedResult.SkipSize).Take(pageSize))
            {
                collection.Add(m.AsConcreteMembershipUser());
            }
            return collection;
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

            var member = MemberService.GetByUsername(username);

            if (RequiresQuestionAndAnswer && member.PasswordAnswer != answer)
            {
                throw new ProviderException("Password retrieval answer doesn't match");
            } 

            return member.Password;
        }
      
        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user (not used with Umbraco).</param>
        /// <returns>The new password for the specified user.</returns>
        public override string ResetPassword(string username, string answer)
        {
            //TODO: Get logic from other provider

            throw new NotImplementedException();

            //if (EnablePasswordReset == false)
            //    throw new ProviderException("Password reset is Not Enabled.");

            //var member = MemberService.GetByUsername(username);

            //if (member == null)
            //    throw new ProviderException("The supplied user is not found");

            //if(member.IsLockedOut)
            //    throw new ProviderException("The member is locked out.");

            //if (RequiresQuestionAndAnswer == false || (RequiresQuestionAndAnswer && answer == member.PasswordAnswer))
            //{
            //    member.Password =
            //        EncryptOrHashPassword(Membership.GeneratePassword(MinRequiredPasswordLength,
            //                                                   MinRequiredNonAlphanumericCharacters));
            //    MemberService.Save(member);
            //}
            //else
            //{
            //    throw new MembershipPasswordException("Incorrect password answer");
            //}
            
            //return null;
        }
        
        /// <summary>
        /// Updates e-mail and potentially approved status, lock status and comment on a user.
        /// Note: To automatically support lock, approve and comments you'll need to add references to the membertype properties in the 
        /// 'Member' element in web.config by adding their aliases to the 'umbracoApprovePropertyTypeAlias', 'umbracoLockPropertyTypeAlias' and 'umbracoCommentPropertyTypeAlias' attributes
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>      
        public override void UpdateUser(MembershipUser user)
        {
            //var member = user.AsIMember();
            //MemberService.Save(member);
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

            string salt;
            var encodedPassword = EncryptOrHashNewPassword(password, out salt);
            
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
        

        public override string ToString()
        {
            var result = base.ToString();
            var sb = new StringBuilder(result);
            sb.AppendLine("DefaultMemberTypeAlias=" + DefaultMemberTypeAlias);
            return sb.ToString();
        }

    }
}