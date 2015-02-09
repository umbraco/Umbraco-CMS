using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    public class BackOfficeUserStore : DisposableObject, IUserStore<BackOfficeIdentityUser, int>, IUserPasswordStore<BackOfficeIdentityUser, int>, IUserEmailStore<BackOfficeIdentityUser, int>, IUserLoginStore<BackOfficeIdentityUser, int>
    {
        private readonly IUserService _userService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly MembershipProviderBase _usersMembershipProvider;

        public BackOfficeUserStore(IUserService userService, IExternalLoginService externalLoginService, MembershipProviderBase usersMembershipProvider)
        {
            _userService = userService;
            _externalLoginService = externalLoginService;
            _usersMembershipProvider = usersMembershipProvider;
            if (userService == null) throw new ArgumentNullException("userService");
            if (usersMembershipProvider == null) throw new ArgumentNullException("usersMembershipProvider");
            if (externalLoginService == null) throw new ArgumentNullException("externalLoginService");

            _userService = userService;
            _usersMembershipProvider = usersMembershipProvider;
            _externalLoginService = externalLoginService;

            if (_usersMembershipProvider.PasswordFormat != MembershipPasswordFormat.Hashed)
            {
                throw new InvalidOperationException("Cannot use ASP.Net Identity with UmbracoMembersUserStore when the password format is not Hashed");
            }
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Insert a new user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task CreateAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task UpdateAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task DeleteAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds a user
        /// </summary>
        /// <param name="userId"/>
        /// <returns/>
        public Task<BackOfficeIdentityUser> FindByIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find a user by name
        /// </summary>
        /// <param name="userName"/>
        /// <returns/>
        public Task<BackOfficeIdentityUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the user password hash
        /// </summary>
        /// <param name="user"/><param name="passwordHash"/>
        /// <returns/>
        public Task SetPasswordHashAsync(BackOfficeIdentityUser user, string passwordHash)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the user password hash
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<string> GetPasswordHashAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns true if a user has a password set
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<bool> HasPasswordAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the user email
        /// </summary>
        /// <param name="user"/><param name="email"/>
        /// <returns/>
        public Task SetEmailAsync(BackOfficeIdentityUser user, string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the user email
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<string> GetEmailAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns true if the user email is confirmed
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<bool> GetEmailConfirmedAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets whether the user email is confirmed
        /// </summary>
        /// <param name="user"/><param name="confirmed"/>
        /// <returns/>
        public Task SetEmailConfirmedAsync(BackOfficeIdentityUser user, bool confirmed)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the user associated with this email
        /// </summary>
        /// <param name="email"/>
        /// <returns/>
        public Task<BackOfficeIdentityUser> FindByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a user login with the specified provider and key
        /// </summary>
        /// <param name="user"/><param name="login"/>
        /// <returns/>
        public Task AddLoginAsync(BackOfficeIdentityUser user, UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the user login with the specified combination if it exists
        /// </summary>
        /// <param name="user"/><param name="login"/>
        /// <returns/>
        public Task RemoveLoginAsync(BackOfficeIdentityUser user, UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the linked accounts for this user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(BackOfficeIdentityUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the user associated with this login
        /// </summary>
        /// <returns/>
        public Task<BackOfficeIdentityUser> FindAsync(UserLoginInfo login)
        {
            //get all logins associated with the login id
            var result = _externalLoginService.Find(login).ToArray();
            if (result.Any())
            {
                //return the first member that matches the result
                var user = (from l in result
                            select _userService.GetUserById(l.Id)
                                into member
                                where member != null
                                select new BackOfficeIdentityUser
                                {
                                    Email = member.Email,
                                    Id = member.Id,
                                    LockoutEnabled = member.IsLockedOut,
                                    LockoutEndDateUtc = DateTime.MaxValue.ToUniversalTime(),
                                    UserName = member.Username,
                                    PasswordHash = GetPasswordHash(member.RawPasswordValue)
                                }).FirstOrDefault();

                return Task.FromResult(AssignLoginsCallback(user));
            }

            return Task.FromResult<BackOfficeIdentityUser>(null);
        }

        private BackOfficeIdentityUser AssignLoginsCallback(BackOfficeIdentityUser user)
        {
            if (user != null)
            {
                user.SetLoginsCallback(new Lazy<IEnumerable<IIdentityUserLogin>>(() =>
                            _externalLoginService.GetAll(user.Id)));
            }
            return user;
        }

        private string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith("___UIDEMPTYPWORD__") ? null : storedPass;
        }
    }
}