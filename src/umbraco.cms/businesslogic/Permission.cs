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
            MakeNew(userGroup, node, permissionKey, true);
        }

        private static void MakeNew(IUserGroup userGroup, IEnumerable<CMSNode> nodes, char permissionKey, bool raiseEvents)
        {
            var asArray = nodes.ToArray();

            ApplicationContext.Current.Services.UserService.AssignUserGroupPermission(userGroup.Id, permissionKey, asArray.Select(x => x.Id).ToArray());

            if (raiseEvents)
            {
                OnNew(new UserGroupPermission(userGroup, asArray, new[] { permissionKey }), new NewEventArgs());
            }
        }

        private static void MakeNew(IUserGroup userGroup, CMSNode Node, char PermissionKey, bool raiseEvents)
        {
            MakeNew(userGroup, new[] {Node}, PermissionKey, raiseEvents);
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
            DeletePermissions(userGroup, node, true);
        }

        internal static void DeletePermissions(IUserGroup userGroup, CMSNode node, bool raiseEvents)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserGroupPermissions(userGroup.Id, node.Id);
            if (raiseEvents)
            {
                OnDeleted(new UserGroupPermission(userGroup, node, null), new DeleteEventArgs());
            }
        }

        /// <summary>
        /// deletes all permissions for the user group
        /// </summary>
        /// <param name="userGroup"></param>
        public static void DeletePermissions(IUserGroup userGroup)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserGroupPermissions(userGroup.Id);

            OnDeleted(new UserGroupPermission(userGroup, Enumerable.Empty<CMSNode>(), null), new DeleteEventArgs());
        }

        public static void DeletePermissions(int userGroupId, int[] iNodeIDs)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserGroupPermissions(userGroupId, iNodeIDs);

            OnDeleted(new UserGroupPermission(userGroupId, iNodeIDs), new DeleteEventArgs());
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
            
            OnDeleted(new UserGroupPermission(null, node, null), new DeleteEventArgs());
        }

        public static void UpdateCruds(IUserGroup userGroup, CMSNode node, string permissions)
        {
            ApplicationContext.Current.Services.UserService.ReplaceUserGroupPermissions(
                userGroup.Id, 
                permissions.ToCharArray(), 
                node.Id);

            OnUpdated(new UserGroupPermission(userGroup, node, permissions.ToCharArray()), new SaveEventArgs());
        }

        internal static event TypedEventHandler<UserGroupPermission, DeleteEventArgs> Deleted;
        private static void OnDeleted(UserGroupPermission permission, DeleteEventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(permission, args);
            }
        }

        internal static event TypedEventHandler<UserGroupPermission, SaveEventArgs> Updated;
        private static void OnUpdated(UserGroupPermission permission, SaveEventArgs args)
        {
            if (Updated != null)
            {
                Updated(permission, args);
            }
        }

        internal static event TypedEventHandler<UserGroupPermission, NewEventArgs> New;
        private static void OnNew(UserGroupPermission permission, NewEventArgs args)
        {
            if (New != null)
            {
                New(permission, args);
            }
        }

    }

    internal class UserGroupPermission
    {
        private int? _userGroupId;
        private readonly int[] _nodeIds;

        internal UserGroupPermission(int userGroupId)
        {
            _userGroupId = userGroupId;
        }

        internal UserGroupPermission(int userGroupId, IEnumerable<int> nodeIds)
        {
            _userGroupId = userGroupId;
            _nodeIds = nodeIds.ToArray();
        }

        internal UserGroupPermission(IUserGroup userGroup, CMSNode node, char[] permissionKeys)
        {
            UserGroup = userGroup;
            Nodes = new[] { node };
            PermissionKeys = permissionKeys;
        }

        internal UserGroupPermission(IUserGroup userGroup, IEnumerable<CMSNode> nodes, char[] permissionKeys)
        {
            UserGroup = userGroup;
            Nodes = nodes;
            PermissionKeys = permissionKeys;
        }

        internal int UserId
        {
            get
            {
                if (_userGroupId.HasValue)
                {
                    return _userGroupId.Value;
                }
                if (UserGroup != null)
                {
                    return UserGroup.Id;
                }
                return -1;
            }
        }

        internal IEnumerable<int> NodeIds
        {
            get
            {
                if (_nodeIds != null)
                {
                    return _nodeIds;
                }
                if (Nodes != null)
                {
                    return Nodes.Select(x => x.Id);
                }
                return Enumerable.Empty<int>();
            }
        }

        internal IUserGroup UserGroup { get; private set; }
        internal IEnumerable<CMSNode> Nodes { get; private set; }
        internal char[] PermissionKeys { get; private set; }
    }
}