using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class UserModelMapper
    {
        /// <summary>
        /// Configures the automapper mappings
        /// </summary>
        internal static void Configure()
        {
            Mapper.CreateMap<IUser, UserDetail>()
                  .ForMember(detail => detail.UserId, opt => opt.MapFrom(user => GetIntId(user.Id)))
                  .ForMember(
                      detail => detail.EmailHash,
                      opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().ToMd5()));
            Mapper.CreateMap<IProfile, UserBasic>()
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

        public UserDetail ToUserDetail(IUser user)
        {
            return Mapper.Map<UserDetail>(user);
        }

        public UserBasic ToUserBasic(IProfile profile)
        {
            return Mapper.Map<UserBasic>(profile);
        }
    }
}