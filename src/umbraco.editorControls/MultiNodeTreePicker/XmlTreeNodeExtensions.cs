using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco.cms.presentation.Trees;

namespace umbraco.editorControls.MultiNodeTreePicker
{
    /// <summary>
    /// XmlTreeNode extensions for the MultiNodeTreePicker.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public static class XmlTreeNodeExtensions
    {
        //public static void DetermineSelected(this XmlTreeNode node)
        //{
        //}

        /// <summary>
        /// Determines if the node should be clickable based on the xpath given
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="type">The type.</param>
        /// <param name="xml">The XML.</param>
        public static void DetermineClickable(this XmlTreeNode node, string xpath, XPathFilterType type, XElement xml)
        {
            if (!string.IsNullOrEmpty(xpath))
            {
                try
                {
                    var matched = xml.XPathSelectElements(xpath);
                    if (matched.Count() > 0)
                    {
                        if (type == XPathFilterType.Disable)
                        {
                            //add the non-clickable color to the node
                            node.Style.AddCustom("uc-treenode-noclick");
                        }
                        else
                        {
                            //add the non-clickable color to the node
                            node.Style.AddCustom("uc-treenode-click");
                        }
                    }
                    else
                    {
                        if (type == XPathFilterType.Disable)
                        {
                            //ensure the individual node is the correct color
                            node.Style.AddCustom("uc-treenode-click");
                        }
                        else
                        {
                            //ensure the individual node is the correct color
                            node.Style.AddCustom("uc-treenode-noclick");
                        }
                    }
                }
                catch (XPathException)
                {
                    node.Text = "umbraco.editorControls: MNTP: XPath Error!";
                }
            }
            else
            {
                if (type == XPathFilterType.Disable)
                {
                    //ensure the individual node is the correct color
                    node.Style.AddCustom("uc-treenode-click");
                }
                else
                {
                    //ensure the individual node is the correct color
                    node.Style.AddCustom("uc-treenode-noclick");
                }
            }
        }
    }
}
