using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// A special tree node that represents the section root node for any section.
    /// </summary>
    /// <remarks>
    /// This is required to return the tree data for a given section. Some sections may only contain one tree which means it's section
    /// root should also display a menu, whereas other sections have multiple trees and the section root shouldn't display a menu.
    /// 
    /// The section root also contains an explicit collection of children.
    /// </remarks>
    [DataContract(Name = "node", Namespace = "")]
    public sealed class SectionRootNode : TreeNode
    {
        public static SectionRootNode CreateMultiTreeSectionRoot(string nodeId, TreeNodeCollection children)
        {
           var sectionRoot = new SectionRootNode(nodeId, "", "")
                {
                    IsContainer = true,
                    Children = children
                };

            //some metadata as to whether or not this section contains any trees
            sectionRoot.AdditionalData["containsTrees"] = children.Any();

            return sectionRoot;
        }

        public static SectionRootNode CreateSingleTreeSectionRoot(string nodeId, string getChildNodesUrl, string menuUrl, string title, TreeNodeCollection children)
        {
            return new SectionRootNode(nodeId, getChildNodesUrl, menuUrl)
            {
                Children = children,
                Name = title
            };
        }

        private SectionRootNode(string nodeId, string getChildNodesUrl, string menuUrl)
            : base(nodeId, null, getChildNodesUrl, menuUrl)
        {
            //default to false
            IsContainer = false;
        }
        
        [DataMember(Name = "isContainer")]
        public bool IsContainer { get; private set; }

        [DataMember(Name = "children")]
        public TreeNodeCollection Children { get; private set; }
    }
}