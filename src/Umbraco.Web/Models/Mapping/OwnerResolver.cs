using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

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

        public UserBasic Resolve(TPersisted source)
        {
            return Mapper.Map<IProfile, UserBasic>(source.GetCreatorProfile(_userService));
        }
    }
}