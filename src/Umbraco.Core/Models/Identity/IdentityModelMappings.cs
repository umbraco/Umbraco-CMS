using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;

namespace Umbraco.Core.Models.Identity
{
    public class IdentityModelMappings : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IUser, BackOfficeIdentityUser>()
                .BeforeMap((user, identityUser) =>
                {
                    identityUser.DisableChangeTracking();
                })
                .ConstructUsing(user => new BackOfficeIdentityUser(user.Id, user.Groups))
                .ForMember(user => user.LastLoginDateUtc, expression => expression.MapFrom(user => user.LastLoginDate.ToUniversalTime()))
                .ForMember(user => user.LastPasswordChangeDateUtc, expression => expression.MapFrom(user => user.LastPasswordChangeDate.ToUniversalTime()))
                .ForMember(user => user.Email, expression => expression.MapFrom(user => user.Email))
                .ForMember(user => user.EmailConfirmed, expression => expression.MapFrom(user => user.EmailConfirmedDate.HasValue))
                .ForMember(user => user.Id, expression => expression.MapFrom(user => user.Id))
                .ForMember(user => user.LockoutEndDateUtc, expression => expression.MapFrom(user => user.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?)null))                
                .ForMember(user => user.IsApproved, expression => expression.MapFrom(user => user.IsApproved))
                .ForMember(user => user.UserName, expression => expression.MapFrom(user => user.Username))
                .ForMember(user => user.PasswordHash, expression => expression.MapFrom(user => GetPasswordHash(user.RawPasswordValue)))
                .ForMember(user => user.Culture, expression => expression.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(user => user.Name, expression => expression.MapFrom(user => user.Name))
                .ForMember(user => user.StartMediaIds, expression => expression.MapFrom(user => user.StartMediaIds))
                .ForMember(user => user.StartContentIds, expression => expression.MapFrom(user => user.StartContentIds))
                .ForMember(user => user.AccessFailedCount, expression => expression.MapFrom(user => user.FailedPasswordAttempts))
                .ForMember(user => user.CalculatedContentStartNodeIds, expression => expression.MapFrom(user => user.CalculateContentStartNodeIds(applicationContext.Services.EntityService)))
                .ForMember(user => user.CalculatedMediaStartNodeIds, expression => expression.MapFrom(user => user.CalculateMediaStartNodeIds(applicationContext.Services.EntityService)))
                .ForMember(user => user.AllowedSections, expression => expression.MapFrom(user => user.AllowedSections.ToArray()))

                .ForMember(user => user.LockoutEnabled, expression => expression.Ignore())
                .ForMember(user => user.Logins, expression => expression.Ignore())
                .ForMember(user => user.Roles, expression => expression.Ignore())
                .ForMember(user => user.PhoneNumber, expression => expression.Ignore())
                .ForMember(user => user.PhoneNumberConfirmed, expression => expression.Ignore())
                .ForMember(user => user.TwoFactorEnabled, expression => expression.Ignore())
                .ForMember(user => user.Claims, expression => expression.Ignore())

                .AfterMap((user, identityUser) =>
                {
                    identityUser.ResetDirtyProperties(true);
                    identityUser.EnableChangeTracking();
                });
            
        }

        private string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith(Constants.Security.EmptyPasswordPrefix) ? null : storedPass;
        }
    }
}
