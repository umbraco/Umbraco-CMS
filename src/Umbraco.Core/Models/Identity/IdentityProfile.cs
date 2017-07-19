using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.Identity
{
    public class IdentityProfile : Profile
    {
        public IdentityProfile(ILocalizedTextService textService)
        {
            CreateMap<IUser, BackOfficeIdentityUser>()
                .ForMember(dest => dest.LastLoginDateUtc, opt => opt.MapFrom(src => src.LastLoginDate.ToUniversalTime()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LockoutEndDateUtc, opt => opt.MapFrom(src => src.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?) null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(user => GetPasswordHash(user.RawPasswordValue)))
                .ForMember(dest => dest.Culture, opt => opt.MapFrom(src => src.GetUserCulture(textService)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StartMediaId, opt => opt.MapFrom(src => src.StartMediaId))
                .ForMember(dest => dest.StartContentId, opt => opt.MapFrom(src => src.StartContentId))
                .ForMember(dest => dest.UserTypeAlias, opt => opt.MapFrom(src => src.UserType.Alias))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => src.FailedPasswordAttempts))
                .ForMember(dest => dest.AllowedSections, opt => opt.MapFrom(src => src.AllowedSections.ToArray()))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.Logins, opt => opt.Ignore())
                .ForMember(dest => dest.LoginsChanged, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Claims, opt => opt.Ignore());

            CreateMap<BackOfficeIdentityUser, UserData>()
                .ConstructUsing(source => new UserData(Guid.NewGuid().ToString("N"))) //this is the 'session id'
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AllowedApplications, opt => opt.MapFrom(src => src.AllowedSections))
                .ForMember(dest => dest.RealName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(user => new[] { user.UserTypeAlias }))
                .ForMember(dest => dest.StartContentNode, opt => opt.MapFrom(src => src.StartContentId))
                .ForMember(dest => dest.StartMediaNode, opt => opt.MapFrom(src => src.StartMediaId))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Culture, opt => opt.MapFrom(src => src.Culture))
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SecurityStamp.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString("N") : src.SecurityStamp));
        }

        private static string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith("___UIDEMPTYPWORD__") ? null : storedPass;
        }
    }
}