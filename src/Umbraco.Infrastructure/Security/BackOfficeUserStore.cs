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
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{

    /// <summary>
    /// The user store for back office users
    /// </summary>
    public class BackOfficeUserStore : UmbracoUserStore<BackOfficeIdentityUser, IdentityRole<string>>
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly GlobalSettings _globalSettings;
        private readonly IUmbracoMapper _mapper;
        private readonly AppCaches _appCaches;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOfficeUserStore"/> class.
        /// </summary>
        public BackOfficeUserStore(
            IScopeProvider scopeProvider,
            IUserService userService,
            IEntityService entityService,
            IExternalLoginService externalLoginService,
            IOptions<GlobalSettings> globalSettings,
            IUmbracoMapper mapper,
            BackOfficeErrorDescriber describer,
            AppCaches appCaches)
            : base(describer)
        {
            _scopeProvider = scopeProvider;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _entityService = entityService;
            _externalLoginService = externalLoginService ?? throw new ArgumentNullException(nameof(externalLoginService));
            _globalSettings = globalSettings.Value;
            _mapper = mapper;
            _appCaches = appCaches;
            _userService = userService;
            _externalLoginService = externalLoginService;
        }

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
            var emptyPasswordValue = Cms.Core.Constants.Security.EmptyPasswordPrefix +
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
            var isTokensPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.LoginTokens));

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

            if (isTokensPropertyDirty)
            {
                _externalLoginService.Save(
                    userEntity.Id,
                    user.LoginTokens.Select(x => new ExternalLoginToken(
                        x.LoginProvider,
                        x.Name,
                        x.Value)));
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
                    var isTokensPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.LoginTokens));

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

                    if (isTokensPropertyDirty)
                    {
                        _externalLoginService.Save(
                            found.Id,
                            user.LoginTokens.Select(x => new ExternalLoginToken(
                                x.LoginProvider,
                                x.Name,
                                x.Value)));
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
        public override async Task SetPasswordHashAsync(BackOfficeIdentityUser user, string passwordHash, CancellationToken cancellationToken = default)
        {
            await base.SetPasswordHashAsync(user, passwordHash, cancellationToken);

            // Clear this so that it's reset at the repository level
            user.PasswordConfig = null;
        }

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

        private BackOfficeIdentityUser AssignLoginsCallback(BackOfficeIdentityUser user)
        {
            if (user != null)
            {
                var userId = UserIdToInt(user.Id);
                user.SetLoginsCallback(new Lazy<IEnumerable<IIdentityUserLogin>>(() => _externalLoginService.GetExternalLogins(userId)));
                user.SetTokensCallback(new Lazy<IEnumerable<IIdentityUserToken>>(() => _externalLoginService.GetExternalLoginTokens(userId)));
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

            if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Roles)))
            {
                var identityUserRoles = identityUser.Roles.Select(x => x.RoleId).ToArray();

                anythingChanged = true;

                // clear out the current groups (need to ToArray since we are modifying the iterator)
                user.ClearGroups();

                // go lookup all these groups
                IReadOnlyUserGroup[] groups = _userService.GetUserGroupsByAlias(identityUserRoles).Select(x => x.ToReadOnlyGroup()).ToArray();

                // use all of the ones assigned and add them
                foreach (IReadOnlyUserGroup group in groups)
                {
                    user.AddGroup(group);
                }

                // re-assign
                identityUser.SetGroups(groups);
            }

            // we should re-set the calculated start nodes
            identityUser.CalculatedMediaStartNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
            identityUser.CalculatedContentStartNodeIds = user.CalculateContentStartNodeIds(_entityService, _appCaches);

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


        /// <summary>
        /// Overridden to support Umbraco's own data storage requirements
        /// </summary>
        /// <remarks>
        /// The base class's implementation of this calls into FindTokenAsync and AddUserTokenAsync, both methods will only work with ORMs that are change
        /// tracking ORMs like EFCore.
        /// </remarks>
        /// <inheritdoc />
        public override Task SetTokenAsync(BackOfficeIdentityUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IIdentityUserToken token = user.LoginTokens.FirstOrDefault(x => x.LoginProvider.InvariantEquals(loginProvider) && x.Name.InvariantEquals(name));
            if (token == null)
            {
                user.LoginTokens.Add(new IdentityUserToken(loginProvider, name, value, user.Id));
            }
            else
            {
                token.Value = value;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Overridden to support Umbraco's own data storage requirements
        /// </summary>
        /// <remarks>
        /// The base class's implementation of this calls into FindTokenAsync, RemoveUserTokenAsync and AddUserTokenAsync, both methods will only work with ORMs that are change
        /// tracking ORMs like EFCore.
        /// </remarks>
        /// <inheritdoc />
        public override Task RemoveTokenAsync(BackOfficeIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IIdentityUserToken token = user.LoginTokens.FirstOrDefault(x => x.LoginProvider.InvariantEquals(loginProvider) && x.Name.InvariantEquals(name));
            if (token != null)
            {
                user.LoginTokens.Remove(token);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Overridden to support Umbraco's own data storage requirements
        /// </summary>
        /// <remarks>
        /// The base class's implementation of this calls into FindTokenAsync, RemoveUserTokenAsync and AddUserTokenAsync, both methods will only work with ORMs that are change
        /// tracking ORMs like EFCore.
        /// </remarks>
        /// <inheritdoc />
        public override Task<string> GetTokenAsync(BackOfficeIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            IIdentityUserToken token = user.LoginTokens.FirstOrDefault(x => x.LoginProvider.InvariantEquals(loginProvider) && x.Name.InvariantEquals(name));

            return Task.FromResult(token?.Value);
        }
    }
}
