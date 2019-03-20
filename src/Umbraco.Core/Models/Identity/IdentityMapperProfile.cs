using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.Identity
{
    public class IdentityMapperProfile : IMapperProfile
    {
        private readonly ILocalizedTextService _textService;
        private readonly IEntityService _entityService;
        private readonly IGlobalSettings _globalSettings;

        public IdentityMapperProfile(ILocalizedTextService textService, IEntityService entityService, IGlobalSettings globalSettings)
        {
            _textService = textService;
            _entityService = entityService;
            _globalSettings = globalSettings;
        }

        public void SetMaps(Mapper mapper)
        {
            mapper.SetMap<IUser, BackOfficeIdentityUser>(Map);
        }

        public BackOfficeIdentityUser Map(IUser source)
        {
            var target = new BackOfficeIdentityUser(source.Id, source.Groups);
            target.DisableChangeTracking();
            Map(source, target);
            target.ResetDirtyProperties(true);
            target.EnableChangeTracking();

            return target;
        }

        // Umbraco.Code.MapAll -Groups -LockoutEnabled -PhoneNumber -PhoneNumberConfirmed -TwoFactorEnabled
        private void Map(IUser source, BackOfficeIdentityUser target)
        {
            target.Id = source.Id; // also in ctor but required; BackOfficeIdentityUser is weird
            target.CalculatedMediaStartNodeIds = source.CalculateMediaStartNodeIds(_entityService);
            target.CalculatedContentStartNodeIds = source.CalculateContentStartNodeIds(_entityService);
            target.Email = source.Email;
            target.UserName = source.Username;
            target.LastPasswordChangeDateUtc = source.LastPasswordChangeDate.ToUniversalTime();
            target.LastLoginDateUtc = source.LastLoginDate.ToUniversalTime();
            target.EmailConfirmed = source.EmailConfirmedDate.HasValue;
            target.Name = source.Name;
            target.AccessFailedCount = source.FailedPasswordAttempts;
            target.PasswordHash = GetPasswordHash(source.RawPasswordValue);
            target.StartContentIds = source.StartContentIds;
            target.StartMediaIds = source.StartMediaIds;
            target.Culture = source.GetUserCulture(_textService, _globalSettings).ToString(); // project CultureInfo to string
            target.IsApproved = source.IsApproved;
            target.SecurityStamp = source.SecurityStamp;
            target.LockoutEndDateUtc = source.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?) null;

            // this was in AutoMapper but does not have a setter anyways
            //target.AllowedSections = source.AllowedSections.ToArray(),

            // these were marked as ignored for AutoMapper but don't have a setter anyways
            //target.Logins =;
            //target.Claims =;
            //target.Roles =;
        }

        private static string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith(Constants.Security.EmptyPasswordPrefix) ? null : storedPass;
        }
    }
}
