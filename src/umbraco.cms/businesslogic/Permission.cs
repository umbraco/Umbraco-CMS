using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

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
        
        public static void MakeNew(IUser User, CMSNode Node, char PermissionKey)
        {
            MakeNew(User, new[] { Node }, PermissionKey);
        }

        private static void MakeNew(IUser user, IEnumerable<CMSNode> nodes, char permissionKey)
        {
            var asArray = nodes.ToArray();

            ApplicationContext.Current.Services.UserService.AssignUserPermission(user.Id, permissionKey, asArray.Select(x => x.Id).ToArray());

        }        

        /// <summary>
        /// Returns the permissions for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<Permission> GetUserPermissions(IUser user)
        {
            var permissions = ApplicationContext.Current.Services.UserService.GetPermissions(user);

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
        public static void DeletePermissions(IUser user, CMSNode node)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserPermissions(user.Id, node.Id);
        }
        
        /// <summary>
        /// deletes all permissions for the user
        /// </summary>
        /// <param name="user"></param>
        public static void DeletePermissions(IUser user)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserPermissions(user.Id);

        }

        public static void DeletePermissions(int iUserID, int[] iNodeIDs)
        {
            ApplicationContext.Current.Services.UserService.RemoveUserPermissions(iUserID, iNodeIDs);

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
            
        }

        public static void UpdateCruds(IUser user, CMSNode node, string permissions)
        {
            ApplicationContext.Current.Services.UserService.ReplaceUserPermissions(
                user.Id, 
                permissions.ToCharArray(), 
                node.Id);

        }

   

    }

    internal class UserPermission
    {
        private int? _userId;
        private readonly int[] _nodeIds;
        
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

        internal IUser User { get; private set; }
        internal IEnumerable<CMSNode> Nodes { get; private set; }
        internal char[] PermissionKeys { get; private set; }
    }
}