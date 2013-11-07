using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Rdbms;

using umbraco.DataLayer;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
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
             
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }


        public static void MakeNew(User User, CMSNode Node, char PermissionKey)
        {
            MakeNew(User, Node, PermissionKey, true);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void MakeNew(User user, IEnumerable<CMSNode> nodes, char permissionKey, bool raiseEvents)
        {
            var asArray = nodes.ToArray();
            foreach (var node in asArray)
            {
                // Method is synchronized so exists remains consistent (avoiding race condition)
                var exists =
                    ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                        "SELECT COUNT(userId) FROM umbracoUser2nodePermission WHERE userId = @userId AND nodeId = @nodeId AND permission = @permission",
                        new
                        {
                            userId = user.Id,
                            nodeId = node.Id,
                            permission = permissionKey.ToString()
                            
                        }) > 0;


                if (exists) return;

                ApplicationContext.Current.DatabaseContext.Database.Insert(new User2NodePermissionDto
                                                                           {
                                                                               UserId = user.Id,
                                                                               NodeId = node.Id,
                                                                               Permission =
                                                                                   permissionKey
                                                                                   .ToString()
                                                                           });
            }

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
            var items = new List<Permission>();
            var dtos =
                ApplicationContext.Current.DatabaseContext.Database.Fetch<User2NodePermissionDto>(
                    "where userId = @userId order by nodeId",
                    new {userId = user.Id});
            foreach (var dto in dtos)
            {
                items.Add(new Permission()
                          {
                              NodeId = dto.NodeId,
                              PermissionId = Convert.ToChar(dto.Permission),
                              UserId = dto.UserId
                          });
            }

            return items;
        }

        /// <summary>
        /// Returns the permissions for a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<Permission> GetNodePermissions(CMSNode node)
        {
            var items = new List<Permission>();
            var dtos =
                ApplicationContext.Current.DatabaseContext.Database.Fetch<User2NodePermissionDto>(
                    "where nodeId = @nodeId order by nodeId",
                    new {nodeId = node.Id});
            foreach (var dto in dtos)
            {
                items.Add(new Permission()
                          {
                              NodeId = dto.NodeId,
                              PermissionId = Convert.ToChar(dto.Permission),
                              UserId = dto.UserId
                          });
            }

            return items;

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
            // delete all settings on the node for this user
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodePermissionDto>("where userId = @userId and nodeId = @nodeId", new { userId = user.Id, nodeId = node.Id });

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
            // delete all settings on the node for this user
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodePermissionDto>("where userId = @userId", new { userId = user.Id });

            OnDeleted(new UserPermission(user, Enumerable.Empty<CMSNode>(), null), new DeleteEventArgs());
        }

        public static void DeletePermissions(int iUserID, int[] iNodeIDs)
        {
            foreach (var NodeID in iNodeIDs)
            {
                ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodePermissionDto>(
                    "WHERE nodeID = @nodeID AND userID = @userID",
                    new
                    {
                        nodeID = NodeID,
                        userID = iUserID
                        
                    });
            }
 
            OnDeleted(new UserPermission(iUserID, iNodeIDs), new DeleteEventArgs());
        }
        public static void DeletePermissions(int iUserID, int iNodeID)
        {
            DeletePermissions(iUserID, new[] { iNodeID });
        }
        private static string Converter(int from)
        {
            return from.ToString();
        }

        /// <summary>
        /// delete all permissions for this node
        /// </summary>
        /// <param name="node"></param>
        public static void DeletePermissions(CMSNode node)
        {
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodePermissionDto>("where nodeId = @nodeId", new { nodeId = node.Id });

            OnDeleted(new UserPermission(null, node, null), new DeleteEventArgs());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UpdateCruds(User user, CMSNode node, string permissions)
        {
            // delete all settings on the node for this user
            //false = do not raise events
            DeletePermissions(user, node, false);

            // Loop through the permissions and create them
            foreach (char c in permissions)
            {
                //false = don't raise events since we'll raise a custom event after
                MakeNew(user, node, c, false);
            }
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