using System.Collections.Generic;
using System.Linq;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using Umbraco.Web;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Provides base functionality for umbraco user and group permission functionality on various nodes. Only nodes that are published are queried via the cache.
    /// </summary>    
    public abstract class UserGroupPermissionsBase
    {
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
                var nodeActions = UmbracoContext.Current.UmbracoUser.GetPermissions(GetNodePath(nodeId), true);
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
                //the node will inherit from it's parent (in the case of users) and to ensure cache refreshes
                AssignPermissions(new[] { ActionNull.Instance.Letter }, allNodes.ToArray());
            }
        }

        protected abstract string GetPermissions(string path);

        protected abstract void AssignPermissions(IEnumerable<char> permissions, params int[] entityIds);

        /// <summary>
        /// Returns the current user permissions for the node specified
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public List<IAction> GetExistingNodePermission(int nodeId)
        {
            var path = GetNodePath(nodeId);
            if (path != string.Empty)
            {
                //get the user/group permissions
                var permissions = GetPermissions(path);
                return umbraco.BusinessLogic.Actions.Action.FromString(permissions);
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