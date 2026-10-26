namespace Umbraco.Cms.Api.Management.ViewModels.Server;

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
