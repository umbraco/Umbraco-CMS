using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The user store for back office users
/// </summary>
public class BackOfficeUserStore :
    UmbracoUserStore<BackOfficeIdentityUser, IdentityRole<string>>,
    IUserSessionStore<BackOfficeIdentityUser>,
    IBackOfficeUserStore
{
    private readonly AppCaches _appCaches;
    private readonly IEntityService _entityService;
    private readonly IExternalLoginWithKeyService _externalLoginService;
    private readonly GlobalSettings _globalSettings;
    private readonly IUmbracoMapper _mapper;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ITwoFactorLoginService _twoFactorLoginService;
    private readonly IUserGroupService _userGroupService;
    private readonly IUserRepository _userRepository;
    private readonly IRuntimeState _runtimeState;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ILogger<BackOfficeUserStore> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeUserStore" /> class.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public BackOfficeUserStore(
        ICoreScopeProvider scopeProvider,
        IEntityService entityService,
        IExternalLoginWithKeyService externalLoginService,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IUmbracoMapper mapper,
        BackOfficeErrorDescriber describer,
        AppCaches appCaches,
        ITwoFactorLoginService twoFactorLoginService,
        IUserGroupService userGroupService,
        IUserRepository userRepository,
        IRuntimeState runtimeState,
        IEventMessagesFactory eventMessagesFactory,
        ILogger<BackOfficeUserStore> logger)
        : base(describer)
    {
        _scopeProvider = scopeProvider;
        _entityService = entityService;
        _externalLoginService = externalLoginService ?? throw new ArgumentNullException(nameof(externalLoginService));
        _globalSettings = globalSettings.Value;
        _mapper = mapper;
        _appCaches = appCaches;
        _twoFactorLoginService = twoFactorLoginService;
        _userGroupService = userGroupService;
        _userRepository = userRepository;
        _runtimeState = runtimeState;
        _eventMessagesFactory = eventMessagesFactory;
        _logger = logger;
        _externalLoginService = externalLoginService;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateSessionIdAsync(string? userId, string? sessionId)
    {
        if (!Guid.TryParse(sessionId, out Guid guidSessionId))
        {
            return false;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // We need to resolve the id from the key here...
        var id = await ResolveEntityIdFromIdentityId(userId);

        var sessionIsValid = _userRepository.ValidateLoginSession(id, guidSessionId);
        scope.Complete();

        return sessionIsValid;
    }

    /// <inheritdoc />
    public override async Task<bool> GetTwoFactorEnabledAsync(
        BackOfficeIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(user.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intUserId))
        {
            return await base.GetTwoFactorEnabledAsync(user, cancellationToken);
        }

        return await _twoFactorLoginService.IsTwoFactorEnabledAsync(user.Key);
    }

    /// <inheritdoc />
    public override Task<IdentityResult> CreateAsync(
        BackOfficeIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (user.Email is null || user.UserName is null)
        {
            throw new InvalidOperationException("Email and UserName is required.");
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
            Key = user.Key,
            Kind = user.Kind
        };


        // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
        var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.Logins));
        var isTokensPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.LoginTokens));

        UpdateMemberProperties(userEntity, user);

        SaveAsync(userEntity).GetAwaiter().GetResult();

        if (!userEntity.HasIdentity)
        {
            throw new DataException("Could not create the user, check logs for details");
        }

        // re-assign id and key
        user.Id = UserIdToString(userEntity.Id);
        user.Key = userEntity.Key;

        if (isLoginsPropertyDirty)
        {
            _externalLoginService.Save(
                userEntity.Key,
                user.Logins.Select(x => new ExternalLogin(
                    x.LoginProvider,
                    x.ProviderKey,
                    x.UserData)));
        }

        if (isTokensPropertyDirty)
        {
            _externalLoginService.Save(
                userEntity.Key,
                user.LoginTokens.Select(x => new ExternalLoginToken(
                    x.LoginProvider,
                    x.Name,
                    x.Value)));
        }

        return Task.FromResult(IdentityResult.Success);
    }

    /// <inheritdoc />
    public Task<UserOperationStatus> SaveAsync(IUser user)
    {
        EventMessages eventMessages = _eventMessagesFactory.Get();

        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        var savingNotification = new UserSavingNotification(user, eventMessages);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return Task.FromResult(UserOperationStatus.CancelledByNotification);
        }

        if (string.IsNullOrWhiteSpace(user.Username))
        {
            throw new ArgumentException("Empty username.", nameof(user));
        }

        if (string.IsNullOrWhiteSpace(user.Name))
        {
            throw new ArgumentException("Empty name.", nameof(user));
        }

        try
        {
            _userRepository.Save(user);
            scope.Notifications.Publish(
                new UserSavedNotification(user, eventMessages).WithStateFrom(savingNotification));

            scope.Complete();
        }
        catch (DbException ex)
        {
            // if we are upgrading and an exception occurs, log and swallow it
            if (IsUpgrading == false)
            {
                throw;
            }

            _logger.LogWarning(
                ex,
                "An error occurred attempting to save a user instance during upgrade, normally this warning can be ignored");

            // we don't want the uow to rollback its scope!
            scope.Complete();
        }

        return Task.FromResult(UserOperationStatus.Success);
    }

    /// <inheritdoc />
    public Task<UserOperationStatus> DisableAsync(IUser user)
    {
        // disable
        user.IsApproved = false;

        return SaveAsync(user);
    }

    public Task<IUser?> GetAsync(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        try
        {
            return Task.FromResult(_userRepository.Get(id));
        }
        catch (DbException)
        {
            // TODO: refactor users/upgrade
            // currently kinda accepting anything on upgrade, but that won't deal with all cases
            // so we need to do it differently, see the custom UmbracoPocoDataBuilder which should
            // be better BUT requires that the app restarts after the upgrade!
            if (IsUpgrading)
            {
                // NOTE: this will not be cached
                return Task.FromResult(_userRepository.GetForUpgrade(id));
            }

            throw;
        }
    }

    public Task<IEnumerable<IUser>> GetUsersAsync(params int[]? ids)
    {
        if (ids is null || ids.Length <= 0)
        {
            return Task.FromResult(Enumerable.Empty<IUser>());
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IQuery<IUser> query = _scopeProvider.CreateQuery<IUser>().Where(x => ids.Contains(x.Id));
        IEnumerable<IUser> users = _userRepository.Get(query);

        return Task.FromResult(users);
    }

    public Task<IEnumerable<IUser>> GetUsersAsync(params Guid[]? keys)
    {
        if (keys is null || keys.Length <= 0)
        {
            return Task.FromResult(Enumerable.Empty<IUser>());
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<IUser> users = _userRepository.GetMany(keys);

        return Task.FromResult(users);
    }

    /// <inheritdoc />
    public Task<IUser?> GetAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_userRepository.Get(key));
    }

    /// <inheritdoc />
    public Task<IUser?> GetByUserNameAsync(string username)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        try
        {
            IUser? user = _userRepository.GetByUsername(username, true);
            return Task.FromResult(user);
        }
        catch (DbException)
        {
            // TODO: refactor users/upgrade
            // currently kinda accepting anything on upgrade, but that won't deal with all cases
            // so we need to do it differently, see the custom UmbracoPocoDataBuilder which should
            // be better BUT requires that the app restarts after the upgrade!
            if (IsUpgrading)
            {
                // NOTE: this will not be cached
                IUser? upgradeUser =  _userRepository.GetForUpgradeByUsername(username);
                return Task.FromResult(upgradeUser);
            }

            throw;
        }

    }

    /// <inheritdoc />
    public Task<IUser?> GetByEmailAsync(string email)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        try
        {
            IQuery<IUser> query = _scopeProvider.CreateQuery<IUser>().Where(x => x.Email.Equals(email));
            IUser? user = _userRepository.Get(query).FirstOrDefault();
            return Task.FromResult(user);
        }
        catch(DbException)
        {
            // We also need to catch upgrade state here, because the framework will try to call this to validate the email.
            if (IsUpgrading)
            {
                IUser? upgradeUser = _userRepository.GetForUpgradeByEmail(email);
                return Task.FromResult(upgradeUser);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<IUser>> GetAllInGroupAsync(int groupId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<IUser> usersInGroup = _userRepository.GetAllInGroup(groupId);
        return Task.FromResult(usersInGroup);
    }

    private bool IsUpgrading =>
        _runtimeState.Level == RuntimeLevel.Install || _runtimeState.Level == RuntimeLevel.Upgrade;

    /// <inheritdoc />
    public override Task<IdentityResult> UpdateAsync(
        BackOfficeIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (!int.TryParse(user.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInt))
        {
            throw new InvalidOperationException("The user id must be an integer to work with the Umbraco");
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            IUser? found = GetAsync(asInt).GetAwaiter().GetResult();
            if (found != null)
            {
                // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
                var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.Logins));
                var isTokensPropertyDirty = user.IsPropertyDirty(nameof(BackOfficeIdentityUser.LoginTokens));

                if (UpdateMemberProperties(found, user))
                {
                    SaveAsync(found).GetAwaiter().GetResult();
                }

                if (isLoginsPropertyDirty)
                {
                    _externalLoginService.Save(
                        found.Key,
                        user.Logins.Select(x => new ExternalLogin(
                            x.LoginProvider,
                            x.ProviderKey,
                            x.UserData)));
                }

                if (isTokensPropertyDirty)
                {
                    _externalLoginService.Save(
                        found.Key,
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
    public override async Task<IdentityResult> DeleteAsync(
        BackOfficeIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (TryFindUserFromString(user.Id, out IUser? found))
        {
            await DisableAsync(found);
        }

        _externalLoginService.DeleteUserLogins(user.Key);

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public override async Task<BackOfficeIdentityUser?> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        IUser? user = await GetByUserNameAsync(userName);
        if (user == null)
        {
            return null;
        }

        BackOfficeIdentityUser? result = AssignLoginsCallback(_mapper.Map<BackOfficeIdentityUser>(user));

        return result;
    }

    /// <inheritdoc />
    protected override Task<BackOfficeIdentityUser?> FindUserAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (TryFindUserFromString(userId, out IUser? user) is false)
        {
            return Task.FromResult((BackOfficeIdentityUser?)null)!;
        }

        return Task.FromResult(AssignLoginsCallback(_mapper.Map<BackOfficeIdentityUser>(user)))!;
    }

    private bool TryFindUserFromString(string userId, [NotNullWhen(true)] out IUser? user)
    {
        // We could use ResolveEntityIdFromIdentityId here, but that would require multiple DB calls, so let's not.
        if (TryConvertIdentityIdToInt(userId, out var id))
        {
            user = GetAsync(id).GetAwaiter().GetResult();
            return user is not null;
        }

        // We couldn't directly convert the ID to an int, this is because the user logged in with external login.
        // So we need to look up the user by key.
        if (Guid.TryParse(userId, out Guid key))
        {
            user = GetAsync(key).GetAwaiter().GetResult();
            return user is not null;
        }

        // Maybe we have some other format of user Id from an external login flow, so don't throw but return null.
        // We won't be able to find the user via this ID in a local database lookup so we'll handle the same as if they don't exist.

        user = null;
        return false;
    }

    protected override async Task<int> ResolveEntityIdFromIdentityId(string? identityId)
    {
        if (TryConvertIdentityIdToInt(identityId, out var result))
        {
            return result;
        }

        // We couldn't directly convert the ID to an int, this is because the user logged in with external login.
        // So we need to look up the user by key, and then get the ID.
        if (Guid.TryParse(identityId, out Guid key))
        {
            IUser? user = await GetAsync(key);
            if (user is not null)
            {
                return user.Id;
            }
        }

        throw new InvalidOperationException($"Unable to resolve a user id from {identityId}");
    }

    /// <inheritdoc />
    public override async Task<BackOfficeIdentityUser?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        IUser? user = await GetByEmailAsync(email);
        BackOfficeIdentityUser? result = user == null
            ? null
            : _mapper.Map<BackOfficeIdentityUser>(user);

        return AssignLoginsCallback(result);
    }

    /// <inheritdoc />
    public override async Task SetPasswordHashAsync(BackOfficeIdentityUser user, string? passwordHash, CancellationToken cancellationToken = default)
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
        var instance = new IdentityUserLogin(login.LoginProvider, login.ProviderKey, user.Id);
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

        IIdentityUserLogin? userLogin =
            user.Logins.SingleOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        if (userLogin != null)
        {
            user.Logins.Remove(userLogin);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task<IList<UserLoginInfo>> GetLoginsAsync(
        BackOfficeIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult((IList<UserLoginInfo>)user.Logins
            .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.LoginProvider)).ToList());
    }

    /// <summary>
    ///     Lists all users of a given role.
    /// </summary>
    /// <remarks>
    ///     Identity Role names are equal to Umbraco UserGroup alias.
    /// </remarks>
    public override async Task<IList<BackOfficeIdentityUser>> GetUsersInRoleAsync(
        string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (normalizedRoleName == null)
        {
            throw new ArgumentNullException(nameof(normalizedRoleName));
        }

        IUserGroup? userGroup = _userGroupService.GetAsync(normalizedRoleName).GetAwaiter().GetResult();

        if (userGroup is null)
        {
            return new List<BackOfficeIdentityUser>();
        }

        IEnumerable<IUser> users = await GetAllInGroupAsync(userGroup.Id);
        IList<BackOfficeIdentityUser> backOfficeIdentityUsers =
            users.Select(x => _mapper.Map<BackOfficeIdentityUser>(x)).Where(x => x != null).ToList()!;

        return backOfficeIdentityUsers;
    }

    /// <summary>
    ///     Overridden to support Umbraco's own data storage requirements
    /// </summary>
    /// <remarks>
    ///     The base class's implementation of this calls into FindTokenAsync and AddUserTokenAsync, both methods will only
    ///     work with ORMs that are change
    ///     tracking ORMs like EFCore.
    /// </remarks>
    /// <inheritdoc />
    public override Task SetTokenAsync(BackOfficeIdentityUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        IIdentityUserToken? token = user.LoginTokens.FirstOrDefault(x =>
            x.LoginProvider.InvariantEquals(loginProvider) && x.Name.InvariantEquals(name));

        // We have to remove token and then re-add to ensure that LoginTokens are dirty, which is required for them to save
        // This is because we're using an observable collection, which only cares about added/removed items.
        if (token is not null)
        {
            // The token hasn't changed, so there's no reason for us to re-add it.
            if (token.Value == value)
            {
                return Task.CompletedTask;
            }

            user.LoginTokens.Remove(token);
        }

        user.LoginTokens.Add(new IdentityUserToken(loginProvider, name, value, user.Id));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task<IdentityUserLogin<string>?> FindUserLoginAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        BackOfficeIdentityUser? user = await FindUserAsync(userId, cancellationToken);
        if (user?.Id is null)
        {
            return null;
        }

        IList<UserLoginInfo> logins = await GetLoginsAsync(user, cancellationToken);
        UserLoginInfo? found =
            logins.FirstOrDefault(x => x.ProviderKey == providerKey && x.LoginProvider == loginProvider);
        if (found == null)
        {
            return null;
        }

        return new IdentityUserLogin<string>
        {
            LoginProvider = found.LoginProvider,
            ProviderKey = found.ProviderKey,
            ProviderDisplayName = found.ProviderDisplayName, // TODO: We don't store this value so it will be null
            UserId = user.Id,
        };
    }

    /// <inheritdoc />
    protected override Task<IdentityUserLogin<string>?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var logins = _externalLoginService.Find(loginProvider, providerKey).ToList();
        if (logins.Count == 0)
        {
            return Task.FromResult<IdentityUserLogin<string>?>(null);
        }

        IIdentityUserLogin found = logins[0];
        return Task.FromResult<IdentityUserLogin<string>?>(new IdentityUserLogin<string>
        {
            LoginProvider = found.LoginProvider,
            ProviderKey = found.ProviderKey,
            ProviderDisplayName = null, // TODO: We don't store this value so it will be null
            UserId = found.UserId,
        });
    }

    /// <inheritdoc />
    protected override Task<IdentityRole<string>?> FindRoleAsync(
        string normalizedRoleName,
        CancellationToken cancellationToken)
    {
        IUserGroup? group = _userGroupService.GetAsync(normalizedRoleName).GetAwaiter().GetResult();
        if (group?.Name is null)
        {
            return Task.FromResult<IdentityRole<string>?>(null);
        }

        return Task.FromResult<IdentityRole<string>?>(new IdentityRole<string>(group.Name)
        {
            Id = group.Alias,
        });
    }

    /// <inheritdoc />
    protected override async Task<IdentityUserRole<string>?> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken)
    {
        BackOfficeIdentityUser? user = await FindUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return null!;
        }

        IdentityUserRole<string>? found = user.Roles.FirstOrDefault(x => x.RoleId.InvariantEquals(roleId));
        return found;
    }

    private BackOfficeIdentityUser? AssignLoginsCallback(BackOfficeIdentityUser? user)
    {
        if (user != null)
        {
            user.SetLoginsCallback(
                new Lazy<IEnumerable<IIdentityUserLogin>?>(() => _externalLoginService.GetExternalLogins(user.Key)));
            user.SetTokensCallback(
                new Lazy<IEnumerable<IIdentityUserToken>?>(() => _externalLoginService.GetExternalLoginTokens(user.Key)));
        }

        return user;
    }

    private bool UpdateMemberProperties(IUser user, BackOfficeIdentityUser identityUser)
    {
        var anythingChanged = false;

        // don't assign anything if nothing has changed as this will trigger the track changes of the model
        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastLoginDateUtc))
            || (user.LastLoginDate != default && identityUser.LastLoginDateUtc.HasValue == false)
            || (identityUser.LastLoginDateUtc.HasValue &&
                user.LastLoginDate?.ToUniversalTime() != identityUser.LastLoginDateUtc.Value))
        {
            anythingChanged = true;

            // if the LastLoginDate is being set to MinValue, don't convert it ToLocalTime
            DateTime? dt = identityUser.LastLoginDateUtc?.ToLocalTime();
            user.LastLoginDate = dt;
        }

        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.InviteDateUtc))
            || user.InvitedDate?.ToUniversalTime() != identityUser.InviteDateUtc)
        {
            anythingChanged = true;
            user.InvitedDate = identityUser.InviteDateUtc?.ToLocalTime();
        }

        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.LastPasswordChangeDateUtc))
            || (user.LastPasswordChangeDate.HasValue && user.LastPasswordChangeDate.Value != default &&
                identityUser.LastPasswordChangeDateUtc.HasValue == false)
            || (identityUser.LastPasswordChangeDateUtc.HasValue && user.LastPasswordChangeDate?.ToUniversalTime() !=
                identityUser.LastPasswordChangeDateUtc.Value))
        {
            anythingChanged = true;
            user.LastPasswordChangeDate = identityUser.LastPasswordChangeDateUtc?.ToLocalTime() ?? DateTime.Now;
        }

        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.EmailConfirmed))
            || (user.EmailConfirmedDate.HasValue && user.EmailConfirmedDate.Value != default &&
                identityUser.EmailConfirmed == false)
            || ((user.EmailConfirmedDate.HasValue == false || user.EmailConfirmedDate.Value == default) &&
                identityUser.EmailConfirmed))
        {
            anythingChanged = true;
            user.EmailConfirmedDate = identityUser.EmailConfirmed ? DateTime.Now : null;
        }

        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Name))
            && user.Name != identityUser.Name && identityUser.Name.IsNullOrWhiteSpace() == false)
        {
            anythingChanged = true;
            user.Name = identityUser.Name ?? string.Empty;
        }

        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.Email))
            && user.Email != identityUser.Email && identityUser.Email.IsNullOrWhiteSpace() == false)
        {
            anythingChanged = true;
            user.Email = identityUser.Email!;
        }

        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.AccessFailedCount))
            && user.FailedPasswordAttempts != identityUser.AccessFailedCount)
        {
            anythingChanged = true;
            user.FailedPasswordAttempts = identityUser.AccessFailedCount;
        }

        if (user.IsApproved != identityUser.IsApproved)
        {
            anythingChanged = true;
            user.IsApproved = identityUser.IsApproved;
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
            user.Username = identityUser.UserName!;
        }

        if (identityUser.IsPropertyDirty(nameof(BackOfficeIdentityUser.PasswordHash))
            && user.RawPasswordValue != identityUser.PasswordHash &&
            identityUser.PasswordHash.IsNullOrWhiteSpace() == false)
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
            var identityUserRoles = identityUser.Roles.Select(x => x.RoleId).Where(x => x is not null).ToArray();

            anythingChanged = true;

            // clear out the current groups (need to ToArray since we are modifying the iterator)
            user.ClearGroups();

            // go lookup all these groups
            IReadOnlyUserGroup[] groups = _userGroupService.GetAsync(identityUserRoles).GetAwaiter().GetResult()
                .Select(x => x.ToReadOnlyGroup()).ToArray();

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

        IIdentityUserToken? token = user.LoginTokens.FirstOrDefault(x =>
            x.LoginProvider.InvariantEquals(loginProvider) && x.Name.InvariantEquals(name));
        if (token != null)
        {
            user.LoginTokens.Remove(token);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Overridden to support Umbraco's own data storage requirements
    /// </summary>
    /// <remarks>
    ///     The base class's implementation of this calls into FindTokenAsync, RemoveUserTokenAsync and AddUserTokenAsync, both
    ///     methods will only work with ORMs that are change
    ///     tracking ORMs like EFCore.
    /// </remarks>
    /// <inheritdoc />
    public override Task<string?> GetTokenAsync(BackOfficeIdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        IIdentityUserToken? token = user.LoginTokens.FirstOrDefault(x =>
            x.LoginProvider.InvariantEquals(loginProvider) && x.Name.InvariantEquals(name));

        return Task.FromResult(token?.Value);
    }
}
