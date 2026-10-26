namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// The kind of change an endpoint makes to a schema entity.
/// </summary>
public enum SchemaOperation
{
    /// <summary>
    /// The endpoint could not be classified. Denied on every entity type the restrictions speak to, so it fails closed.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The endpoint does not modify the entity.
    /// </summary>
    Read = 1,

    /// <summary>
    /// The endpoint creates a new entity.
    /// </summary>
    Create = 2,

    /// <summary>
    /// The endpoint modifies an existing entity.
    /// </summary>
    Update = 3,

    /// <summary>
    /// The endpoint removes an existing entity.
    /// </summary>
    Delete = 4,
}
