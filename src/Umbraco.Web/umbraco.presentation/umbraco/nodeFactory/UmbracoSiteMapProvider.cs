#region namespace
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Configuration;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using System.Security.Cryptography;
using System.Web.Util;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Collections;
#endregion

namespace umbraco.presentation.nodeFactory
{
    public sealed class UmbracoSiteMapProvider : StaticSiteMapProvider
    {

        private SiteMapNode _root;
        private readonly Dictionary<string, SiteMapNode> _nodes = new Dictionary<string, SiteMapNode>(16);
        private string _defaultDescriptionAlias = "";
        private bool _enableSecurityTrimming;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("Config is null");

            if (string.IsNullOrEmpty(name))
                name = "UmbracoSiteMapProvider";

            if (string.IsNullOrEmpty(config["defaultDescriptionAlias"]) == false)
            {
                _defaultDescriptionAlias = config["defaultDescriptionAlias"];
                config.Remove("defaultDescriptionAlias");
            }
            if (config["securityTrimmingEnabled"] != null)
            {
                _enableSecurityTrimming = bool.Parse(config["securityTrimmingEnabled"]);
            }

            base.Initialize(name, config);

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                string attr = config.GetKey(0);
                if (string.IsNullOrEmpty(attr) == false)
                    throw new ProviderException
                    (string.Format("Unrecognized attribute: {0}", attr));
            }

        }

        public override SiteMapNode BuildSiteMap()
        {
            lock (this)
            {
                if (_root != null)
                    return _root;

                _root = CreateNode("-1", "root", "umbraco root", "/", null);
                try
                {
                    AddNode(_root, null);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<UmbracoSiteMapProvider>("Error adding to SiteMapProvider", ex);
                }

                LoadNodes(_root.Key, _root);
            }

            return _root;
        }

        public void UpdateNode(NodeFactory.Node node)
        {
            lock (this)
            {
                // validate sitemap
                BuildSiteMap();

                SiteMapNode n;
                if (_nodes.ContainsKey(node.Id.ToString()) == false)
                {
                    n = CreateNode(node.Id.ToString(),
                        node.Name,
                        node.GetProperty(_defaultDescriptionAlias) != null ? node.GetProperty(_defaultDescriptionAlias).Value : "",
                        node.Url,
                        FindRoles(node.Id, node.Path));
                    string parentNode = node.Parent == null ? "-1" : node.Parent.Id.ToString();

                    try
                    {
                        AddNode(n, _nodes[parentNode]);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<UmbracoSiteMapProvider>(String.Format("Error adding node with url '{0}' and Id {1} to SiteMapProvider", node.Name, node.Id), ex);
                    }
                }
                else
                {
                    n = _nodes[node.Id.ToString()];
                    n.Url = node.Url;
                    n.Description = node.GetProperty(_defaultDescriptionAlias) != null ? node.GetProperty(_defaultDescriptionAlias).Value : "";
                    n.Title = node.Name;
                    n.Roles = FindRoles(node.Id, node.Path).Split(",".ToCharArray());
                }
            }
        }

        public void RemoveNode(int nodeId)
        {
            lock (this)
            {
                if (_nodes.ContainsKey(nodeId.ToString()))
                    RemoveNode(_nodes[nodeId.ToString()]);
            }
        }

        private void LoadNodes(string parentId, SiteMapNode parentNode)
        {
            lock (this)
            {
                NodeFactory.Node n = new NodeFactory.Node(int.Parse(parentId));
                foreach (NodeFactory.Node child in n.Children)
                {

                    string roles = FindRoles(child.Id, child.Path);
                    SiteMapNode childNode = CreateNode(
                        child.Id.ToString(),
                        child.Name,
                        child.GetProperty(_defaultDescriptionAlias) != null ? child.GetProperty(_defaultDescriptionAlias).Value : "",
                        child.Url,
                        roles);
                    try
                    {
                        AddNode(childNode, parentNode);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<UmbracoSiteMapProvider>(string.Format("Error adding node {0} to SiteMapProvider in loadNodes()", child.Id), ex);
                    }
                    LoadNodes(child.Id.ToString(), childNode);
                }
            }
        }

        private string FindRoles(int nodeId, string nodePath)
        {
            // check for roles
            string roles = "";
            if (_enableSecurityTrimming && !string.IsNullOrEmpty(nodePath) && nodePath.Length > 0)
            {
                string[] roleArray = cms.businesslogic.web.Access.GetAccessingMembershipRoles(nodeId, nodePath);
                if (roleArray != null)
                    roles = String.Join(",", roleArray);
                else
                    roles = "*";
            }
            return roles;
        }

        protected override SiteMapNode GetRootNodeCore()
        {
            BuildSiteMap();
            return _root;
        }

        public override bool IsAccessibleToUser(HttpContext context, SiteMapNode node)
        {
            if (!_enableSecurityTrimming)
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

        private SiteMapNode CreateNode(string id, string name, string description, string url, string roles)
        {
            lock (this)
            {
                if (_nodes.ContainsKey(id))
                    throw new ProviderException(String.Format("A node with id '{0}' already exists", id));
                // Get title, URL, description, and roles from the DataReader

                // If roles were specified, turn the list into a string array
                string[] rolelist = null;
                if (!String.IsNullOrEmpty(roles))
                    rolelist = roles.Split(new char[] { ',', ';' }, 512);

                // Create a SiteMapNode
                SiteMapNode node = new SiteMapNode(this, id, url, name, description, rolelist, null, null, null);

                _nodes.Add(id, node);

                // Return the node
                return node;
            }
        }
    }
}
