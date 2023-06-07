using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that allows developer to modify the root tree node that is being rendered
/// </summary>
public class RootNodeRenderingNotification : INotification
{
    public RootNodeRenderingNotification(TreeNode node, FormCollection queryString, string treeAlias)
    {
        Node = node;
        QueryString = queryString;
        TreeAlias = treeAlias;
    }

    /// <summary>
    ///     The root node being rendered
    /// </summary>
    public TreeNode Node { get; }

    /// <summary>
    ///     The alias of the tree the menu is rendering for
    /// </summary>
    public string TreeAlias { get; }

    /// <summary>
    ///     The query string of the current request
    /// </summary>
    public FormCollection QueryString { get; }
}
