using System;
using System.Web.Http.Routing;
using Umbraco.Core;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataAdapter
    {

        internal static TreeNodeCollection ConvertFromLegacy(XmlTree xmlTree, UrlHelper urlHelper)
        {
            //TODO: Once we get the editor URL stuff working we'll need to figure out how to convert 
            // that over to use the old school ui.xml stuff for these old trees and however the old menu items worked.

            var collection = new TreeNodeCollection();
            foreach (var x in xmlTree.treeCollection)
            {
                //  /umbraco/tree.aspx?rnd=d0d0ff11a1c347dabfaa0fc75effcc2a&id=1046&treeType=content&contextMenu=false&isDialog=false

                //we need to convert the node source to our legacy tree controller
                var source = urlHelper.GetUmbracoApiService<LegacyTreeApiController>("GetNodes");
                //append the query strings
                var query = x.Source.IsNullOrWhiteSpace()
                    ? new string[] { }
                    : x.Source.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                source += query.Length > 1 ? query[1].EnsureStartsWith('?') : "";

                var node = new TreeNode(x.NodeID, source)
                    {
                        HasChildren = x.HasChildren,
                        Icon = x.Icon,
                        Title = x.Text
                    };
                //TODO: Might need to add stuff to additional attributes
                collection.Add(node);
            }
            return collection;
        }

    }
}