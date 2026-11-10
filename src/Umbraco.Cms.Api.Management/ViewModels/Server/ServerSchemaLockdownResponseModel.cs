namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Describes which permissions schema lockdown denies.
/// </summary>
public class ServerSchemaLockdownResponseModel
{
    /// <summary>
    /// Gets or sets the denied permission verbs, for example <c>Umb.DocumentType.Create</c>. Any verb not listed is
    /// permitted.
    /// </summary>
    public IEnumerable<string> DeniedPermissions { get; set; } = [];
}
