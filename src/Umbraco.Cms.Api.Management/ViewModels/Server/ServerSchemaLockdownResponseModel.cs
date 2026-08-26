namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Describes which schema operations are permitted.
/// </summary>
public class ServerSchemaLockdownResponseModel
{
    /// <summary>
    /// Gets or sets the permitted operations per entity type.
    /// </summary>
    /// <remarks>
    /// Only entity types at least one operation is denied on are listed, and their entity types are lower case.
    /// Anything absent is permitted every operation.
    /// </remarks>
    public IEnumerable<ServerSchemaLockdownEntityTypeResponseModel> RestrictedEntityTypes { get; set; } = [];
}

/// <summary>
/// The operations permitted on a single entity type.
/// </summary>
public class ServerSchemaLockdownEntityTypeResponseModel
{
    /// <summary>Gets or sets the entity type.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether creating is permitted.</summary>
    public bool Create { get; set; }

    /// <summary>Gets or sets a value indicating whether updating is permitted.</summary>
    public bool Update { get; set; }

    /// <summary>Gets or sets a value indicating whether deleting is permitted.</summary>
    public bool Delete { get; set; }
}
