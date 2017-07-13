using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Umbraco.Core;
using Umbraco.Core.Events;
using umbraco.DataLayer;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using DeleteEventArgs = umbraco.cms.businesslogic.DeleteEventArgs;

namespace umbraco.BusinessLogic
{
    [Obsolete("This is no longer used and will be removed in future versions, use the IUserService to manage permissions for Users")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Permission
    {

        public int NodeId { get; private set; }
        public int UserGroupId { get; private set; }
        public char PermissionId { get; private set; }

        /// <summary>
        /// Private constructor, this class cannot be directly instantiated
        /// </summary>
        private Permission() { }
        
        public static void MakeNew(IUserGroup userGroup, CMSNode node, char permissionKey)
        {
            ApplicationContext.Current.Services.UserService.AssignUserGroupPermission(
                userGroup.Id, permissionKey, node.Id);
        }

        /// <summary>
        /// Returns the permissions for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<Permission> GetUserPermissions(User user)
        {
            var permissions = ApplicationContext.Current.Services.UserService.GetPermissions(user.UserEntity);

            return permissions.SelectMany(
                entityPermission => entityPermission.AssignedPermissions,
                (entityPermission, assignedPermission) => new Permission
                {
                    NodeId = entityPermission.EntityId,
                    PermissionId = assignedPermission[0],
                });

        }

        /// <summary>
        /// Returns the permissions for a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<Permission> GetNodePermissions(CMSNode node)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(node.Id);
            if (content == null) return Enumerable.Empty<Permission>();

            var permissions = ApplicationContext.Current.Services.ContentService.GetPermissionsForEntity(content);

            return permissions.SelectMany(
                entityPermission => entityPermission.AssignedPermissions,
                (entityPermission, assignedPermission) => new Permission
                {
                    NodeId = entityPermission.EntityId,
                    PermissionId = assignedPermission[0],
                    UserGroupId = entityPermission.UserGroupId,
                });
        }

        /// <summary>
        /// Delets all permissions for the node/user group combination
        /// </summary>
        /// <param name="userGroup"></param>
        /// <param name="node"></param>
        public static void DeletePermissions(IUserGroup userGroup, CMSNode node)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserGroupPermissions(userGroup.Id, node.Id);
        }

        /// <summary>
        /// deletes all permissions for the user group
        /// </summary>
        /// <param name="userGroup"></param>
        public static void DeletePermissions(IUserGroup userGroup)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserGroupPermissions(userGroup.Id);
            
        }

        public static void DeletePermissions(int userGroupId, int[] iNodeIDs)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserGroupPermissions(userGroupId, iNodeIDs);
            
        }
        public static void DeletePermissions(int userGroupId, int iNodeID)
        {
            DeletePermissions(userGroupId, new[] { iNodeID });
        }

        /// <summary>
        /// delete all permissions for this node
        /// </summary>
        /// <param name="node"></param>
        public static void DeletePermissions(CMSNode node)
        {
            ApplicationContext.Current.Services.ContentService.RemoveContentPermissions(node.Id);
            
        }

        public static void UpdateCruds(IUserGroup userGroup, CMSNode node, string permissions)
        {
            ApplicationContext.Current.Services.UserService.ReplaceUserGroupPermissions(
                userGroup.Id, 
                permissions.ToCharArray(), 
                node.Id);
            
        }

    }

}