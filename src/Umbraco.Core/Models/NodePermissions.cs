namespace Umbraco.Cms.Core.Models;

/// <summary>
/// A model representing a set of permissions for a given node.
/// </summary>
public class NodePermissions
{
    /// <summary>
    ///     Gets or sets the unique identifier of the node.
    /// </summary>
    public Guid NodeKey { get; set; }

    /// <summary>
    ///     Gets or sets the collection of permission identifiers assigned to this node.
    /// </summary>
    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}
