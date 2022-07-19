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
    private readonly GlobalSettings _globalSettings;
    private readonly ILocalizedTextService _textService;

    public IdentityMapDefinition(
        ILocalizedTextService textService,
        IEntityService entityService,
        IOptions<GlobalSettings> globalSettings,
        AppCaches appCaches)
    {
        _textService = textService;
        _entityService = entityService;
        _globalSettings = globalSettings.Value;
        _appCaches = appCaches;
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
        target.LockoutEnd = source.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?)null;
    }

    // Umbraco.Code.MapAll -Id -LockoutEnabled -PhoneNumber -PhoneNumberConfirmed -TwoFactorEnabled -ConcurrencyStamp -NormalizedEmail -NormalizedUserName -Roles
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
        target.LockoutEnd = source.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?)null;
        target.Comments = source.Comments;
        target.LastLockoutDateUtc = source.LastLockoutDate == DateTime.MinValue
            ? null
            : source.LastLockoutDate?.ToUniversalTime();
        target.CreatedDateUtc = source.CreateDate.ToUniversalTime();
        target.Key = source.Key;
        target.MemberTypeAlias = source.ContentTypeAlias;

        // NB: same comments re AutoMapper as per BackOfficeUser
    }
}
