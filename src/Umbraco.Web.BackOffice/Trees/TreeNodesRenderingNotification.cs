using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that allows developers to modify the tree node collection that is being rendered
/// </summary>
/// <remarks>
///     Developers can add/remove/replace/insert/update/etc... any of the tree items in the collection.
/// </remarks>
public class TreeNodesRenderingNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeNodesRenderingNotification" /> class.
    /// </summary>
    /// <param name="nodes">The tree nodes being rendered</param>
    /// <param name="queryString">The query string of the current request</param>
    /// <param name="treeAlias">The alias of the tree rendered</param>
    /// <param name="id">The id of the node rendered</param>
    public TreeNodesRenderingNotification(TreeNodeCollection nodes, FormCollection queryString, string treeAlias,
        string id)
    {
        Nodes = nodes;
        QueryString = queryString;
        TreeAlias = treeAlias;
        Id = id;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeNodesRenderingNotification" /> class.
    ///     Constructor
    /// </summary>
    /// <param name="nodes">The tree nodes being rendered</param>
    /// <param name="queryString">The query string of the current request</param>
    /// <param name="treeAlias">The alias of the tree rendered</param>
    [Obsolete("Use ctor with all parameters")]
    public TreeNodesRenderingNotification(TreeNodeCollection nodes, FormCollection queryString, string treeAlias)
    {
        Nodes = nodes;
        QueryString = queryString;
        TreeAlias = treeAlias;
        Id = default;
    }

    /// <summary>
    ///     Gets the tree nodes being rendered
    /// </summary>
    public TreeNodeCollection Nodes { get; }

    /// <summary>
    ///     Gets the query string of the current request
    /// </summary>
    public FormCollection QueryString { get; }

    /// <summary>
    ///     Gets the alias of the tree rendered
    /// </summary>
    public string TreeAlias { get; }

    /// <summary>
    ///     Gets the id of the node rendered
    /// </summary>
    public string? Id { get; }
}
