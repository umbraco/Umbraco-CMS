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
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using DeleteEventArgs = umbraco.cms.businesslogic.DeleteEventArgs;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Summary description for Permission.
    /// </summary>
    public class Permission
    {

        public int NodeId { get; private set; }
        public int UserId { get; private set; }
        public char PermissionId { get; private set; }

        /// <summary>
        /// Private constructor, this class cannot be directly instantiated
        /// </summary>
        private Permission() { }
        
        public static void MakeNew(User User, CMSNode Node, char PermissionKey)
        {
            MakeNew(User, Node, PermissionKey, true);
        }

        private static void MakeNew(User user, IEnumerable<CMSNode> nodes, char permissionKey, bool raiseEvents)
        {
            var asArray = nodes.ToArray();

            ApplicationContext.Current.Services.UserService.AssignUserPermission(user.Id, permissionKey, asArray.Select(x => x.Id).ToArray());

            if (raiseEvents)
            {
                OnNew(new UserPermission(user, asArray, new[] { permissionKey }), new NewEventArgs());
            }
        }

        private static void MakeNew(User User, CMSNode Node, char PermissionKey, bool raiseEvents)
        {
            MakeNew(User, new[] {Node}, PermissionKey, raiseEvents);
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
                    UserId = entityPermission.UserId
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
                    UserId = entityPermission.UserId
                });
        }

        /// <summary>
        /// Delets all permissions for the node/user combination
        /// </summary>
        /// <param name="user"></param>
        /// <param name="node"></param>
        public static void DeletePermissions(User user, CMSNode node)
        {
            DeletePermissions(user, node, true);
        }

        internal static void DeletePermissions(User user, CMSNode node, bool raiseEvents)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserPermissions(user.Id, node.Id);
            if (raiseEvents)
            {
                OnDeleted(new UserPermission(user, node, null), new DeleteEventArgs());
            }
        }

        /// <summary>
        /// deletes all permissions for the user
        /// </summary>
        /// <param name="user"></param>
        public static void DeletePermissions(User user)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserPermissions(user.Id);

            OnDeleted(new UserPermission(user, Enumerable.Empty<CMSNode>(), null), new DeleteEventArgs());
        }

        public static void DeletePermissions(int iUserID, int[] iNodeIDs)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserPermissions(iUserID, iNodeIDs);

            OnDeleted(new UserPermission(iUserID, iNodeIDs), new DeleteEventArgs());
        }
        public static void DeletePermissions(int iUserID, int iNodeID)
        {
            DeletePermissions(iUserID, new[] { iNodeID });
        }

        /// <summary>
        /// delete all permissions for this node
        /// </summary>
        /// <param name="node"></param>
        public static void DeletePermissions(CMSNode node)
        {
            ApplicationContext.Current.Services.ContentService.RemoveContentPermissions(node.Id);
            
            OnDeleted(new UserPermission(null, node, null), new DeleteEventArgs());
        }

        public static void UpdateCruds(User user, CMSNode node, string permissions)
        {
            ApplicationContext.Current.Services.UserService.ReplaceUserPermissions(
                user.Id, 
                permissions.ToCharArray(), 
                node.Id);

            OnUpdated(new UserPermission(user, node, permissions.ToCharArray()), new SaveEventArgs());
        }

        internal static event TypedEventHandler<UserPermission, DeleteEventArgs> Deleted;
        private static void OnDeleted(UserPermission permission, DeleteEventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(permission, args);
            }
        }

        internal static event TypedEventHandler<UserPermission, SaveEventArgs> Updated;
        private static void OnUpdated(UserPermission permission, SaveEventArgs args)
        {
            if (Updated != null)
            {
                Updated(permission, args);
            }
        }

        internal static event TypedEventHandler<UserPermission, NewEventArgs> New;
        private static void OnNew(UserPermission permission, NewEventArgs args)
        {
            if (New != null)
            {
                New(permission, args);
            }
        }

    }

    internal class UserPermission
    {
        private int? _userId;
        private readonly int[] _nodeIds;

        internal UserPermission(int userId)
        {
            _userId = userId;
        }

        internal UserPermission(int userId, IEnumerable<int> nodeIds)
        {
            _userId = userId;
            _nodeIds = nodeIds.ToArray();
        }

        internal UserPermission(User user, CMSNode node, char[] permissionKeys)
        {
            User = user;
            Nodes = new[] { node };
            PermissionKeys = permissionKeys;
        }

        internal UserPermission(User user, IEnumerable<CMSNode> nodes, char[] permissionKeys)
        {
            User = user;
            Nodes = nodes;
            PermissionKeys = permissionKeys;
        }

        internal int UserId
        {
            get
            {
                if (_userId.HasValue)
                {
                    return _userId.Value;
                }
                if (User != null)
                {
                    return User.Id;
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

        internal User User { get; private set; }
        internal IEnumerable<CMSNode> Nodes { get; private set; }
        internal char[] PermissionKeys { get; private set; }
    }
}