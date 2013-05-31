using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataAdapter
    {

        internal static TreeNodeCollection ConvertFromLegacy(XmlTree xmlTree)
        {
            //TODO: Once we get the editor URL stuff working we'll need to figure out how to convert 
            // that over to use the old school ui.xml stuff for these old trees and however the old menu items worked.

            var collection = new TreeNodeCollection();
            foreach (var x in xmlTree.treeCollection)
            {
                var node = new TreeNode(x.NodeID, x.Source)
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