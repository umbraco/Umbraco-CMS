using System;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class UserModelMapper
    {
        public UserDetail ToUserDetail(IUser user)
        {
            var detail = new UserDetail
            {
                Name = user.Name,
                Email = user.Email,
                Language = user.Language
            };
            var result = user.Id.TryConvertTo<int>();
            if (result.Success == false)
            {
                throw new InvalidOperationException("Cannot convert the profile to a " + typeof(UserDetail).Name + " object since the id is not an integer");
            }
            detail.UserId = result.Result;
            return detail;
        }

        public UserBasic ToUserBasic(IProfile profile)
        {
            var user = new UserBasic
                {
                    Name = profile.Name
                };
            var result = profile.Id.TryConvertTo<int>();
            if (result.Success == false)
            {
                throw new InvalidOperationException("Cannot convert the profile to a " + typeof(UserBasic).Name + " object since the id is not an integer");
            }
            user.UserId = result.Result;
            return user;
        }
    }
}