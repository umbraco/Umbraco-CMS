using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using IUser = Umbraco.Core.Models.Membership.IUser;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Core.Security
{
    public class BackOfficeUserStore : DisposableObjectSlim,
        IUserStore<BackOfficeIdentityUser, int>,
        IUserPasswordStore<BackOfficeIdentityUser, int>,
        IUserEmailStore<BackOfficeIdentityUser, int>,
        IUserLoginStore<BackOfficeIdentityUser, int>,
        IUserRoleStore<BackOfficeIdentityUser, int>,
        IUserSecurityStampStore<BackOfficeIdentityUser, int>,
        IUserLockoutStore<BackOfficeIdentityUser, int>,
        IUserTwoFactorStore<BackOfficeIdentityUser, int>,
        IUserSessionStore<BackOfficeIdentityUser, int>

    // TODO: This would require additional columns/tables for now people will need to implement this on their own
    //IUserPhoneNumberStore<BackOfficeIdentityUser, int>,
    // TODO: To do this we need to implement IQueryable -  we'll have an IQuerable implementation soon with the UmbracoLinqPadDriver implementation
    //IQueryableUserStore<BackOfficeIdentityUser, int>
    {
        private readonly IUserService _userService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IEntityService _entityService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly IGlobalSettings _globalSettings;
        private readonly UmbracoMapper _mapper;
        private readonly AppCaches _appCaches;
        private bool _disposed = false;

        [Obsolete("Use the constructor specifying all dependencies")]
        public BackOfficeUserStore(IUserService userService, IMemberTypeService memberTypeService, IEntityService entityService, IExternalLoginService externalLoginService, IGlobalSettings globalSettings, MembershipProviderBase usersMembershipProvider, UmbracoMapper mapper)
            : this(userService, memberTypeService, entityService, externalLoginService, globalSettings, usersMembershipProvider, mapper, Current.AppCaches) { }

        public BackOfficeUserStore(IUserService userService, IMemberTypeService memberTypeService, IEntityService entityService, IExternalLoginService externalLoginService, IGlobalSettings globalSettings, MembershipProviderBase usersMembershipProvider, UmbracoMapper mapper, AppCaches appCaches)
        {
            if (userService == null) throw new ArgumentNullException("userService");
            if (usersMembershipProvider == null) throw new ArgumentNullException("usersMembershipProvider");
            if (externalLoginService == null) throw new ArgumentNullException("externalLoginService");

            _userService = userService;
            _memberTypeService = memberTypeService;
            _entityService = entityService;
            _externalLoginService = externalLoginService;
            _globalSettings = globalSettings;            
            _mapper = mapper;
            _appCaches = appCaches;
            _userService = userService;
            _externalLoginService = externalLoginService;

            if (usersMembershipProvider.PasswordFormat != MembershipPasswordFormat.Hashed)
            {
                throw new InvalidOperationException("Cannot use ASP.Net Identity with UmbracoMembersUserStore when the password format is not Hashed");
            }
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObjectSlim"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _disposed = true;
        }

        /// <summary>
        /// Insert a new user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task CreateAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            //the password must be 'something' it could be empty if authenticating
            // with an external provider so we'll just generate one and prefix it, the
            // prefix will help us determine if the password hasn't actually been specified yet.
            //this will hash the guid with a salt so should be nicely random
            var aspHasher = new PasswordHasher();
            var emptyPasswordValue = Constants.Security.EmptyPasswordPrefix +
                                      aspHasher.HashPassword(Guid.NewGuid().ToString("N"));

            var userEntity = new User(user.Name, user.Email, user.UserName, emptyPasswordValue)
            {
                DefaultToLiveEditing = false,
                Language = user.Culture ?? _globalSettings.DefaultUILanguage,
                StartContentIds = user.StartContentIds ?? new int[] { },
                StartMediaIds = user.StartMediaIds ?? new int[] { },
                IsLockedOut = user.IsLockedOut,
            };

            // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
            var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.Logins));

            UpdateMemberProperties(userEntity, user);

            _userService.Save(userEntity);

            if (!userEntity.HasIdentity) throw new DataException("Could not create the user, check logs for details");

            //re-assign id
            user.Id = userEntity.Id;

            if (isLoginsPropertyDirty)
            {
                _externalLoginService.Save(
                    user.Id,
                    user.Logins.Select(x => new ExternalLogin(
                        x.LoginProvider,
                        x.ProviderKey,
                        (x is IIdentityUserLoginExtended extended) ? extended.UserData : null)));
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task UpdateAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            var asInt = user.Id.TryConvertTo<int>();
            if (asInt == false)
            {
                throw new InvalidOperationException("The user id must be an integer to work with the Umbraco");
            }

            // TODO: Wrap this in a scope!

            var found = _userService.GetUserById(asInt.Result);
            if (found != null)
            {
                // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
                var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.Logins));

                if (UpdateMemberProperties(found, user))
                {
                    _userService.Save(found);
                }

                if (isLoginsPropertyDirty)
                {
                    _externalLoginService.Save(
                        found.Id,
                        user.Logins.Select(x => new ExternalLogin(
                            x.LoginProvider,
                            x.ProviderKey,
                            (x is IIdentityUserLoginExtended extended) ? extended.UserData : null)));                    
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task DeleteAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            var asInt = user.Id.TryConvertTo<int>();
            if (asInt == false)
            {
                throw new InvalidOperationException("The user id must be an integer to work with the Umbraco");
            }

            var found = _userService.GetUserById(asInt.Result);
            if (found != null)
            {
                _userService.Delete(found);
            }
            _externalLoginService.DeleteUserLogins(asInt.Result);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Finds a user
        /// </summary>
        /// <param name="userId"/>
        /// <returns/>
        public async Task<BackOfficeIdentityUser> FindByIdAsync(int userId)
        {
            ThrowIfDisposed();
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return null;
            }

            return await Task.FromResult(AssignLoginsCallback(_mapper.Map<BackOfficeIdentityUser>(user)));
        }

        /// <summary>
        /// Find a user by name
        /// </summary>
        /// <param name="userName"/>
        /// <returns/>
        public async Task<BackOfficeIdentityUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            var user = _userService.GetByUsername(userName);
            if (user == null)
            {
                return null;
            }

            var result = AssignLoginsCallback(_mapper.Map<BackOfficeIdentityUser>(user));

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Set the user password hash
        /// </summary>
        /// <param name="user"/><param name="passwordHash"/>
        /// <returns/>
        public Task SetPasswordHashAsync(BackOfficeIdentityUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (passwordHash == null) throw new ArgumentNullException(nameof(passwordHash));
            if (string.IsNullOrEmpty(passwordHash)) throw new ArgumentException("Value can't be empty.", nameof(passwordHash));

            user.PasswordHash = passwordHash;

            return Task.FromResult(0);
        }

        /// <summary>
        /// Get the user password hash
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<string> GetPasswordHashAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        /// Returns true if a user has a password set
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<bool> HasPasswordAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(string.IsNullOrEmpty(user.PasswordHash) == false);
        }

        /// <summary>
        /// Set the user email
        /// </summary>
        /// <param name="user"/><param name="email"/>
        /// <returns/>
        public Task SetEmailAsync(BackOfficeIdentityUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (email.IsNullOrWhiteSpace()) throw new ArgumentNullException("email");

            user.Email = email;

            return Task.FromResult(0);
        }

        /// <summary>
        /// Get the user email
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<string> GetEmailAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Email);
        }

        /// <summary>
        /// Returns true if the user email is confirmed
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<bool> GetEmailConfirmedAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.EmailConfirmed);
        }

        /// <summary>
        /// Sets whether the user email is confirmed
        /// </summary>
        /// <param name="user"/><param name="confirmed"/>
        /// <returns/>
        public Task SetEmailConfirmedAsync(BackOfficeIdentityUser user, bool confirmed)
        {
            ThrowIfDisposed();
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns the user associated with this email
        /// </summary>
        /// <param name="email"/>
        /// <returns/>
        public Task<BackOfficeIdentityUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            var user = _userService.GetByEmail(email);
            var result = user == null
                ? null
                : _mapper.Map<BackOfficeIdentityUser>(user);

            return Task.FromResult(AssignLoginsCallback(result));
        }

        /// <summary>
        /// Adds a user login with the specified provider and key
        /// </summary>
        /// <param name="user"/><param name="login"/>
        /// <returns/>
        public Task AddLoginAsync(BackOfficeIdentityUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));

            var logins = user.Logins;
            var instance = new IdentityUserLogin(login.LoginProvider, login.ProviderKey, user.Id);
            var userLogin = instance;
            logins.Add(userLogin);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Removes the user login with the specified combination if it exists
        /// </summary>
        /// <param name="user"/><param name="login"/>
        /// <returns/>
        public Task RemoveLoginAsync(BackOfficeIdentityUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));

            var provider = login.LoginProvider;
            var key = login.ProviderKey;
            var userLogin = user.Logins.SingleOrDefault((l => l.LoginProvider == provider && l.ProviderKey == key));
            if (userLogin != null)
                user.Logins.Remove(userLogin);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns the linked accounts for this user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult((IList<UserLoginInfo>)
                user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList());
        }

        /// <summary>
        /// Returns the user associated with this login
        /// </summary>
        /// <returns/>
        public Task<BackOfficeIdentityUser> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (login == null) throw new ArgumentNullException(nameof(login));

            //get all logins associated with the login id
            var result = _externalLoginService.Find(login.LoginProvider, login.ProviderKey).ToArray();
            if (result.Any())
            {
                //return the first user that matches the result
                BackOfficeIdentityUser output = null;
                foreach (var l in result)
                {
                    var user = _userService.GetUserById(l.UserId);
                    if (user != null)
                    {
                        output = _mapper.Map<BackOfficeIdentityUser>(user);
                        break;
                    }
                }

                return Task.FromResult(AssignLoginsCallback(output));
            }

            return Task.FromResult<BackOfficeIdentityUser>(null);
        }


        /// <summary>
        /// Adds a user to a role (user group)
        /// </summary>
        /// <param name="user"/><param name="roleName"/>
        /// <returns/>
        public Task AddToRoleAsync(BackOfficeIdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(roleName));

            var userRole = user.Roles.SingleOrDefault(r => r.RoleId == roleName);

            if (userRole == null)
            {
                user.AddRole(roleName);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Removes the role (user group) for the user
        /// </summary>
        /// <param name="user"/><param name="roleName"/>
        /// <returns/>
        public Task RemoveFromRoleAsync(BackOfficeIdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(roleName));

            var userRole = user.Roles.SingleOrDefault(r => r.RoleId == roleName);

            if (userRole != null)
            {
                user.Roles.Remove(userRole);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns the roles (user groups) for this user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<IList<string>> GetRolesAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult((IList<string>)user.Roles.Select(x => x.RoleId).ToList());
        }

        /// <summary>
        /// Returns true if a user is in the role
        /// </summary>
        /// <param name="user"/><param name="roleName"/>
        /// <returns/>
        public Task<bool> IsInRoleAsync(BackOfficeIdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Roles.Select(x => x.RoleId).InvariantContains(roleName));
        }

        /// <summary>
        /// Set the security stamp for the user
        /// </summary>
        /// <param name="user"/><param name="stamp"/>
        /// <returns/>
        public Task SetSecurityStampAsync(BackOfficeIdentityUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Get the user security stamp
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<string> GetSecurityStampAsync(BackOfficeIdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));

            //the stamp cannot be null, so if it is currently null then we'll just return a hash of the password
            return Task.FromResult(user.SecurityStamp.IsNullOrWhiteSpace()
                ? user.PasswordHash.GenerateHash()
                : user.SecurityStamp);
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

        /// <summary>
        /// Sets whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"/><param name="enabled"/>
        /// <returns/>
        public virtual Task SetTwoFactorEnabledAsync(BackOfficeIdentityUser user, bool enabled)
        {
            user.TwoFactorEnabled = false;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public virtual Task<bool> GetTwoFactorEnabledAsync(BackOfficeIdentityUser user)
        {
            return Task.FromResult(false);
        }

        #region IUserLockoutStore

        /// <summary>
        /// Returns the DateTimeOffset that represents the end of a user's lockout, any time in the past should be considered not locked out.
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        /// <remarks>
        /// Currently we do not support a timed lock out, when they are locked out, an admin will  have to reset the status
        /// </remarks>
        public Task<DateTimeOffset> GetLockoutEndDateAsync(BackOfficeIdentityUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return user.LockoutEndDateUtc.HasValue
                ? Task.FromResult(DateTimeOffset.MaxValue)
                : Task.FromResult(DateTimeOffset.MinValue);
        }

        /// <summary>
        /// Locks a user out until the specified end date (set to a past date, to unlock a user)
        /// </summary>
        /// <param name="user"/><param name="lockoutEnd"/>
        /// <returns/>
        /// <remarks>
        /// Currently we do not support a timed lock out, when they are locked out, an admin will  have to reset the status
        /// </remarks>
        public Task SetLockoutEndDateAsync(BackOfficeIdentityUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Used to record when an attempt to access the user has failed
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<int> IncrementAccessFailedCountAsync(BackOfficeIdentityUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        /// Used to reset the access failed count, typically after the account is successfully accessed
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task ResetAccessFailedCountAsync(BackOfficeIdentityUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns the current number of failed access attempts.  This number usually will be reset whenever the password is
        ///                 verified or the account is locked out.
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<int> GetAccessFailedCountAsync(BackOfficeIdentityUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        /// Returns true
        /// </summary>
        /// <param name="user"/>
        /// <returns/>
        public Task<bool> GetLockoutEnabledAsync(BackOfficeIdentityUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <summary>
        /// Doesn't actually perform any function, users can always be locked out
        /// </summary>
        /// <param name="user"/><param name="enabled"/>
        /// <returns/>
        public Task SetLockoutEnabledAsync(BackOfficeIdentityUser user, bool enabled)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }
        #endregion

        private bool UpdateMemberProperties(IUser user, BackOfficeIdentityUser identityUser)
        {
            var anythingChanged = false;

            //don't assign anything if nothing has changed as this will trigger the track changes of the model

            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastLoginDateUtc))
                || (user.LastLoginDate != default(DateTime) && identityUser.LastLoginDateUtc.HasValue == false)
                || identityUser.LastLoginDateUtc.HasValue && user.LastLoginDate.ToUniversalTime() != identityUser.LastLoginDateUtc.Value)
            {
                anythingChanged = true;
                //if the LastLoginDate is being set to MinValue, don't convert it ToLocalTime
                var dt = identityUser.LastLoginDateUtc == DateTime.MinValue ? DateTime.MinValue : identityUser.LastLoginDateUtc.Value.ToLocalTime();
                user.LastLoginDate = dt;
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastPasswordChangeDateUtc))
                || (user.LastPasswordChangeDate != default(DateTime) && identityUser.LastPasswordChangeDateUtc.HasValue == false)
                || identityUser.LastPasswordChangeDateUtc.HasValue && user.LastPasswordChangeDate.ToUniversalTime() != identityUser.LastPasswordChangeDateUtc.Value)
            {
                anythingChanged = true;
                user.LastPasswordChangeDate = identityUser.LastPasswordChangeDateUtc.Value.ToLocalTime();
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.EmailConfirmed))
                || (user.EmailConfirmedDate.HasValue && user.EmailConfirmedDate.Value != default(DateTime) && identityUser.EmailConfirmed == false)
                || ((user.EmailConfirmedDate.HasValue == false || user.EmailConfirmedDate.Value == default(DateTime)) && identityUser.EmailConfirmed))
            {
                anythingChanged = true;
                user.EmailConfirmedDate = identityUser.EmailConfirmed ? (DateTime?)DateTime.Now : null;
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Name))
                && user.Name != identityUser.Name && identityUser.Name.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                user.Name = identityUser.Name;
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Email))
                && user.Email != identityUser.Email && identityUser.Email.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                user.Email = identityUser.Email;
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.AccessFailedCount))
                && user.FailedPasswordAttempts != identityUser.AccessFailedCount)
            {
                anythingChanged = true;
                user.FailedPasswordAttempts = identityUser.AccessFailedCount;
            }
            if (user.IsLockedOut != identityUser.IsLockedOut)
            {
                anythingChanged = true;
                user.IsLockedOut = identityUser.IsLockedOut;

                if (user.IsLockedOut)
                {
                    //need to set the last lockout date
                    user.LastLockoutDate = DateTime.Now;
                }

            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.UserName))
                && user.Username != identityUser.UserName && identityUser.UserName.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                user.Username = identityUser.UserName;
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.PasswordHash))
                && user.RawPasswordValue != identityUser.PasswordHash && identityUser.PasswordHash.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                user.RawPasswordValue = identityUser.PasswordHash;
            }

            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Culture))
                && user.Language != identityUser.Culture && identityUser.Culture.IsNullOrWhiteSpace() == false)
            {
                anythingChanged = true;
                user.Language = identityUser.Culture;
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.StartMediaIds))
                && user.StartMediaIds.UnsortedSequenceEqual(identityUser.StartMediaIds) == false)
            {
                anythingChanged = true;
                user.StartMediaIds = identityUser.StartMediaIds;
            }
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.StartContentIds))
                && user.StartContentIds.UnsortedSequenceEqual(identityUser.StartContentIds) == false)
            {
                anythingChanged = true;
                user.StartContentIds = identityUser.StartContentIds;
            }
            if (user.SecurityStamp != identityUser.SecurityStamp)
            {
                anythingChanged = true;
                user.SecurityStamp = identityUser.SecurityStamp;
            }

            // TODO: Fix this for Groups too
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Roles)) || identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Groups)))
            {
                var userGroupAliases = user.Groups.Select(x => x.Alias).ToArray();

                var identityUserRoles = identityUser.Roles.Select(x => x.RoleId).ToArray();
                var identityUserGroups = identityUser.Groups.Select(x => x.Alias).ToArray();

                var combinedAliases = identityUserRoles.Union(identityUserGroups).ToArray();

                if (userGroupAliases.ContainsAll(combinedAliases) == false
                    || combinedAliases.ContainsAll(userGroupAliases) == false)
                {
                    anythingChanged = true;

                    //clear out the current groups (need to ToArray since we are modifying the iterator)
                    user.ClearGroups();

                    //go lookup all these groups
                    var groups = _userService.GetUserGroupsByAlias(combinedAliases).Select(x => x.ToReadOnlyGroup()).ToArray();

                    //use all of the ones assigned and add them
                    foreach (var group in groups)
                    {
                        user.AddGroup(group);
                    }

                    //re-assign
                    identityUser.Groups = groups;
                }
            }

            //we should re-set the calculated start nodes
            identityUser.CalculatedMediaStartNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
            identityUser.CalculatedContentStartNodeIds = user.CalculateContentStartNodeIds(_entityService, _appCaches);

            //reset all changes
            identityUser.ResetDirtyProperties(false);

            return anythingChanged;
        }


        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public Task<bool> ValidateSessionIdAsync(int userId, string sessionId)
        {
            Guid guidSessionId;
            if (Guid.TryParse(sessionId, out guidSessionId))
            {
                return Task.FromResult(_userService.ValidateLoginSession(userId, guidSessionId));
            }
            return Task.FromResult(false);
        }
    }
}
