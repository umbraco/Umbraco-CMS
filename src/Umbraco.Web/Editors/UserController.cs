using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;

using legacyUser = umbraco.BusinessLogic.User;


namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Controller to back the User.Resource service, used for fetching user data when already authenticated. user.service is currently used for handling authentication
    /// </summary>
    [PluginController("UmbracoApi")]
    public class UserController : UmbracoAuthorizedJsonController
    {
        private readonly UserModelMapper _userModelMapper;
        public UserController()
            : this(new UserModelMapper())
        {            
        }
        internal UserController(UserModelMapper userModelMapper)
        {
            _userModelMapper = userModelMapper;
        }

        public UserDetail GetById(int id)
        {
            var user = Services.UserService.GetUserById(id);
            return _userModelMapper.ToUserDetail(user);
        }

        //TODO: Change to a service / repo
        public IEnumerable<UserBasic> GetAll()
        {
            return legacyUser.getAll().Where(x => !x.Disabled).Select(x => new UserBasic() { Name = x.Name, UserId = x.Id });
        }
    }
}
