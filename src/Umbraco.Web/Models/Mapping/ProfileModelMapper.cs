using System;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ProfileModelMapper
    {
        public UserBasic ToBasicUser(IProfile profile)
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