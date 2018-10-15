using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Maps the Creator for content
    /// </summary>
    internal class CreatorResolver
    {
        private readonly IUserService _userService;

        public CreatorResolver(IUserService userService)
        {
            _userService = userService;
        }

        public UserProfile Resolve(IContent source)
        {
            return Mapper.Map<IProfile, UserProfile>(source.GetWriterProfile(_userService));
        }
    }
}
