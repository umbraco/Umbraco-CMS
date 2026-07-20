namespace Umbraco.Cms.Core.Models.Membership.Permissions;

/// <summary>
///     Defines the contract for a granular permission that can be assigned to user groups.
/// </summary>
public interface IGranularPermission
{
    /// <summary>
    ///     Gets the context type identifier for this permission.
    /// </summary>
    public string Context { get; }

    /// <summary>
    ///     Gets the unique key associated with this permission, if applicable.
    /// </summary>
    public Guid? Key => null;

    /// <summary>
    ///     Gets or sets the permission identifier.
    /// </summary>
    public string Permission { get; set; }
}
