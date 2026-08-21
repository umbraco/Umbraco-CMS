namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Describes which schema operations the current configuration permits.
/// </summary>
public class ServerSchemaLockdownResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether any operation on any entity type is blocked.
    /// </summary>
    /// <remarks>
    /// Derived from <see cref="EntityTypes"/> rather than from configuration, so it also reports lockdown that a
    /// configurator introduced without the top-level setting being on. It is a summary for presentation only -
    /// <see cref="EntityTypes"/> is what any allow/deny decision must be taken from.
    /// </remarks>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the permitted operations per entity type.
    /// </summary>
    public IEnumerable<ServerSchemaLockdownEntityTypeResponseModel> EntityTypes { get; set; } = [];
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
