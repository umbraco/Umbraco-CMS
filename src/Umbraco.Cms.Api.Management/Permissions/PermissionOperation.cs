namespace Umbraco.Cms.Api.Management.Permissions;

/// <summary>
/// The change an endpoint makes to a schema entity.
/// </summary>
internal enum PermissionOperation
{
    /// <summary>
    /// The endpoint creates a new entity.
    /// </summary>
    Create = 1,

    /// <summary>
    /// The endpoint modifies an existing entity.
    /// </summary>
    Update = 2,

    /// <summary>
    /// The endpoint removes an existing entity.
    /// </summary>
    Delete = 3,
}
