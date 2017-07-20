using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

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

        public UserBasic Resolve(IContent source)
        {
            return Mapper.Map<IProfile, UserBasic>(source.GetWriterProfile(_userService));
        }
    }
}
