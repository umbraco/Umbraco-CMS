namespace Umbraco.Cms.Core.Models;

/// <summary>
/// A model representing a set of permissions for a given node.
/// </summary>
public class NodePermissions
{
    public Guid NodeKey { get; set; }

    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}
