using System;
using System.Linq;
using AutoMapper;

using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models.Identity
{
    public class IdentityModelMappings : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IUser, BackOfficeIdentityUser>()
                .ForMember(user => user.Email, expression => expression.MapFrom(user => user.Email))
                .ForMember(user => user.Id, expression => expression.MapFrom(user => user.Id))
                .ForMember(user => user.LockoutEnabled, expression => expression.MapFrom(user => user.IsLockedOut))
                .ForMember(user => user.LockoutEndDateUtc, expression => expression.UseValue(DateTime.MaxValue.ToUniversalTime()))
                .ForMember(user => user.UserName, expression => expression.MapFrom(user => user.Username))
                .ForMember(user => user.PasswordHash, expression => expression.MapFrom(user => GetPasswordHash(user.RawPasswordValue)))
                .ForMember(user => user.Culture, expression => expression.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(user => user.Name, expression => expression.MapFrom(user => user.Name))
                .ForMember(user => user.StartMediaId, expression => expression.MapFrom(user => user.StartMediaId))
                .ForMember(user => user.StartContentId, expression => expression.MapFrom(user => user.StartContentId))
                .ForMember(user => user.UserTypeAlias, expression => expression.MapFrom(user => user.UserType.Alias))
                .ForMember(user => user.AllowedSections, expression => expression.MapFrom(user => user.AllowedSections.ToArray()));
        }

        private string GetPasswordHash(string storedPass)
        {
            return storedPass.StartsWith("___UIDEMPTYPWORD__") ? null : storedPass;
        }
    }
}