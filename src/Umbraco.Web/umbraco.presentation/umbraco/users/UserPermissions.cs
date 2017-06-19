using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.Actions;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Provides umbraco user permission functionality on various nodes. Only nodes that are published are queried via the cache.
    /// </summary>    
    public class UserPermissions
    {
        readonly IUser _user;

        public UserPermissions(IUser user)
        {
            _user = user;
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
                var nodeActions = Current.Services.UserService.GetPermissions(UmbracoContext.Current.Security.CurrentUser, GetNodePath(nodeId));
                
                var lstActions = Action.FromEntityPermission(nodeActions);
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
                Current.Services.UserService.ReplaceUserPermissions(
                    _user.Id, permissions.Select(x => x.Letter), allNodes.ToArray());
            }
            else
            {
                //If there are NO permissions for this node, we need to assign the ActionNull permission otherwise
                //the node will inherit from it's parent.
                Current.Services.UserService.ReplaceUserPermissions(
                    _user.Id, new[] { ActionNull.Instance.Letter }, allNodes.ToArray());
            }            

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

                var permissions = Current.Services.UserService.GetPermissions(_user, path);
                return Action.FromEntityPermission(permissions);
            }
            return null;
        }

        /// <summary>
        /// gets path attribute for node id passed
        /// </summary>
        /// <param name="iNodeId"></param>
        /// <returns></returns>
        private static string GetNodePath(int iNodeId)
        {
            var e = Current.Services.EntityService.Get(iNodeId, UmbracoObjectTypes.Document);
            return e == null ? string.Empty : e.Path;
        }

        /// <summary>
        /// Finds all child node IDs
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private static IEnumerable<int> FindChildNodes(int nodeId)
        {
            var docs = Current.Services.EntityService.GetChildren(nodeId, UmbracoObjectTypes.Document);
            var nodeIds = new List<int>();
            foreach (var doc in docs)
            {
                nodeIds.Add(doc.Id);
                nodeIds.AddRange(FindChildNodes(doc.Id));
            }
            return nodeIds;
        }
    }
}