using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Identity
{
    public class IdentityProfile : Profile
    {
        public IdentityProfile(ILocalizedTextService textService, IEntityService entityService)
        {
            CreateMap<IUser, BackOfficeIdentityUser>()
                .BeforeMap((src, dest) =>
                {
                    dest.DisableChangeTracking();
                })
                .ConstructUsing(src => new BackOfficeIdentityUser(src.Id, src.Groups))
                .ForMember(dest => dest.LastLoginDateUtc, opt => opt.MapFrom(src => src.LastLoginDate.ToUniversalTime()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmedDate.HasValue))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LockoutEndDateUtc, opt => opt.MapFrom(src => src.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?) null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(user => GetPasswordHash(user.RawPasswordValue)))
                .ForMember(dest => dest.Culture, opt => opt.MapFrom(src => src.GetUserCulture(textService)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StartMediaIds, opt => opt.MapFrom(src => src.StartMediaIds))
                .ForMember(dest => dest.StartContentIds, opt => opt.MapFrom(src => src.StartContentIds))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => src.FailedPasswordAttempts))
                .ForMember(dest => dest.CalculatedContentStartNodeIds, opt => opt.MapFrom(src => src.CalculateContentStartNodeIds(entityService)))
                .ForMember(dest => dest.CalculatedMediaStartNodeIds, opt => opt.MapFrom(src => src.CalculateMediaStartNodeIds(entityService)))
                .ForMember(dest => dest.AllowedSections, opt => opt.MapFrom(src => src.AllowedSections.ToArray()))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.Logins, opt => opt.Ignore())
                .ForMember(dest => dest.LoginsChanged, opt => opt.Ignore())
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

            CreateMap<BackOfficeIdentityUser, UserData>()
                .ConstructUsing(source => new UserData(Guid.NewGuid().ToString("N"))) //this is the 'session id'
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AllowedApplications, opt => opt.MapFrom(src => src.AllowedSections))
                //When mapping to UserData which is used in the authcookie we want ALL start nodes including ones defined on the groups
                .ForMember(dest => dest.StartContentNodes, opt => opt.MapFrom(src => src.CalculatedContentStartNodeIds))
                //When mapping to UserData which is used in the authcookie we want ALL start nodes including ones defined on the groups
                .ForMember(dest => dest.StartMediaNodes, opt => opt.MapFrom(src => src.CalculatedMediaStartNodeIds))
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
            return storedPass.StartsWith(Constants.Security.EmptyPasswordPrefix) ? null : storedPass;
        }
    }
}
