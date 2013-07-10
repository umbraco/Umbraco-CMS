using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Routing;
using Umbraco.Core;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.helpers;
using umbraco.cms.presentation.Trees;
using umbraco.controls.Tree;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataConverter
    {

        /// <summary>
        /// Gets the menu item collection from a legacy tree node based on it's parent node's child collection
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <param name="xmlTree">The node collection that contains the node id</param>
        /// <returns></returns>
        internal static MenuItemCollection ConvertFromLegacyMenu(string nodeId, XmlTree xmlTree)
        {
            var xmlTreeNode = xmlTree.treeCollection.FirstOrDefault(x => x.NodeID == nodeId);
            if (xmlTreeNode == null)
            {
                return null;
            }

            return ConvertFromLegacyMenu(xmlTreeNode);
        }

        /// <summary>
        /// Gets the menu item collection from a legacy tree node
        /// </summary>
        /// <param name="xmlTreeNode"></param>
        /// <returns></returns>
        internal static MenuItemCollection ConvertFromLegacyMenu(XmlTreeNode xmlTreeNode)
        {
            var collection = new MenuItemCollection();

            var menuItems = xmlTreeNode.Menu.ToArray();
            var numAdded = 0;
            var seperators = new List<int>();
            foreach (var t in menuItems)
            {
                if (t is ContextMenuSeperator && numAdded > 0)
                {
                    //store the index for which the seperator should be placed
                    seperators.Add(collection.Count());
                }
                else
                {
                    collection.AddMenuItem(t);
                    numAdded++;
                }
            }
            var length = collection.Count();
            foreach (var s in seperators)
            {
                if (length >= s)
                {
                    collection.ElementAt(s).Seperator = true;
                }
            }

            return collection;
        }

        internal static TreeNode ConvertFromLegacy(string parentId, XmlTreeNode xmlTreeNode, UrlHelper urlHelper, bool isRoot = false)
        {
            //  /umbraco/tree.aspx?rnd=d0d0ff11a1c347dabfaa0fc75effcc2a&id=1046&treeType=content&contextMenu=false&isDialog=false

            //we need to convert the node source to our legacy tree controller
            var childNodesSource = urlHelper.GetUmbracoApiService<LegacyTreeApiController>("GetNodes");

            var childQuery = (xmlTreeNode.Source.IsNullOrWhiteSpace() || xmlTreeNode.Source.IndexOf('?') == -1)
                ? ""
                : xmlTreeNode.Source.Substring(xmlTreeNode.Source.IndexOf('?'));
            
            //append the query strings
            childNodesSource = childNodesSource.AppendQueryStringToUrl(childQuery);

            //for the menu source we need to detect if this is a root node since we'll need to set the parentId and id to -1
            // for which we'll handle correctly on the server side.            
            var menuSource = urlHelper.GetUmbracoApiService<LegacyTreeApiController>("GetMenu");
            menuSource = menuSource.AppendQueryStringToUrl(new[]
                {
                    "id=" + (isRoot ? "-1" : xmlTreeNode.NodeID),
                    "treeType=" + xmlTreeNode.TreeType,
                    "parentId=" + (isRoot ? "-1" : parentId)
                });

            //TODO: Might need to add stuff to additional attributes

            var node = new TreeNode(xmlTreeNode.NodeID, childNodesSource, menuSource)
            {
                HasChildren = xmlTreeNode.HasChildren,
                Icon = xmlTreeNode.Icon,
                Title = xmlTreeNode.Text,
                NodeType = xmlTreeNode.NodeType
                
            };

            //This is a special case scenario, we know that content/media works based on the normal Belle routing/editing so we'll ensure we don't
            // pass in the legacy JS handler so we do it the new way, for all other trees (Currently, this is a WIP), we'll render
            // the legacy js callback,.
            var knownNonLegacyNodeTypes = new[] {"content", "contentRecycleBin", "mediaRecyleBin", "media"};
            if (knownNonLegacyNodeTypes.InvariantContains(xmlTreeNode.NodeType) == false)
            {
                node.OnClickCallback = xmlTreeNode.Action;
            }
            return node;
        }

        internal static TreeNodeCollection ConvertFromLegacy(string parentId, XmlTree xmlTree, UrlHelper urlHelper)
        {
            //TODO: Once we get the editor URL stuff working we'll need to figure out how to convert 
            // that over to use the old school ui.xml stuff for these old trees and however the old menu items worked.

            var collection = new TreeNodeCollection();
            foreach (var x in xmlTree.treeCollection)
            {
                collection.Add(ConvertFromLegacy(parentId, x, urlHelper));
            }
            return collection;
        }

    }
}