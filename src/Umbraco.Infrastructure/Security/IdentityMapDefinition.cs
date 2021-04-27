// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{
    public class IdentityMapDefinition : IMapDefinition
    {
        private readonly ILocalizedTextService _textService;
        private readonly IEntityService _entityService;
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly AppCaches _appCaches;

        public IdentityMapDefinition(
            ILocalizedTextService textService,
            IEntityService entityService,
            IOptions<GlobalSettings> globalSettings,
            AppCaches appCaches)
        {
            _textService = textService;
            _entityService = entityService;
            _globalSettings = globalSettings;
            _appCaches = appCaches;
        }

        public void DefineMaps(IUmbracoMapper mapper)
        {
            mapper.Define<IUser, BackOfficeIdentityUser>(
                (source, context) =>
                {
                    var target = new BackOfficeIdentityUser(_globalSettings.Value, source.Id, source.Groups);
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

        // Umbraco.Code.MapAll -Id -LockoutEnabled -PhoneNumber -PhoneNumberConfirmed -TwoFactorEnabled -ConcurrencyStamp -NormalizedEmail -NormalizedUserName -Roles
        private void Map(IUser source, BackOfficeIdentityUser target)
        {
            // NOTE: Groups/Roles are set in the BackOfficeIdentityUser ctor

            target.CalculatedMediaStartNodeIds = source.CalculateMediaStartNodeIds(_entityService, _appCaches);
            target.CalculatedContentStartNodeIds = source.CalculateContentStartNodeIds(_entityService, _appCaches);
            target.Email = source.Email;
            target.UserName = source.Username;
            target.LastPasswordChangeDateUtc = source.LastPasswordChangeDate.ToUniversalTime();
            target.LastLoginDateUtc = source.LastLoginDate.ToUniversalTime();
            target.EmailConfirmed = source.EmailConfirmedDate.HasValue;
            target.Name = source.Name;
            target.AccessFailedCount = source.FailedPasswordAttempts;
            target.PasswordHash = GetPasswordHash(source.RawPasswordValue);
            target.PasswordConfig = source.PasswordConfiguration;
            target.StartContentIds = source.StartContentIds;
            target.StartMediaIds = source.StartMediaIds;
            target.Culture = source.GetUserCulture(_textService, _globalSettings.Value).ToString(); // project CultureInfo to string
            target.IsApproved = source.IsApproved;
            target.SecurityStamp = source.SecurityStamp;
            target.LockoutEnd = source.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?)null;
        }

        // TODO: We need to validate this mapping is OK, we need to get Umbraco.Code working
        private void Map(IMember source, MemberIdentityUser target)
        {
            target.Email = source.Email;
            target.UserName = source.Username;
            target.LastPasswordChangeDateUtc = source.LastPasswordChangeDate.ToUniversalTime();
            target.LastLoginDateUtc = source.LastLoginDate.ToUniversalTime();
            target.EmailConfirmed = source.EmailConfirmedDate.HasValue;
            target.Name = source.Name;
            target.AccessFailedCount = source.FailedPasswordAttempts;
            target.PasswordHash = GetPasswordHash(source.RawPasswordValue);
            target.PasswordConfig = source.PasswordConfiguration;
            target.IsApproved = source.IsApproved;
            target.SecurityStamp = source.SecurityStamp;
            target.LockoutEnd = source.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?)null;
            target.Comments = source.Comments;
            target.LastLockoutDateUtc = source.LastLockoutDate == DateTime.MinValue ? null : source.LastLockoutDate.ToUniversalTime();
            target.CreatedDateUtc = source.CreateDate.ToUniversalTime();
            target.Key = source.Key;
            target.MemberTypeAlias = source.ContentTypeAlias;

            // NB: same comments re AutoMapper as per BackOfficeUser
        }

        private static string GetPasswordHash(string storedPass) => storedPass.StartsWith(Constants.Security.EmptyPasswordPrefix) ? null : storedPass;
    }
}
