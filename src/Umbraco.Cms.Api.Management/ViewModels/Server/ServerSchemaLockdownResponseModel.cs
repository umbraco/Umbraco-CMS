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
