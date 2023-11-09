namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Request model to patch a resourceList inside another resource.
/// </summary>
public class PutResourceListRequestModel
{
    /// <summary>
    /// The identifiers of the top level resources to patch.
    /// </summary>
    public required ISet<Guid> Resources { get; set; }

    /// <summary>
    /// The list of identifiers to apply onto the specified resources
    /// </summary>
    public required ISet<Guid> List { get; set; }
}
