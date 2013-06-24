using System;
using System.Linq;
using System.Web.Http.Routing;
using Umbraco.Core;
using umbraco.BusinessLogic.Actions;
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

        internal static TreeNode ConvertFromLegacy(XmlTreeNode xmlTreeNode, UrlHelper urlHelper)
        {
            //  /umbraco/tree.aspx?rnd=d0d0ff11a1c347dabfaa0fc75effcc2a&id=1046&treeType=content&contextMenu=false&isDialog=false

            //we need to convert the node source to our legacy tree controller
            var source = urlHelper.GetUmbracoApiService<LegacyTreeApiController>("GetNodes");
            //append the query strings
            var query = xmlTreeNode.Source.IsNullOrWhiteSpace()
                ? new string[] { }
                : xmlTreeNode.Source.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            source += query.Length > 1 ? query[1].EnsureStartsWith('?') : "";

            //TODO: Might need to add stuff to additional attributes
            
            var node = new TreeNode(xmlTreeNode.NodeID, source)
            {
                HasChildren = xmlTreeNode.HasChildren,
                Icon = xmlTreeNode.Icon,
                Title = xmlTreeNode.Text                
            };

            //This is a special case scenario, we know that content/media works based on the normal Belle routing/editing so we'll ensure we don't
            // pass in the legacy JS handler so we do it the new way, for all other trees (Currently, this is a WIP), we'll render
            // the legacy js callback,.
            var knownNonLegacyNodeTypes = new[] {"content", "contentRecycleBin", "mediaRecyleBin", "media"};
            if (knownNonLegacyNodeTypes.InvariantContains(xmlTreeNode.NodeType) == false)
            {
                node.OnClickCallback = xmlTreeNode.Action;
            }

            var menuItems = xmlTreeNode.Menu.ToArray();
            var numAdded = 0;
            foreach (var t in menuItems)
            {
                if (t is ContextMenuSeperator && numAdded > 0)
                {
                    //if it is a seperator, then update the previous menu item that we've added to be flagged
                    //with a seperator
                    node.Menu.ElementAt(numAdded - 1).Seperator = true;
                }
                else
                {
                    node.AddMenuItem(t);
                    numAdded++;   
                }
            }

            return node;
        }

        internal static TreeNodeCollection ConvertFromLegacy(XmlTree xmlTree, UrlHelper urlHelper)
        {
            //TODO: Once we get the editor URL stuff working we'll need to figure out how to convert 
            // that over to use the old school ui.xml stuff for these old trees and however the old menu items worked.

            var collection = new TreeNodeCollection();
            foreach (var x in xmlTree.treeCollection)
            {                                
                collection.Add(ConvertFromLegacy(x, urlHelper));
            }
            return collection;
        }

    }
}