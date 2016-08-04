using System.Collections.Generic;
using Umbraco.Core;
using umbraco.BusinessLogic;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Provides umbraco user permission functionality on various nodes. Only nodes that are published are queried via the cache.
    /// </summary>    
    public class UserPermissions : UserGroupPermissionsBase
    {
        readonly User _user;

        public UserPermissions(User user)
        {
            _user = user;
        }

        protected override string GetPermissions(string path)
        {
            return _user.GetPermissions(path);
        }

        protected override void AssignPermissions(IEnumerable<char> permissions, params int[] entityIds)
        {
            ApplicationContext.Current.Services.UserService.ReplaceUserPermissions(_user.Id, permissions, entityIds);
        }
    }
}