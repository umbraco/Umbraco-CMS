using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.Identity
{
    public class IdentityMapperProfile : Profile
    {
        public IdentityMapperProfile(ILocalizedTextService textService, IEntityService entityService, IGlobalSettings globalSettings)
        {
            CreateMap<IUser, BackOfficeIdentityUser>()
                .BeforeMap((src, dest) =>
                {
                    dest.DisableChangeTracking();
                })
                .ConstructUsing(src => new BackOfficeIdentityUser(src.Id, src.Groups))
                .ForMember(dest => dest.LastLoginDateUtc, opt => opt.MapFrom(src => src.LastLoginDate.ToUniversalTime()))
                .ForMember(user => user.LastPasswordChangeDateUtc, expression => expression.MapFrom(user => user.LastPasswordChangeDate.ToUniversalTime()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmedDate.HasValue))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LockoutEndDateUtc, opt => opt.MapFrom(src => src.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?) null))
                .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.IsApproved))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(user => GetPasswordHash(user.RawPasswordValue)))
                .ForMember(dest => dest.Culture, opt => opt.MapFrom(src => src.GetUserCulture(textService, globalSettings)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StartMediaIds, opt => opt.MapFrom(src => src.StartMediaIds))
                .ForMember(dest => dest.StartContentIds, opt => opt.MapFrom(src => src.StartContentIds))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => src.FailedPasswordAttempts))
                .ForMember(dest => dest.CalculatedContentStartNodeIds, opt => opt.MapFrom(src => src.CalculateContentStartNodeIds(entityService)))
                .ForMember(dest => dest.CalculatedMediaStartNodeIds, opt => opt.MapFrom(src => src.CalculateMediaStartNodeIds(entityService)))
                .ForMember(dest => dest.AllowedSections, opt => opt.MapFrom(src => src.AllowedSections.ToArray()))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.Logins, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Claims, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.ResetDirtyProperties(true);
                    dest.EnableChangeTracking();
                });
        }

        private static string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith(Constants.Security.EmptyPasswordPrefix) ? null : storedPass;
        }
    }
}
