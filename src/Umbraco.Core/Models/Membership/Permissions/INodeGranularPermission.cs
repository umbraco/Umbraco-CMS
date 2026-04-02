namespace Umbraco.Cms.Core.Models.Membership.Permissions;

/// <summary>
///     Defines the contract for a granular permission that is associated with a specific node.
/// </summary>
public interface INodeGranularPermission : IGranularPermission
{
    /// <summary>
    ///     Gets or sets the unique key of the node this permission applies to.
    /// </summary>
    new Guid Key { get; set; }

    /// <inheritdoc />
    Guid? IGranularPermission.Key
    {
        get => Key;
    }
}
