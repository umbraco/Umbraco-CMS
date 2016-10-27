using System.Collections.Generic;
using System.Linq;
using System.Data;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Provides umbraco user group permission functionality on various nodes. Only nodes that are published are queried via the cache.
    /// </summary>    
    public class UserGroupPermissions
    {
        readonly IUserGroup _group;

        public UserGroupPermissions(IUserGroup group)
        {
            _group = group;
        }

        /// <summary>
        /// saves new permissions with the parameters supplied
        /// </summary>
        /// <param name="nodeIDs"></param>
        /// <param name="actions"></param>
        /// <param name="replaceChildren"></param>
        public void SaveNewPermissions(int[] nodeIDs, List<IAction> actions, bool replaceChildren)
        {
            //ensure permissions that are permission assignable
            var permissions = actions.FindAll(a => (a.CanBePermissionAssigned));

            //ensure that only the nodes that the user has permissions to update are updated
            var lstNoPermissions = new List<int>();
            foreach (var nodeId in nodeIDs)
            {
                var nodeActions = UmbracoContext.Current.UmbracoUser.GetPermissions(GetNodePath(nodeId));
                var lstActions = BusinessLogic.Actions.Action.FromString(nodeActions);
                if (lstActions == null || !lstActions.Contains(ActionRights.Instance))
                    lstNoPermissions.Add(nodeId);
            }
            //remove the nodes that the user doesn't have permission to update
            var lstNodeIDs = new List<int>();
            lstNodeIDs.AddRange(nodeIDs);
            foreach (var noPermission in lstNoPermissions)
                lstNodeIDs.Remove(noPermission);
            nodeIDs = lstNodeIDs.ToArray();

            //get the complete list of node ids that this change will affect
            var allNodes = new List<int>();
            if (replaceChildren)
            {
                foreach (var nodeId in nodeIDs)
                {
                    allNodes.Add(nodeId);
                    allNodes.AddRange(FindChildNodes(nodeId));
                }
            }
            else
            {
                allNodes.AddRange(nodeIDs);
            }

            //if permissions are to be assigned, then assign them
            if (permissions.Count > 0)
            {
                AssignPermissions(permissions.Select(x => x.Letter), allNodes.ToArray());
            }
            else
            {
                //If there are NO permissions for this node, we need to assign the ActionNull permission otherwise
                //the node will inherit from it's parent.
                AssignPermissions(new[] { ActionNull.Instance.Letter }, allNodes.ToArray());
            }
        }

        protected void AssignPermissions(IEnumerable<char> permissions, params int[] entityIds)
        {
            var service = ApplicationContext.Current.Services.UserService;
            service.ReplaceUserGroupPermissions(_group.Id, permissions, entityIds);
        }

        /// <summary>
        /// Returns the current user permissions for the node specified
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public List<IAction> GetExistingNodePermission(int nodeId)
        {
            var path = GetNodePath(nodeId);
            if (path != "")
            {
                //get the user and their permissions
                string permissions = GetPermissions(path);
                return umbraco.BusinessLogic.Actions.Action.FromString(permissions);
            }
            return null;
        }

        private string GetPermissions(string path)
        {
            var userService = ApplicationContext.Current.Services.UserService;
            return userService.GetPermissionsForPath(_group, path);
        }

        /// <summary>
        /// gets path attribute for node id passed
        /// </summary>
        /// <param name="iNodeId"></param>
        /// <returns></returns>
        private static string GetNodePath(int iNodeId)
        {
            if (Document.IsDocument(iNodeId))
            {
                var doc = new Document(iNodeId);
                return doc.Path;
            } 
            
            return "";
        }

        /// <summary>
        /// Finds all child node IDs
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private static IEnumerable<int> FindChildNodes(int nodeId)
        {
            var docs = Document.GetChildrenForTree(nodeId);
            var nodeIds = new List<int>();
            foreach (var doc in docs)
            {
                nodeIds.Add(doc.Id);
                if (doc.HasChildren)
                {
                    nodeIds.AddRange(FindChildNodes(doc.Id));
                }
            }
            return nodeIds;
        }
    }
}