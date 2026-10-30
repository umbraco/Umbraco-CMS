namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Describes which schema operations are permitted.
/// </summary>
public class ServerSchemaLockdownResponseModel
{
    /// <summary>
    /// Gets or sets the permitted operations per restricted entity type, named in lower case. Entity types not listed
    /// are permitted every operation.
    /// </summary>
    public IEnumerable<ServerSchemaLockdownEntityTypeResponseModel> RestrictedEntityTypes { get; set; } = [];
}
