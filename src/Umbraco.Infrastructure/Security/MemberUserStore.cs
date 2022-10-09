using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A custom user store that uses Umbraco member data
/// </summary>
public class MemberUserStore : UmbracoUserStore<MemberIdentityUser, UmbracoIdentityRole>, IMemberUserStore
{
    private const string GenericIdentityErrorCode = "IdentityErrorUserStore";
    private readonly IExternalLoginWithKeyService _externalLoginService;
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberService _memberService;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberUserStore" /> class for the members identity store
    /// </summary>
    /// <param name="memberService">The member service</param>
    /// <param name="mapper">The mapper for properties</param>
    /// <param name="scopeProvider">The scope provider</param>
    /// <param name="describer">The error describer</param>
    /// <param name="publishedSnapshotAccessor">The published snapshot accessor</param>
    /// <param name="externalLoginService">The external login service</param>
    /// <param name="twoFactorLoginService">The two factor login service</param>
    [ActivatorUtilitiesConstructor]
    public MemberUserStore(
        IMemberService memberService,
        IUmbracoMapper mapper,
        ICoreScopeProvider scopeProvider,
        IdentityErrorDescriber describer,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IExternalLoginWithKeyService externalLoginService,
        ITwoFactorLoginService twoFactorLoginService)
        : base(describer)
    {
        _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _externalLoginService = externalLoginService;
        _twoFactorLoginService = twoFactorLoginService;
    }

    [Obsolete("Use ctor with IExternalLoginWithKeyService and ITwoFactorLoginService param")]
    public MemberUserStore(
        IMemberService memberService,
        IUmbracoMapper mapper,
        ICoreScopeProvider scopeProvider,
        IdentityErrorDescriber describer,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IExternalLoginService externalLoginService)
        : this(memberService, mapper, scopeProvider, describer, publishedSnapshotAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IExternalLoginWithKeyService>(),
            StaticServiceProvider.Instance.GetRequiredService<ITwoFactorLoginService>())
    {
    }

    [Obsolete("Use ctor with IExternalLoginWithKeyService and ITwoFactorLoginService param")]
    public MemberUserStore(
        IMemberService memberService,
        IUmbracoMapper mapper,
        ICoreScopeProvider scopeProvider,
        IdentityErrorDescriber describer,
        IPublishedSnapshotAccessor publishedSnapshotAccessor)
        : this(memberService, mapper, scopeProvider, describer, publishedSnapshotAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IExternalLoginWithKeyService>(),
            StaticServiceProvider.Instance.GetRequiredService<ITwoFactorLoginService>())
    {
    }

    /// <inheritdoc />
    public override Task<IdentityResult> CreateAsync(
        MemberIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

            // create member
            IMember memberEntity = _memberService.CreateMember(
                user.UserName,
                user.Email,
                user.Name.IsNullOrWhiteSpace() ? user.UserName : user.Name!,
                user.MemberTypeAlias.IsNullOrWhiteSpace()
                    ? Constants.Security.DefaultMemberTypeAlias
                    : user.MemberTypeAlias!);

            UpdateMemberProperties(memberEntity, user, out bool _);

            // create the member
            _memberService.Save(memberEntity);

            // We need to add roles now that the member has an Id. It do not work implicit in UpdateMemberProperties
            _memberService.AssignRoles(
                new[] { memberEntity.Id },
                user.Roles.Select(x => x.RoleId).Where(x => x is not null).ToArray());

            if (!memberEntity.HasIdentity)
            {
                throw new DataException("Could not create the member, check logs for details");
            }

            // re-assign id
            user.Id = UserIdToString(memberEntity.Id);
            user.Key = memberEntity.Key;

            // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
            var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(MemberIdentityUser.Logins));
            var isTokensPropertyDirty = user.IsPropertyDirty(nameof(MemberIdentityUser.LoginTokens));

            if (isLoginsPropertyDirty)
            {
                _externalLoginService.Save(
                    memberEntity.Key,
                    user.Logins.Select(x => new ExternalLogin(
                        x.LoginProvider,
                        x.ProviderKey,
                        x.UserData)));
            }

            if (isTokensPropertyDirty)
            {
                _externalLoginService.Save(
                    memberEntity.Key,
                    user.LoginTokens.Select(x => new ExternalLoginToken(
                        x.LoginProvider,
                        x.Name,
                        x.Value)));
            }

            return Task.FromResult(IdentityResult.Success);
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                IdentityResult.Failed(new IdentityError { Code = GenericIdentityErrorCode, Description = ex.Message }));
        }
    }

    /// <inheritdoc />
    public override Task<IdentityResult> UpdateAsync(
        MemberIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!int.TryParse(user.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInt))
            {
                // TODO: should this be thrown, or an identity result?
                throw new InvalidOperationException("The user id must be an integer to work with the Umbraco");
            }

            using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

            IMember? found = _memberService.GetById(asInt);
            if (found != null)
            {
                // we have to remember whether Logins property is dirty, since the UpdateMemberProperties will reset it.
                var isLoginsPropertyDirty = user.IsPropertyDirty(nameof(MemberIdentityUser.Logins));
                var isTokensPropertyDirty = user.IsPropertyDirty(nameof(MemberIdentityUser.LoginTokens));

                if (UpdateMemberProperties(found, user, out var updateRoles))
                {
                    _memberService.Save(found);

                    if (updateRoles)
                    {
                        var identityUserRoles = user.Roles.Select(x => x.RoleId).ToArray();
                        _memberService.ReplaceRoles(new[] { found.Id }, identityUserRoles);
                    }
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

            return Task.FromResult(IdentityResult.Success);
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                IdentityResult.Failed(new IdentityError { Code = GenericIdentityErrorCode, Description = ex.Message }));
        }
    }

    /// <inheritdoc />
    public override Task<IdentityResult> DeleteAsync(
        MemberIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IMember? found = _memberService.GetById(UserIdToInt(user.Id));
            if (found != null)
            {
                _memberService.Delete(found);
            }

            _externalLoginService.DeleteUserLogins(user.Key);

            return Task.FromResult(IdentityResult.Success);
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                IdentityResult.Failed(new IdentityError { Code = GenericIdentityErrorCode, Description = ex.Message }));
        }
    }

    /// <inheritdoc />
    public override Task<MemberIdentityUser> FindByNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        IMember? user = _memberService.GetByUsername(userName);
        if (user == null)
        {
            return Task.FromResult((MemberIdentityUser)null!);
        }

        MemberIdentityUser result = AssignLoginsCallback(_mapper.Map<MemberIdentityUser>(user))!;

        return Task.FromResult(result);
    }

    public IPublishedContent? GetPublishedMember(MemberIdentityUser? user)
    {
        if (user == null)
        {
            return null;
        }

        IMember? member = _memberService.GetByKey(user.Key);
        if (member == null)
        {
            return null;
        }

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
        return publishedSnapshot.Members?.Get(member);
    }

    /// <inheritdoc />
    public override Task<MemberIdentityUser> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        IMember? member = _memberService.GetByEmail(email);
        MemberIdentityUser? result = member == null
            ? null
            : _mapper.Map<MemberIdentityUser>(member);

        return Task.FromResult(AssignLoginsCallback(result))!;
    }

    /// <inheritdoc />
    protected override Task<MemberIdentityUser> FindUserAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentNullException(nameof(userId));
        }

        IMember? user = Guid.TryParse(userId, out Guid key)
            ? _memberService.GetByKey(key)
            : _memberService.GetById(UserIdToInt(userId));
        if (user == null)
        {
            return Task.FromResult((MemberIdentityUser)null!);
        }

        return Task.FromResult(AssignLoginsCallback(_mapper.Map<MemberIdentityUser>(user)))!;
    }

    /// <inheritdoc />
    public override Task AddLoginAsync(MemberIdentityUser user, UserLoginInfo login,
        CancellationToken cancellationToken = default)
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

        if (string.IsNullOrWhiteSpace(login.LoginProvider))
        {
            throw new ArgumentNullException(nameof(login.LoginProvider));
        }

        if (string.IsNullOrWhiteSpace(login.ProviderKey))
        {
            throw new ArgumentNullException(nameof(login.ProviderKey));
        }

        ICollection<IIdentityUserLogin> logins = user.Logins;
        if (user.Id is not null)
        {
            var instance = new IdentityUserLogin(
                login.LoginProvider,
                login.ProviderKey,
                user.Id);

            IdentityUserLogin userLogin = instance;
            logins.Add(userLogin);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task RemoveLoginAsync(MemberIdentityUser user, string loginProvider, string providerKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(loginProvider))
        {
            throw new ArgumentNullException(nameof(loginProvider));
        }

        if (string.IsNullOrWhiteSpace(providerKey))
        {
            throw new ArgumentNullException(nameof(providerKey));
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
        MemberIdentityUser user,
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
    ///     Gets a list of role names the specified user belongs to.
    /// </summary>
    /// <remarks>
    ///     This lazy loads the roles for the member
    /// </remarks>
    public override Task<IList<string>> GetRolesAsync(
        MemberIdentityUser user,
        CancellationToken cancellationToken = default)
    {
        EnsureRoles(user);
        return base.GetRolesAsync(user, cancellationToken);
    }

    /// <summary>
    ///     Returns true if a user is in the role
    /// </summary>
    public override Task<bool> IsInRoleAsync(MemberIdentityUser user, string roleName,
        CancellationToken cancellationToken = default)
    {
        EnsureRoles(user);

        return base.IsInRoleAsync(user, roleName, cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<IdentityUserLogin<string>> FindUserLoginAsync(string userId, string loginProvider,
        string providerKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(loginProvider))
        {
            throw new ArgumentNullException(nameof(loginProvider));
        }

        if (string.IsNullOrWhiteSpace(providerKey))
        {
            throw new ArgumentNullException(nameof(providerKey));
        }

        MemberIdentityUser user = await FindUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return await Task.FromResult((IdentityUserLogin<string>)null!);
        }

        IList<UserLoginInfo> logins = await GetLoginsAsync(user, cancellationToken);
        UserLoginInfo? found =
            logins.FirstOrDefault(x => x.ProviderKey == providerKey && x.LoginProvider == loginProvider);
        if (found == null)
        {
            return await Task.FromResult((IdentityUserLogin<string>)null!);
        }

        if (user.Id is not null)
        {
            return new IdentityUserLogin<string>
            {
                LoginProvider = found.LoginProvider,
                ProviderKey = found.ProviderKey,

                // TODO: We don't store this value so it will be null
                ProviderDisplayName = found.ProviderDisplayName,
                UserId = user.Id,
            };
        }

        return null!;
    }

    /// <inheritdoc />
    protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(loginProvider))
        {
            throw new ArgumentNullException(nameof(loginProvider));
        }

        if (string.IsNullOrWhiteSpace(providerKey))
        {
            throw new ArgumentNullException(nameof(providerKey));
        }

        var logins = _externalLoginService.Find(loginProvider, providerKey).ToList();
        if (logins.Count == 0)
        {
            return Task.FromResult((IdentityUserLogin<string>)null!);
        }

        IIdentityUserLogin found = logins[0];
        return Task.FromResult(new IdentityUserLogin<string>
        {
            LoginProvider = found.LoginProvider,
            ProviderKey = found.ProviderKey,

            // TODO: We don't store this value so it will be null
            ProviderDisplayName = null,
            UserId = found.UserId,
        });
    }

    private void EnsureRoles(MemberIdentityUser user)
    {
        if (user.Roles.Count == 0)
        {
            // if there are no roles, they either haven't been loaded since we don't eagerly
            // load for members, or they just have no roles.
            IEnumerable<string> currentRoles = _memberService.GetAllRoles(user.UserName);
            ICollection<IdentityUserRole<string>> roles = currentRoles
                .Select(role => new IdentityUserRole<string> { RoleId = role, UserId = user.Id }).ToList();

            user.Roles = roles;
        }
    }

    /// <summary>
    ///     Lists all users of a given role.
    /// </summary>
    public override Task<IList<MemberIdentityUser>?> GetUsersInRoleAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentNullException(nameof(roleName));
        }

        IEnumerable<IMember>? members = _memberService.GetMembersByMemberType(roleName);

        IList<MemberIdentityUser>? membersIdentityUsers =
            members?.Select(x => _mapper.Map<MemberIdentityUser>(x)!).ToList();

        return Task.FromResult(membersIdentityUsers);
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
    public override Task SetTokenAsync(MemberIdentityUser user, string loginProvider, string name, string value,
        CancellationToken cancellationToken)
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

    /// <summary>
    ///     Overridden to support Umbraco's own data storage requirements
    /// </summary>
    /// <remarks>
    ///     The base class's implementation of this calls into FindTokenAsync, RemoveUserTokenAsync and AddUserTokenAsync, both
    ///     methods will only work with ORMs that are change
    ///     tracking ORMs like EFCore.
    /// </remarks>
    /// <inheritdoc />
    public override Task<string?> GetTokenAsync(MemberIdentityUser user, string loginProvider, string name,
        CancellationToken cancellationToken)
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

    /// <inheritdoc />
    protected override Task<UmbracoIdentityRole> FindRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentNullException(nameof(roleName));
        }

        IMemberGroup? group = _memberService.GetAllRoles().SingleOrDefault(x => x.Name == roleName);
        if (group == null)
        {
            return Task.FromResult((UmbracoIdentityRole)null!);
        }

        return Task.FromResult(new UmbracoIdentityRole(group.Name)
        {
            // TODO: what should the alias be?
            Id = group.Id.ToString(),
        });
    }

    /// <inheritdoc />
    protected override async Task<IdentityUserRole<string>> FindUserRoleAsync(string userId, string roleId,
        CancellationToken cancellationToken)
    {
        MemberIdentityUser user = await FindUserAsync(userId, cancellationToken);
        if (user == null)
        {
            return null!;
        }

        IdentityUserRole<string>? found = user.Roles.FirstOrDefault(x => x.RoleId.InvariantEquals(roleId));
        return found!;
    }

    private MemberIdentityUser? AssignLoginsCallback(MemberIdentityUser? user)
    {
        if (user != null)
        {
            user.SetLoginsCallback(
                new Lazy<IEnumerable<IIdentityUserLogin>?>(() => _externalLoginService.GetExternalLogins(user.Key)));
            user.SetTokensCallback(new Lazy<IEnumerable<IIdentityUserToken>?>(() =>
                _externalLoginService.GetExternalLoginTokens(user.Key)));
        }

        return user;
    }

    private bool UpdateMemberProperties(IMember member, MemberIdentityUser identityUser, out bool updateRoles)
    {
        var anythingChanged = false;
        updateRoles = false;

        // don't assign anything if nothing has changed as this will trigger the track changes of the model
        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.LastLoginDateUtc))
            || (member.LastLoginDate != default && identityUser.LastLoginDateUtc.HasValue == false)
            || (identityUser.LastLoginDateUtc.HasValue &&
                member.LastLoginDate?.ToUniversalTime() != identityUser.LastLoginDateUtc.Value))
        {
            anythingChanged = true;

            // if the LastLoginDate is being set to MinValue, don't convert it ToLocalTime
            DateTime dt = identityUser.LastLoginDateUtc == DateTime.MinValue
                ? DateTime.MinValue
                : identityUser.LastLoginDateUtc?.ToLocalTime() ?? DateTime.MinValue;
            member.LastLoginDate = dt;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.LastPasswordChangeDateUtc))
            || (member.LastPasswordChangeDate != default && identityUser.LastPasswordChangeDateUtc.HasValue == false)
            || (identityUser.LastPasswordChangeDateUtc.HasValue && member.LastPasswordChangeDate?.ToUniversalTime() !=
                identityUser.LastPasswordChangeDateUtc.Value))
        {
            anythingChanged = true;
            member.LastPasswordChangeDate = identityUser.LastPasswordChangeDateUtc?.ToLocalTime() ?? DateTime.Now;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.Comments))
            && member.Comments != identityUser.Comments && identityUser.Comments.IsNullOrWhiteSpace() == false)
        {
            anythingChanged = true;
            member.Comments = identityUser.Comments;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.EmailConfirmed))
            || (member.EmailConfirmedDate.HasValue && member.EmailConfirmedDate.Value != default &&
                identityUser.EmailConfirmed == false)
            || ((member.EmailConfirmedDate.HasValue == false || member.EmailConfirmedDate.Value == default) &&
                identityUser.EmailConfirmed))
        {
            anythingChanged = true;
            member.EmailConfirmedDate = identityUser.EmailConfirmed ? DateTime.Now : null;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.Name))
            && member.Name != identityUser.Name && identityUser.Name.IsNullOrWhiteSpace() == false)
        {
            anythingChanged = true;
            member.Name = identityUser.Name ?? string.Empty;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.Email))
            && member.Email != identityUser.Email && identityUser.Email.IsNullOrWhiteSpace() == false)
        {
            anythingChanged = true;
            member.Email = identityUser.Email;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.AccessFailedCount))
            && member.FailedPasswordAttempts != identityUser.AccessFailedCount)
        {
            anythingChanged = true;
            member.FailedPasswordAttempts = identityUser.AccessFailedCount;
        }

        if (member.IsLockedOut != identityUser.IsLockedOut)
        {
            anythingChanged = true;
            member.IsLockedOut = identityUser.IsLockedOut;

            if (member.IsLockedOut)
            {
                // need to set the last lockout date
                member.LastLockoutDate = DateTime.Now;
            }
        }

        if (member.IsApproved != identityUser.IsApproved)
        {
            anythingChanged = true;
            member.IsApproved = identityUser.IsApproved;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.UserName))
            && member.Username != identityUser.UserName && identityUser.UserName.IsNullOrWhiteSpace() == false)
        {
            anythingChanged = true;
            member.Username = identityUser.UserName;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.PasswordHash))
            && member.RawPasswordValue != identityUser.PasswordHash &&
            identityUser.PasswordHash.IsNullOrWhiteSpace() == false)
        {
            anythingChanged = true;
            member.RawPasswordValue = identityUser.PasswordHash;
            member.PasswordConfiguration = identityUser.PasswordConfig;
        }

        if (member.PasswordConfiguration != identityUser.PasswordConfig)
        {
            anythingChanged = true;
            member.PasswordConfiguration = identityUser.PasswordConfig;
        }

        if (member.SecurityStamp != identityUser.SecurityStamp)
        {
            anythingChanged = true;
            member.SecurityStamp = identityUser.SecurityStamp;
        }

        if (identityUser.IsPropertyDirty(nameof(MemberIdentityUser.Roles)))
        {
            anythingChanged = true;
            updateRoles = true;
        }

        // reset all changes
        identityUser.ResetDirtyProperties(false);

        return anythingChanged;
    }

    /// <inheritdoc />
    public override async Task<bool> GetTwoFactorEnabledAsync(
        MemberIdentityUser user,
        CancellationToken cancellationToken = default) =>
        await _twoFactorLoginService.IsTwoFactorEnabledAsync(user.Key);
}
