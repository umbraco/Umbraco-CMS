#region namespace
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Configuration;
using umbraco.BusinessLogic;
using System.Security.Cryptography;
using System.Web.Util;
using System.Collections.Specialized;
using System.Configuration.Provider;
using umbraco.cms.businesslogic;
using System.Collections;
#endregion

namespace umbraco.presentation.nodeFactory {
    public sealed class UmbracoSiteMapProvider : System.Web.StaticSiteMapProvider {

        private SiteMapNode m_root;
        private int m_indexID, m_indexTitle, m_indexUrl, m_indexDesc, m_indexRoles, m_indexParent;
        private Dictionary<string, SiteMapNode> m_nodes = new Dictionary<string, SiteMapNode>(16);
        private string m_defaultDescriptionAlias = "";
        private bool m_enableSecurityTrimming;

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) {

            if (config == null)
                throw new ArgumentNullException("Config is null");

            if (String.IsNullOrEmpty(name))
                name = "UmbracoSiteMapProvider";

            if (!String.IsNullOrEmpty(config["defaultDescriptionAlias"])) {
                m_defaultDescriptionAlias = config["defaultDescriptionAlias"].ToString();
                config.Remove("defaultDescriptionAlias");
            }
            if (config["securityTrimmingEnabled"] != null) {
                m_enableSecurityTrimming = bool.Parse(config["securityTrimmingEnabled"]);
            }

            base.Initialize(name, config);

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0) {
                string attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                    throw new ProviderException
                    (String.Format("Unrecognized attribute: {0}", attr));
            }

        }



        public override System.Web.SiteMapNode BuildSiteMap() {
            lock (this) {
                if (m_root != null)
                    return m_root;

                m_root = createNode("-1", "root", "umbraco root", "/", null);
                try {
                    AddNode(m_root, null);
                } catch (Exception ex) {
                    BusinessLogic.Log.Add(
                        LogTypes.Error,
                        -1,
                        String.Format("Error adding to SiteMapProvider: {0}", ex));
                }

                loadNodes(m_root.Key, m_root);
            }

            return m_root;
        }

        public void UpdateNode(Node node) {
            lock (this) {
                // validate sitemap
                BuildSiteMap();

                SiteMapNode n;
                if (!m_nodes.ContainsKey(node.Id.ToString())) {
                    n = createNode(node.Id.ToString(),
                        node.Name,
                        node.GetProperty(m_defaultDescriptionAlias) != null ? node.GetProperty(m_defaultDescriptionAlias).Value : "",
                        node.Url,
                        findRoles(node.Id, node.Path));
                    string parentNode = node.Parent == null ? "-1" : node.Parent.Id.ToString();

                    try {
                        AddNode(n, m_nodes[parentNode]);
                    } catch (Exception ex) {
                        BusinessLogic.Log.Add(
                            LogTypes.Error,
                            -1,
                            String.Format("Error adding node with url '{0}' to SiteMapProvider: {1}", node.Name, ex));
                    }
                } else {
                    n = m_nodes[node.Id.ToString()];
                    n.Url = node.Url;
                    n.Description = node.GetProperty(m_defaultDescriptionAlias) != null ? node.GetProperty(m_defaultDescriptionAlias).Value : "";
                    n.Title = node.Name;
                    n.Roles = findRoles(node.Id, node.Path).Split(",".ToCharArray());
                }
            }
        }

        public void RemoveNode(int nodeId) {
            lock (this) {
                if (m_nodes.ContainsKey(nodeId.ToString()))
                    RemoveNode(m_nodes[nodeId.ToString()]);
            }
        }

        private void loadNodes(string parentId, SiteMapNode parentNode) {
            lock (this) {
                Node n = new Node(int.Parse(parentId));
                foreach (Node child in n.Children) {

                    string roles = findRoles(child.Id, child.Path);
                    SiteMapNode childNode = createNode(
                        child.Id.ToString(),
                        child.Name,
                        child.GetProperty(m_defaultDescriptionAlias) != null ? child.GetProperty(m_defaultDescriptionAlias).Value : "",
                        child.Url,
                        roles);
                    try {
                        AddNode(childNode, parentNode);
                    } catch (Exception ex) {
                        BusinessLogic.Log.Add(
                            LogTypes.Error,
                            child.Id,
                            String.Format("Error adding to SiteMapProvider: {0}", ex));
                    }
                    loadNodes(child.Id.ToString(), childNode);
                }
            }
        }

        private string findRoles(int nodeId, string nodePath) {
            // check for roles
            string roles = "";
            if (m_enableSecurityTrimming && !String.IsNullOrEmpty(nodePath) && nodePath.Length > 0) {
                string[] roleArray = cms.businesslogic.web.Access.GetAccessingMembershipRoles(nodeId, nodePath);
                if (roleArray != null)
                    roles = String.Join(",", roleArray);
                else
                    roles = "*";
            }
            return roles;
        }

        protected override SiteMapNode GetRootNodeCore() {
            BuildSiteMap();
            return m_root;
        }

        public override bool IsAccessibleToUser(HttpContext context, SiteMapNode node) {
            if (!m_enableSecurityTrimming)
                return true;

            if (node.Roles == null || node.Roles.Count == -1)
                return true;

            if (node.Roles[0].ToString() == "*")
                return true;

            foreach (string role in node.Roles)
                if (context.User.IsInRole(role))
                    return true;

            return false;
        }

        private SiteMapNode createNode(string id, string name, string description, string url, string roles) {
            lock (this) {
                if (m_nodes.ContainsKey(id))
                    throw new ProviderException(String.Format("A node with id '{0}' already exists", id));

                // Get title, URL, description, and roles from the DataReader

                // If roles were specified, turn the list into a string array
                string[] rolelist = null;
                if (!String.IsNullOrEmpty(roles))
                    rolelist = roles.Split(new char[] { ',', ';' }, 512);

                // Create a SiteMapNode
                SiteMapNode node = new SiteMapNode(this, id, url, name, description, rolelist, null, null, null);

                m_nodes.Add(id, node);

                // Return the node
                return node;
            }
        }


        private SiteMapNode GetParentNode(string id) {

            // Make sure the parent ID is valid
            if (!m_nodes.ContainsKey(id))
                throw new ProviderException(String.Format("No parent with id '{0}' is found", id));
            // Return the parent SiteMapNode
            return m_nodes[id];
        }
    }

    public class UmbracoSiteMapProviderAccessUpdate : BusinessLogic.ApplicationBase
    {
        public UmbracoSiteMapProviderAccessUpdate()
        {
            // Add events to security
            if (System.Web.SiteMap.Provider is UmbracoSiteMapProvider)
            {
                cms.businesslogic.web.Access.AfterAddMemberShipRoleToDocument += new global::umbraco.cms.businesslogic.web.Access.AddMemberShipRoleToDocumentEventHandler(Access_AfterAddMemberShipRoleToDocument);
                cms.businesslogic.web.Access.AfterRemoveMemberShipRoleToDocument += new global::umbraco.cms.businesslogic.web.Access.RemoveMemberShipRoleFromDocumentEventHandler(Access_AfterRemoveMemberShipRoleToDocument);
                cms.businesslogic.web.Access.AfterRemoveProtection += new global::umbraco.cms.businesslogic.web.Access.RemoveProtectionEventHandler(Access_AfterRemoveProtection);
            }

        }

        void Access_AfterRemoveProtection(global::umbraco.cms.businesslogic.web.Document sender, RemoveProtectionEventArgs e)
        {
            ((UmbracoSiteMapProvider)System.Web.SiteMap.Provider).UpdateNode(new Node(sender.Id));
        }


        void Access_AfterRemoveMemberShipRoleToDocument(global::umbraco.cms.businesslogic.web.Document sender, string role, RemoveMemberShipRoleFromDocumentEventArgs e)
        {
            ((UmbracoSiteMapProvider) System.Web.SiteMap.Provider).UpdateNode(new Node(sender.Id));
        }

        void Access_AfterAddMemberShipRoleToDocument(global::umbraco.cms.businesslogic.web.Document sender, string role, AddMemberShipRoleToDocumentEventArgs e)
        {
            ((UmbracoSiteMapProvider) System.Web.SiteMap.Provider).UpdateNode(new Node(sender.Id));
        }
    }
}
