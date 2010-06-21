using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using umbraco;
using umbraco.BusinessLogic;
using System.Web;
using umbraco.BusinessLogic.Actions;
using umbraco.DataLayer;
using umbraco.cms.businesslogic;
using umbraco.interfaces;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Provides umbraco user permission functionality on various nodes. Only nodes that are published are queried via the cache.
    /// </summary>    
    public class UserPermissions
    {

        User m_user;

        public UserPermissions(User user)
        {
            m_user = user;
        }

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// saves new permissions with the parameters supplied
        /// </summary>
        /// <param name="nodeIDs"></param>
        /// <param name="actions"></param>
        /// <param name="replaceChildren"></param>
        public void SaveNewPermissions(int[] nodeIDs, List<umbraco.interfaces.IAction> actions, bool replaceChildren)
        {
            //ensure permissions that are permission assignable
            List<IAction> permissions = actions.FindAll(
                delegate(IAction a)
                {
                    return (a.CanBePermissionAssigned);
                }
            );

            //ensure that only the nodes that the user has permissions to update are updated
            List<int> lstNoPermissions = new List<int>();
            foreach (int nodeID in nodeIDs)
            {
                string nodeActions = UmbracoEnsuredPage.CurrentUser.GetPermissions(GetNodePath(nodeID));
                List<IAction> lstActions = umbraco.BusinessLogic.Actions.Action.FromString(nodeActions);
                if (lstActions == null || !lstActions.Contains(ActionRights.Instance))
                    lstNoPermissions.Add(nodeID);
            }
            //remove the nodes that the user doesn't have permission to update
            List<int> lstNodeIDs = new List<int>();
            lstNodeIDs.AddRange(nodeIDs);
            foreach (int noPermission in lstNoPermissions)
                lstNodeIDs.Remove(noPermission);
            nodeIDs = lstNodeIDs.ToArray();

            //get the complete list of node ids that this change will affect
            List<int> allNodes = new List<int>();
            if (replaceChildren)
                foreach (int nodeID in nodeIDs)
                {
                    allNodes.Add(nodeID);
                    allNodes.AddRange(FindChildNodes(nodeID));
                }
            else
                allNodes.AddRange(nodeIDs);

            //First remove all permissions for all nodes in question       
            Permission.DeletePermissions(m_user.Id, allNodes.ToArray());

            //if permissions are to be assigned, then assign them
            if (permissions.Count > 0)
                foreach (umbraco.interfaces.IAction oPer in permissions)
                    InsertPermissions(allNodes.ToArray(), oPer);
            else
            {
                //If there are NO permissions for this node, we need to assign the ActionNull permission otherwise
                //the node will inherit from it's parent.
                InsertPermissions(nodeIDs, ActionNull.Instance);
            }

            //clear umbraco cache (this is the exact syntax umbraco uses... which should be a public method).
            HttpRuntime.Cache.Remove(string.Format("UmbracoUser{0}", m_user.Id.ToString()));
            //TODO:can also set a user property which will flush the cache!
        }

        /// <summary>
        /// Returns the current user permissions for the node specified
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public List<IAction> GetExistingNodePermission(int nodeID)
        {
            string path = GetNodePath(nodeID);
            if (path != "")
            {
                //get the user and their permissions
                string permissions = m_user.GetPermissions(path);
                return umbraco.BusinessLogic.Actions.Action.FromString(permissions);
            }
            return null;
        }

        /// <summary>
        /// gets path attribute for node id passed
        /// </summary>
        /// <param name="iNodeID"></param>
        /// <returns></returns>
        private string GetNodePath(int iNodeID)
        {
            if (Document.IsDocument(iNodeID))
            {
                Document doc = new Document(iNodeID);
                return doc.Path;
            } 
            
            return "";
        }

        /// <summary>
        /// Finds all child node IDs
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        private List<int> FindChildNodes(int nodeID)
        {
            Document[] docs = Document.GetChildrenForTree(nodeID);
            List<int> nodeIds = new List<int>();
            foreach (Document doc in docs)
            {
                nodeIds.Add(doc.Id);
                if (doc.HasChildren)
                {
                    nodeIds.AddRange(FindChildNodes(doc.Id));
                }
            }
            return nodeIds;
        }

        private void InsertPermissions(int[] nodeIDs, IAction permission)
        {
            foreach (int i in nodeIDs)
                InsertPermission(i, permission);
        }

        private void InsertPermission(int nodeID, IAction permission)
        {
            //create a new CMSNode object but don't initialize (this prevents a db query)
            CMSNode node = new CMSNode(nodeID, false);
            Permission.MakeNew(m_user, node, permission.Letter);
        }

        

    }
}