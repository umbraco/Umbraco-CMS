// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Represents the definition of an identity map used to manage and track security identities within Umbraco CMS.
/// This class helps ensure that each security identity is uniquely mapped and efficiently retrieved during authentication and authorization processes.
/// </summary>
public class IdentityMapDefinition : IMapDefinition
{
    private readonly AppCaches _appCaches;
    private readonly IEntityService _entityService;
    private readonly SecuritySettings _securitySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly ILocalizedTextService _textService;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Security.IdentityMapDefinition"/> class.
    /// </summary>
    /// <param name="textService">An <see cref="ILocalizedTextService"/> used for retrieving localized text.</param>
    /// <param name="entityService">An <see cref="IEntityService"/> for accessing Umbraco entities.</param>
    /// <param name="globalSettings">The <see cref="IOptions{GlobalSettings}"/> providing global configuration options.</param>
    /// <param name="securitySettings">The <see cref="IOptions{SecuritySettings}"/> providing security configuration options.</param>
    /// <param name="appCaches">The <see cref="AppCaches"/> instance for application-level caching.</param>
    /// <param name="twoFactorLoginService">An <see cref="ITwoFactorLoginService"/> for managing two-factor authentication.</param>
    public IdentityMapDefinition(
        ILocalizedTextService textService,
        IEntityService entityService,
        IOptions<GlobalSettings> globalSettings,
        IOptions<SecuritySettings> securitySettings,
        AppCaches appCaches,
        ITwoFactorLoginService twoFactorLoginService)
    {
        _textService = textService;
        _entityService = entityService;
        _globalSettings = globalSettings.Value;
        _securitySettings = securitySettings.Value;
        _appCaches = appCaches;
        _twoFactorLoginService = twoFactorLoginService;
    }

    /// <summary>
    /// Configures mapping definitions between core identity interfaces and their corresponding identity user implementations.
    /// Specifically, maps <see cref="IUser"/> to <see cref="BackOfficeIdentityUser"/> and <see cref="IMember"/> to <see cref="MemberIdentityUser"/> using the provided <paramref name="mapper"/>.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mappings.</param>
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IUser, BackOfficeIdentityUser>(
            (source, context) =>
            {
                var target = new BackOfficeIdentityUser(_globalSettings, source.Id, source.Groups);
                target.DisableChangeTracking();
                return target;
            },
            (source, target, context) =>
            {
                Map(source, target);
                target.ResetDirtyProperties(true);
                target.EnableChangeTracking();
            });

        mapper.Define<IMember, MemberIdentityUser>(
            (source, context) =>
            {
                var target = new MemberIdentityUser(source.Id);
                target.DisableChangeTracking();
                return target;
            },
            (source, target, context) =>
            {
                Map(source, target);
                target.ResetDirtyProperties(true);
                target.EnableChangeTracking();
            });
    }

    private static string? GetPasswordHash(string? storedPass) =>
        storedPass?.StartsWith(Constants.Security.EmptyPasswordPrefix) ?? false ? null : storedPass;

    // Umbraco.Code.MapAll -Id -LockoutEnabled -PhoneNumber -PhoneNumberConfirmed -TwoFactorEnabled -ConcurrencyStamp -NormalizedEmail -NormalizedUserName -Roles
    private void Map(IUser source, BackOfficeIdentityUser target)
    {
        // NOTE: Groups/Roles are set in the BackOfficeIdentityUser ctor
        target.Key = source.Key;
        target.CalculatedMediaStartNodeIds = source.CalculateMediaStartNodeIds(_entityService, _appCaches);
        target.CalculatedContentStartNodeIds = source.CalculateContentStartNodeIds(_entityService, _appCaches);
        target.Email = source.Email;
        target.UserName = source.Username;
        target.LastPasswordChangeDate = source.LastPasswordChangeDate?.ToUniversalTime();
        target.LastLoginDate = source.LastLoginDate?.ToUniversalTime();
        target.InviteDate = source.InvitedDate?.ToUniversalTime();
        target.EmailConfirmed = source.EmailConfirmedDate.HasValue;
        target.Name = source.Name;
        target.AccessFailedCount = source.FailedPasswordAttempts;
        target.PasswordHash = GetPasswordHash(source.RawPasswordValue);
        target.PasswordConfig = source.PasswordConfiguration;
        target.StartContentIds = source.StartContentIds ?? Array.Empty<int>();
        target.StartMediaIds = source.StartMediaIds ?? Array.Empty<int>();
        target.Culture =
            source.GetUserCulture(_textService, _globalSettings).ToString(); // project CultureInfo to string
        target.IsApproved = source.IsApproved;
        target.SecurityStamp = source.SecurityStamp;
        DateTime? lockedOutUntil = source.LastLockoutDate?.AddMinutes(_securitySettings.UserDefaultLockoutTimeInMinutes);
        target.LockoutEnd = source.IsLockedOut ? (lockedOutUntil ?? DateTime.MaxValue).ToUniversalTime() : null;
        target.Kind = source.Kind;
    }

    // Umbraco.Code.MapAll -Id -LockoutEnabled -PhoneNumber -PhoneNumberConfirmed -ConcurrencyStamp -NormalizedEmail -NormalizedUserName -Roles
    private void Map(IMember source, MemberIdentityUser target)
    {
        target.Email = source.Email;
        target.UserName = source.Username;
        target.LastPasswordChangeDate = source.LastPasswordChangeDate?.ToUniversalTime();
        target.LastLoginDate = source.LastLoginDate?.ToUniversalTime();
        target.EmailConfirmed = source.EmailConfirmedDate.HasValue;
        target.Name = source.Name;
        target.AccessFailedCount = source.FailedPasswordAttempts;
        target.PasswordHash = GetPasswordHash(source.RawPasswordValue);
        target.PasswordConfig = source.PasswordConfiguration;
        target.IsApproved = source.IsApproved;
        target.SecurityStamp = source.SecurityStamp;
        DateTime? lockedOutUntil = source.LastLockoutDate?.AddMinutes(_securitySettings.MemberDefaultLockoutTimeInMinutes);
        target.LockoutEnd = source.IsLockedOut ? (lockedOutUntil ?? DateTime.MaxValue).ToUniversalTime() : null;
        target.Comments = source.Comments;
        target.LastLockoutDate = source.LastLockoutDate == DateTime.MinValue
            ? null
            : source.LastLockoutDate?.ToUniversalTime();
        target.CreatedDate = source.CreateDate.ToUniversalTime();
        target.Key = source.Key;
        target.MemberTypeAlias = source.ContentTypeAlias;
        target.TwoFactorEnabled = _twoFactorLoginService.IsTwoFactorEnabledAsync(source.Key).GetAwaiter().GetResult();

        // NB: same comments re AutoMapper as per BackOfficeUser
    }
}
