using System.Runtime.Serialization;

namespace Umbraco.Web.Trees
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
        public SectionRootNode(string nodeId, string menuUrl)
            : base(nodeId, string.Empty, menuUrl)
        {
            //default to false
            IsContainer = false;
        }
        
        [DataMember(Name = "isContainer")]
        public bool IsContainer { get; set; }

        [DataMember(Name = "children")]
        public TreeNodeCollection Children { get; set; }
    }
}