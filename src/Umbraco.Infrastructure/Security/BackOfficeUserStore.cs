using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Infrastructure.Security
{
    // TODO: Make this into a base class that can be re-used

    /// <summary>
    /// The user store for back office users
    /// </summary>
    public class BackOfficeUserStore : UserStoreBase<BackOfficeIdentityUser, IdentityRole<string>, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly GlobalSettings _globalSettings;
        private readonly UmbracoMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeUserStore"/> class.
        /// </summary>
        public BackOfficeUserStore(
            IScopeProvider scopeProvider,
            IUserService userService,
            IEntityService entityService,
            IExternalLoginService externalLoginService,
            IOptions<GlobalSettings> globalSettings,
            UmbracoMapper mapper,
            IdentityErrorDescriber describer)
            : base(describer)
        {
            _scopeProvider = scopeProvider;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _entityService = entityService;
            _externalLoginService = externalLoginService ?? throw new ArgumentNullException(nameof(externalLoginService));
            _globalSettings = globalSettings.Value;
            _mapper = mapper;
            _userService = userService;
            _externalLoginService = externalLoginService;
        }

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IQueryable<BackOfficeIdentityUser> Users => throw new NotImplementedException();

        /// <inheritdoc />
        public override Task<string> GetNormalizedUserNameAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken) => GetUserNameAsync(user, cancellationToken);

        /// <inheritdoc />
        public override Task SetNormalizedUserNameAsync(BackOfficeIdentityUser user, string normalizedName, CancellationToken cancellationToken) => SetUserNameAsync(user, normalizedName, cancellationToken);

        /// <inheritdoc />
        public override Task<IdentityResult> CreateAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // the password must be 'something' it could be empty if authenticating
            // with an external provider so we'll just generate one and prefix it, the
            // prefix will help us determine if the password hasn't actually been specified yet.
            // this will hash the guid with a salt so should be nicely random
            var aspHasher = new PasswordHasher<BackOfficeIdentityUser>();
            var emptyPasswordValue = Constants.Security.EmptyPasswordPrefix +
                                      aspHasher.HashPassword(user, Guid.NewGuid().ToString("N"));

            var userEntity = new User(_globalSettings, user.Name, user.Email, user.UserName, emptyPasswordValue)
            {
                Language = user.Culture ?? _globalSettings.DefaultUILanguage,
                StartContentIds = user.StartContentIds ?? new int[] { },
                StartMediaIds = user.StartMediaIds ?? new int[] { },
                IsLockedOut = user.IsLockedOut,
            };

            // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
            var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.Logins));

            UpdateMemberProperties(userEntity, user);

            _userService.Save(userEntity);

            if (!userEntity.HasIdentity)
            {
                throw new DataException("Could not create the user, check logs for details");
            }

            // re-assign id
            user.Id = UserIdToString(userEntity.Id);

            if (isLoginsPropertyDirty)
            {
                _externalLoginService.Save(
                    userEntity.Id,
                    user.Logins.Select(x => new ExternalLogin(
                        x.LoginProvider,
                        x.ProviderKey,
                        x.UserData)));
            }

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<IdentityResult> UpdateAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Attempt<int> asInt = user.Id.TryConvertTo<int>();
            if (asInt == false)
            {
                throw new InvalidOperationException("The user id must be an integer to work with the Umbraco");
            }

            using (IScope scope = _scopeProvider.CreateScope())
            {
                IUser found = _userService.GetUserById(asInt.Result);
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
                                x.UserData)));
                    }
                }

                scope.Complete();
            }

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<IdentityResult> DeleteAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IUser found = _userService.GetUserById(UserIdToInt(user.Id));
            if (found != null)
            {
                _userService.Delete(found);
            }

            _externalLoginService.DeleteUserLogins(UserIdToInt(user.Id));

            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override Task<BackOfficeIdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default) => FindUserAsync(userId, cancellationToken);

        /// <inheritdoc />
        protected override Task<BackOfficeIdentityUser> FindUserAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            IUser user = _userService.GetUserById(UserIdToInt(userId));
            if (user == null)
            {
                return Task.FromResult((BackOfficeIdentityUser)null);
            }

            return Task.FromResult(AssignLoginsCallback(_mapper.Map<BackOfficeIdentityUser>(user)));
        }

        /// <inheritdoc />
        public override Task<BackOfficeIdentityUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            IUser user = _userService.GetByUsername(userName);
            if (user == null)
            {
                return Task.FromResult((BackOfficeIdentityUser)null);
            }

            BackOfficeIdentityUser result = AssignLoginsCallback(_mapper.Map<BackOfficeIdentityUser>(user));

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override async Task SetPasswordHashAsync(BackOfficeIdentityUser user, string passwordHash, CancellationToken cancellationToken = default)
        {
            await base.SetPasswordHashAsync(user, passwordHash, cancellationToken);

            user.PasswordConfig = null; // Clear this so that it's reset at the repository level
            user.LastPasswordChangeDateUtc = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override async Task<bool> HasPasswordAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default)
        {
            // This checks if it's null
            var result = await base.HasPasswordAsync(user, cancellationToken);
            if (result)
            {
                // we also want to check empty
                return string.IsNullOrEmpty(user.PasswordHash) == false;
            }

            return result;
        }

        /// <inheritdoc />
        public override Task<BackOfficeIdentityUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            IUser user = _userService.GetByEmail(email);
            BackOfficeIdentityUser result = user == null
                ? null
                : _mapper.Map<BackOfficeIdentityUser>(user);

            return Task.FromResult(AssignLoginsCallback(result));
        }

        /// <inheritdoc />
        public override Task<string> GetNormalizedEmailAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken)
            => GetEmailAsync(user, cancellationToken);

        /// <inheritdoc />
        public override Task SetNormalizedEmailAsync(BackOfficeIdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
            => SetEmailAsync(user, normalizedEmail, cancellationToken);

        /// <inheritdoc />
        public override Task AddLoginAsync(BackOfficeIdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            ICollection<IIdentityUserLogin> logins = user.Logins;
            var instance = new IdentityUserLogin(login.LoginProvider, login.ProviderKey, user.Id.ToString());
            IdentityUserLogin userLogin = instance;
            logins.Add(userLogin);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task RemoveLoginAsync(BackOfficeIdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IIdentityUserLogin userLogin = user.Logins.SingleOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
            if (userLogin != null)
            {
                user.Logins.Remove(userLogin);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task<IList<UserLoginInfo>> GetLoginsAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult((IList<UserLoginInfo>)user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.LoginProvider)).ToList());
        }

        /// <inheritdoc />
        protected override async Task<IdentityUserLogin<string>> FindUserLoginAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            BackOfficeIdentityUser user = await FindUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            IList<UserLoginInfo> logins = await GetLoginsAsync(user, cancellationToken);
            UserLoginInfo found = logins.FirstOrDefault(x => x.ProviderKey == providerKey && x.LoginProvider == loginProvider);
            if (found == null)
            {
                return null;
            }

            return new IdentityUserLogin<string>
            {
                LoginProvider = found.LoginProvider,
                ProviderKey = found.ProviderKey,
                ProviderDisplayName = found.ProviderDisplayName, // TODO: We don't store this value so it will be null
                UserId = user.Id
            };
        }

        /// <inheritdoc />
        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var logins = _externalLoginService.Find(loginProvider, providerKey).ToList();
            if (logins.Count == 0)
            {
                return Task.FromResult((IdentityUserLogin<string>)null);
            }

            IIdentityUserLogin found = logins[0];
            return Task.FromResult(new IdentityUserLogin<string>
            {
                LoginProvider = found.LoginProvider,
                ProviderKey = found.ProviderKey,
                ProviderDisplayName = null, // TODO: We don't store this value so it will be null
                UserId = found.UserId
            });
        }

        /// <summary>
        /// Adds a user to a role (user group)
        /// </summary>
        public override Task AddToRoleAsync(BackOfficeIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(normalizedRoleName));
            }

            IdentityUserRole<string> userRole = user.Roles.SingleOrDefault(r => r.RoleId == normalizedRoleName);

            if (userRole == null)
            {
                user.AddRole(normalizedRoleName);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the role (user group) for the user
        /// </summary>
        public override Task RemoveFromRoleAsync(BackOfficeIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(normalizedRoleName));
            }

            IdentityUserRole<string> userRole = user.Roles.SingleOrDefault(r => r.RoleId == normalizedRoleName);

            if (userRole != null)
            {
                user.Roles.Remove(userRole);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the roles (user groups) for this user
        /// </summary>
        public override Task<IList<string>> GetRolesAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult((IList<string>)user.Roles.Select(x => x.RoleId).ToList());
        }

        /// <summary>
        /// Returns true if a user is in the role
        /// </summary>
        public override Task<bool> IsInRoleAsync(BackOfficeIdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Roles.Select(x => x.RoleId).InvariantContains(normalizedRoleName));
        }

        /// <summary>
        /// Lists all users of a given role.
        /// </summary>
        /// <remarks>
        ///     Identity Role names are equal to Umbraco UserGroup alias.
        /// </remarks>
        public override Task<IList<BackOfficeIdentityUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            IUserGroup userGroup = _userService.GetUserGroupByAlias(normalizedRoleName);

            IEnumerable<IUser> users = _userService.GetAllInGroup(userGroup.Id);
            IList<BackOfficeIdentityUser> backOfficeIdentityUsers = users.Select(x => _mapper.Map<BackOfficeIdentityUser>(x)).ToList();

            return Task.FromResult(backOfficeIdentityUsers);
        }

        /// <inheritdoc/>
        protected override Task<IdentityRole<string>> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            IUserGroup group = _userService.GetUserGroupByAlias(normalizedRoleName);
            if (group == null)
            {
                return Task.FromResult((IdentityRole<string>)null);
            }

            return Task.FromResult(new IdentityRole<string>(group.Name)
            {
                Id = group.Alias
            });
        }

        /// <inheritdoc/>
        protected override async Task<IdentityUserRole<string>> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
        {
            BackOfficeIdentityUser user = await FindUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            IdentityUserRole<string> found = user.Roles.FirstOrDefault(x => x.RoleId.InvariantEquals(roleId));
            return found;
        }

        /// <inheritdoc />
        public override Task<string> GetSecurityStampAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // the stamp cannot be null, so if it is currently null then we'll just return a hash of the password
            return Task.FromResult(user.SecurityStamp.IsNullOrWhiteSpace()
                ? user.PasswordHash.GenerateHash()
                : user.SecurityStamp);
        }

        private BackOfficeIdentityUser AssignLoginsCallback(BackOfficeIdentityUser user)
        {
            if (user != null)
            {
                user.SetLoginsCallback(new Lazy<IEnumerable<IIdentityUserLogin>>(() => _externalLoginService.GetAll(UserIdToInt(user.Id))));
            }

            return user;
        }

        private bool UpdateMemberProperties(IUser user, BackOfficeIdentityUser identityUser)
        {
            var anythingChanged = false;

            // don't assign anything if nothing has changed as this will trigger the track changes of the model
            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastLoginDateUtc))
                || (user.LastLoginDate != default && identityUser.LastLoginDateUtc.HasValue == false)
                || (identityUser.LastLoginDateUtc.HasValue && user.LastLoginDate.ToUniversalTime() != identityUser.LastLoginDateUtc.Value))
            {
                anythingChanged = true;

                // if the LastLoginDate is being set to MinValue, don't convert it ToLocalTime
                DateTime dt = identityUser.LastLoginDateUtc == DateTime.MinValue ? DateTime.MinValue : identityUser.LastLoginDateUtc.Value.ToLocalTime();
                user.LastLoginDate = dt;
            }

            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastPasswordChangeDateUtc))
                || (user.LastPasswordChangeDate != default && identityUser.LastPasswordChangeDateUtc.HasValue == false)
                || (identityUser.LastPasswordChangeDateUtc.HasValue && user.LastPasswordChangeDate.ToUniversalTime() != identityUser.LastPasswordChangeDateUtc.Value))
            {
                anythingChanged = true;
                user.LastPasswordChangeDate = identityUser.LastPasswordChangeDateUtc.Value.ToLocalTime();
            }

            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.EmailConfirmed))
                || (user.EmailConfirmedDate.HasValue && user.EmailConfirmedDate.Value != default && identityUser.EmailConfirmed == false)
                || ((user.EmailConfirmedDate.HasValue == false || user.EmailConfirmedDate.Value == default) && identityUser.EmailConfirmed))
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
                    // need to set the last lockout date
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
                user.PasswordConfiguration = identityUser.PasswordConfig;
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

                    // clear out the current groups (need to ToArray since we are modifying the iterator)
                    user.ClearGroups();

                    // go lookup all these groups
                    var groups = _userService.GetUserGroupsByAlias(combinedAliases).Select(x => x.ToReadOnlyGroup()).ToArray();

                    // use all of the ones assigned and add them
                    foreach (var group in groups)
                    {
                        user.AddGroup(group);
                    }

                    // re-assign
                    identityUser.Groups = groups;
                }
            }

            // we should re-set the calculated start nodes
            identityUser.CalculatedMediaStartNodeIds = user.CalculateMediaStartNodeIds(_entityService);
            identityUser.CalculatedContentStartNodeIds = user.CalculateContentStartNodeIds(_entityService);

            // reset all changes
            identityUser.ResetDirtyProperties(false);

            return anythingChanged;
        }

        /// <inheritdoc />
        public Task<bool> ValidateSessionIdAsync(string userId, string sessionId)
        {
            if (Guid.TryParse(sessionId, out Guid guidSessionId))
            {
                return Task.FromResult(_userService.ValidateLoginSession(UserIdToInt(userId), guidSessionId));
            }

            return Task.FromResult(false);
        }

        private static int UserIdToInt(string userId)
        {
            Attempt<int> attempt = userId.TryConvertTo<int>();
            if (attempt.Success)
            {
                return attempt.Result;
            }

            throw new InvalidOperationException("Unable to convert user ID to int", attempt.Exception);
        }

        private static string UserIdToString(int userId) => string.Intern(userId.ToString());

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task<IList<Claim>> GetClaimsAsync(BackOfficeIdentityUser user, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task AddClaimsAsync(BackOfficeIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task ReplaceClaimAsync(BackOfficeIdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task RemoveClaimsAsync(BackOfficeIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Task<IList<BackOfficeIdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        // TODO: We should support these

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override Task<IdentityUserToken<string>> FindTokenAsync(BackOfficeIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override Task AddUserTokenAsync(IdentityUserToken<string> token) => throw new NotImplementedException();

        /// <summary>
        /// Not supported in Umbraco
        /// </summary>
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override Task RemoveUserTokenAsync(IdentityUserToken<string> token) => throw new NotImplementedException();
    }
}
