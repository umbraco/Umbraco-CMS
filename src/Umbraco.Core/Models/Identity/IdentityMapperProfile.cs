using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.Identity
{
    public class IdentityMapper : IMapperProfile
    {
        private readonly ILocalizedTextService _textService;
        private readonly IEntityService _entityService;
        private readonly IGlobalSettings _globalSettings;

        public IdentityMapper(ILocalizedTextService textService, IEntityService entityService, IGlobalSettings globalSettings)
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
            // AssignAll enable
            var target = new BackOfficeIdentityUser(source.Id, source.Groups)
            {
                // ignored
                //Groups = ,
                //LockoutEnabled = ,
                //PhoneNumber = ,
                //PhoneNumberConfirmed = ,
                //TwoFactorEnabled = ,

                Id = source.Id, // also in ctor but required, BackOfficeIdentityUser is weird
                CalculatedMediaStartNodeIds = source.CalculateMediaStartNodeIds(_entityService),
                CalculatedContentStartNodeIds = source.CalculateContentStartNodeIds(_entityService),
                Email = source.Email,
                UserName = source.Username,
                LastPasswordChangeDateUtc = source.LastPasswordChangeDate.ToUniversalTime(),
                LastLoginDateUtc = source.LastLoginDate.ToUniversalTime(),
                EmailConfirmed = source.EmailConfirmedDate.HasValue,
                Name = source.Name,
                AccessFailedCount = source.FailedPasswordAttempts,
                PasswordHash = GetPasswordHash(source.RawPasswordValue),
                StartContentIds = source.StartContentIds,
                StartMediaIds = source.StartMediaIds,
                Culture = source.GetUserCulture(_textService, _globalSettings).ToString(), // project CultureInfo to string
                IsApproved = source.IsApproved,
                SecurityStamp = source.SecurityStamp,
                LockoutEndDateUtc = source.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?)null,

                // this was in AutoMapper but does not have a setter anyways
                //AllowedSections = source.AllowedSections.ToArray(),

                // these were marked as ignored for AutoMapper but don't have a setter anyways
                //Logins =,
                //Claims =,
                //Roles =,
            };

            target.ResetDirtyProperties(true);
            target.EnableChangeTracking(); // fixme but how can we disable it?

            return target;
        }

        private static string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith(Constants.Security.EmptyPasswordPrefix) ? null : storedPass;
        }
    }
}
