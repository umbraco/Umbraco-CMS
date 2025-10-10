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

public class IdentityMapDefinition : IMapDefinition
{
    private readonly AppCaches _appCaches;
    private readonly IEntityService _entityService;
    private readonly SecuritySettings _securitySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly ILocalizedTextService _textService;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

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
        target.LastPasswordChangeDateUtc = source.LastPasswordChangeDate?.ToUniversalTime();
        target.LastLoginDateUtc = source.LastLoginDate?.ToUniversalTime();
        target.InviteDateUtc = source.InvitedDate?.ToUniversalTime();
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
        target.LockoutEnd = source.IsLockedOut ? lockedOutUntil ?? DateTime.MaxValue : null;
        target.Kind = source.Kind;
    }

    // Umbraco.Code.MapAll -Id -LockoutEnabled -PhoneNumber -PhoneNumberConfirmed -ConcurrencyStamp -NormalizedEmail -NormalizedUserName -Roles
    private void Map(IMember source, MemberIdentityUser target)
    {
        target.Email = source.Email;
        target.UserName = source.Username;
        target.LastPasswordChangeDateUtc = source.LastPasswordChangeDate?.ToUniversalTime();
        target.LastLoginDateUtc = source.LastLoginDate?.ToUniversalTime();
        target.EmailConfirmed = source.EmailConfirmedDate.HasValue;
        target.Name = source.Name;
        target.AccessFailedCount = source.FailedPasswordAttempts;
        target.PasswordHash = GetPasswordHash(source.RawPasswordValue);
        target.PasswordConfig = source.PasswordConfiguration;
        target.IsApproved = source.IsApproved;
        target.SecurityStamp = source.SecurityStamp;
        DateTime? lockedOutUntil = source.LastLockoutDate?.AddMinutes(_securitySettings.MemberDefaultLockoutTimeInMinutes);
        target.LockoutEnd = GetLockoutEnd(source);
        target.LastLockoutDateUtc = GetLastLockoutDateUtc(source);
        target.CreatedDateUtc = EnsureUtcWithServerTime(source.CreateDate);
        target.Comments = source.Comments;
        target.Key = source.Key;
        target.MemberTypeAlias = source.ContentTypeAlias;
        target.TwoFactorEnabled = _twoFactorLoginService.IsTwoFactorEnabledAsync(source.Key).GetAwaiter().GetResult();

        // NB: same comments re AutoMapper as per BackOfficeUser
    }

    private DateTimeOffset? GetLockoutEnd(IMember source)
    {
        if (source.IsLockedOut is false)
        {
            return null;
        }

        DateTime? lockedOutUntil = source.LastLockoutDate?.AddMinutes(_securitySettings.MemberDefaultLockoutTimeInMinutes);
        if (lockedOutUntil.HasValue is false)
        {
            return DateTime.MaxValue;
        }

        return EnsureUtcWithServerTime(lockedOutUntil.Value);
    }

    private static DateTime? GetLastLockoutDateUtc(IMember source)
    {
        if (source.LastLockoutDate is null || source.LastLockoutDate == DateTime.MinValue)
        {
            return null;
        }

        return EnsureUtcWithServerTime(source.LastLockoutDate.Value);
    }

    private static DateTime EnsureUtcWithServerTime(DateTime date) =>

        // We have a server time value here, but the Kind is UTC, so we can't use .ToUniversalTime() to convert to the UTC
        // value that the LockoutEnd property expects. We need to create a DateTimeOffset with the correct offset.
        DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
}
