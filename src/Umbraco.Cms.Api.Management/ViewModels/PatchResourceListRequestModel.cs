namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Request model to patch a resourceList inside another resource.
/// </summary>
public class PatchResourceListRequestModel
{
    /// <summary>
    /// The identifiers of the top level resources to patch.
    /// </summary>
    public required ISet<Guid> Resources { get; set; }

    /// <summary>
    /// The identifiers of the resources to add to the list.
    /// </summary>
    public required ISet<Guid> Post { get; set; }

    /// <summary>
    /// The identifiers of the resources to remove from the list.
    /// </summary>
    public required ISet<Guid> Delete { get; set; }
}
