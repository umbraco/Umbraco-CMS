using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using umbraco;

namespace Umbraco.Web.Models.Mapping
{
    internal class UserModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IUser, UserDetail>()
                  .ForMember(detail => detail.UserId, opt => opt.MapFrom(user => GetIntId(user.Id)))
                  .ForMember(detail => detail.UserType, opt => opt.MapFrom(user => user.UserType.Alias))
                  .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => ui.Culture(user)))
                  .ForMember(
                      detail => detail.EmailHash,
                      opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().ToMd5()));

            config.CreateMap<IProfile, UserBasic>()
                  .ForMember(detail => detail.UserId, opt => opt.MapFrom(profile => GetIntId(profile.Id)));
        } 
     
        private static int GetIntId(object id)
        {
            var result = id.TryConvertTo<int>();
            if (result.Success == false)
            {
                throw new InvalidOperationException(
                    "Cannot convert the profile to a " + typeof(UserDetail).Name + " object since the id is not an integer");
            }
            return result.Result;
        } 
 
    }
}