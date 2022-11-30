using System.Runtime.Serialization;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Core.Models.Trees;

/// <summary>
///     A tree node that represents various types of root nodes
/// </summary>
/// <remarks>
///     <para>
///         A <see cref="TreeRootNode" /> represents:
///         * The root node for a section containing a single tree
///         * The root node for a section containing multiple sub-trees
///         * The root node for a section containing groups of multiple sub-trees
///         * The group node in a section containing groups of multiple sub-trees
///     </para>
///     <para>
///         This is required to return the tree data for a given section. Some sections may only contain one tree which
///         means it's section
///         root should also display a menu, whereas other sections have multiple trees and the section root shouldn't
///         display a menu.
///     </para>
///     <para>
///         The root node also contains an explicit collection of children.
///     </para>
/// </remarks>
[DataContract(Name = "node", Namespace = "")]
public sealed class TreeRootNode : TreeNode
{
    private static readonly string RootId = Constants.System.RootString;
    private bool _isGroup;
    private bool _isSingleNodeTree;

    /// <summary>
    ///     Private constructor
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="getChildNodesUrl"></param>
    /// <param name="menuUrl"></param>
    private TreeRootNode(string nodeId, string? getChildNodesUrl, string? menuUrl)
        : base(nodeId, null, getChildNodesUrl, menuUrl) =>

        // default to false
        IsContainer = false;

    /// <summary>
    ///     Will be true if this is a multi-tree section root node (i.e. contains other trees)
    /// </summary>
    [DataMember(Name = "isContainer")]
    public bool IsContainer { get; private set; }

    /// <summary>
    ///     True if this is a group root node
    /// </summary>
    [DataMember(Name = "isGroup")]
    public bool IsGroup
    {
        get => _isGroup;
        private set
        {
            // if a group is true then it is also a container
            _isGroup = value;
            IsContainer = true;
        }
    }

    /// <summary>
    ///     True if this root node contains group root nodes
    /// </summary>
    [DataMember(Name = "containsGroups")]
    public bool ContainsGroups { get; private set; }

    /// <summary>
    ///     The node's children collection
    /// </summary>
    [DataMember(Name = "children")]
    public TreeNodeCollection? Children { get; private set; }

    /// <summary>
    ///     Returns true if there are any children
    /// </summary>
    /// <remarks>
    ///     This is used in the UI to configure a full screen section/app
    /// </remarks>
    [DataMember(Name = "containsTrees")]
    public bool ContainsTrees => Children?.Count > 0 || !_isSingleNodeTree;

    /// <summary>
    ///     Creates a group node for grouped multiple trees
    /// </summary>
    public static TreeRootNode CreateGroupNode(TreeNodeCollection children, string section)
    {
        var sectionRoot = new TreeRootNode(RootId, string.Empty, string.Empty)
        {
            IsGroup = true,
            Children = children,
            RoutePath = section,
        };

        return sectionRoot;
    }

    /// <summary>
    ///     Creates a section root node for grouped multiple trees
    /// </summary>
    /// <param name="children"></param>
    /// <returns></returns>
    public static TreeRootNode CreateGroupedMultiTreeRoot(TreeNodeCollection children)
    {
        var sectionRoot = new TreeRootNode(RootId, string.Empty, string.Empty)
        {
            IsContainer = true,
            Children = children,
            ContainsGroups = true,
        };

        return sectionRoot;
    }

    /// <summary>
    ///     Creates a section root node for non-grouped multiple trees
    /// </summary>
    /// <param name="children"></param>
    /// <returns></returns>
    public static TreeRootNode CreateMultiTreeRoot(TreeNodeCollection children)
    {
        var sectionRoot = new TreeRootNode(RootId, string.Empty, string.Empty)
        {
            IsContainer = true,
            Children = children,
        };

        return sectionRoot;
    }

    /// <summary>
    ///     Creates a section root node for a section with a single tree
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="getChildNodesUrl"></param>
    /// <param name="menuUrl"></param>
    /// <param name="title"></param>
    /// <param name="children"></param>
    /// <param name="isSingleNodeTree"></param>
    /// <returns></returns>
    public static TreeRootNode CreateSingleTreeRoot(string nodeId, string? getChildNodesUrl, string? menuUrl, string? title, TreeNodeCollection? children, bool isSingleNodeTree = false) =>
        new TreeRootNode(nodeId, getChildNodesUrl, menuUrl)
        {
            Children = children,
            Name = title,
            _isSingleNodeTree = isSingleNodeTree,
        };
}
