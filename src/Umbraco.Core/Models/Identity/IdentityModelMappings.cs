using System;
using System.Linq;
using AutoMapper;

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
                .ForMember(user => user.LastLoginDateUtc, expression => expression.MapFrom(user => user.LastLoginDate.ToUniversalTime()))
                .ForMember(user => user.Email, expression => expression.MapFrom(user => user.Email))
                .ForMember(user => user.EmailConfirmed, expression => expression.MapFrom(user => user.EmailConfirmedDate.HasValue))
                .ForMember(user => user.Id, expression => expression.MapFrom(user => user.Id))
                .ForMember(user => user.LockoutEndDateUtc, expression => expression.MapFrom(user => user.IsLockedOut ? DateTime.MaxValue.ToUniversalTime() : (DateTime?)null))                
                .ForMember(user => user.UserName, expression => expression.MapFrom(user => user.Username))
                .ForMember(user => user.PasswordHash, expression => expression.MapFrom(user => GetPasswordHash(user.RawPasswordValue)))
                .ForMember(user => user.Culture, expression => expression.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(user => user.Name, expression => expression.MapFrom(user => user.Name))
                .ForMember(user => user.StartMediaIds, expression => expression.MapFrom(user => user.StartMediaIds))
                .ForMember(user => user.StartContentIds, expression => expression.MapFrom(user => user.StartContentIds))
                .ForMember(user => user.AccessFailedCount, expression => expression.MapFrom(user => user.FailedPasswordAttempts))
                .ForMember(user => user.Groups, expression => expression.MapFrom(user => user.Groups.ToArray()))
                .ForMember(user => user.AllowedSections, expression => expression.MapFrom(user => user.AllowedSections.ToArray()));

            config.CreateMap<BackOfficeIdentityUser, UserData>()
                .ConstructUsing((BackOfficeIdentityUser user) => new UserData(Guid.NewGuid().ToString("N"))) //this is the 'session id'
                .ForMember(detail => detail.Id, opt => opt.MapFrom(user => user.Id))
                .ForMember(detail => detail.AllowedApplications, opt => opt.MapFrom(user => user.AllowedSections))
                .ForMember(detail => detail.Roles, opt => opt.MapFrom(user => user.Groups))
                .ForMember(detail => detail.RealName, opt => opt.MapFrom(user => user.Name))
                //When mapping to UserData which is used in the authcookie we want ALL start nodes including ones defined on the groups
                .ForMember(detail => detail.StartContentNodes, opt => opt.MapFrom(user => user.GetCombinedStartContentIds()))
                //When mapping to UserData which is used in the authcookie we want ALL start nodes including ones defined on the groups
                .ForMember(detail => detail.StartMediaNodes, opt => opt.MapFrom(user => user.GetCombinedStartMediaIds()))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.UserName))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.Culture))
                .ForMember(detail => detail.SessionId, opt => opt.MapFrom(user => user.SecurityStamp.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString("N") : user.SecurityStamp));
        }

        private string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith(Constants.Security.EmptyPasswordPrefix) ? null : storedPass;
        }
    }
}