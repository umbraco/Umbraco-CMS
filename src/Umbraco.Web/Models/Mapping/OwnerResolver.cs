using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Maps the Owner for IContentBase
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    internal class OwnerResolver<TPersisted>
        where TPersisted : IContentBase
    {
        private readonly IUserService _userService;

        public OwnerResolver(IUserService userService)
        {
            _userService = userService;
        }

        public UserProfile Resolve(TPersisted source)
        {
            var profile = source.GetCreatorProfile(_userService);
            return profile == null ? null : Mapper.Map<IProfile, UserProfile>(profile);
        }
    }
}
