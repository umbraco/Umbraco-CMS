using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Provides umbraco user permission functionality on various nodes. Only nodes that are published are queried via the cache.
    /// </summary>    
    public class UserGroupPermissions : UserGroupPermissionsBase
    {
        readonly IUserGroup _group;

        public UserGroupPermissions(IUserGroup group)
        {
            _group = group;
        }

        protected override string GetPermissions(string path)
        {
            var userService = ApplicationContext.Current.Services.UserService;
            return userService.GetPermissionsForPath(_group, path);
        }

        protected override void AssignPermissions(IEnumerable<char> permissions, params int[] entityIds)
        {
            var service = ApplicationContext.Current.Services.UserService;
            service.ReplaceUserGroupPermissions(_group.Id, permissions, entityIds);
        }
    }
}