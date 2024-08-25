namespace Umbraco.Cms.Core.Models;

/// <summary>
/// A model representing a set of permissions for a given node.
/// </summary>
public class NodePermissions
{
    public Guid NodeKey { get; set; }

    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}
