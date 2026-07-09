namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

/// <summary>
/// Represents the data required for a dynamic root request in the Umbraco CMS Management API.
/// </summary>
public class DynamicRootRequestModel
{
    /// <summary>
    /// Gets or sets the context in which the dynamic root request is made.
    /// </summary>
    public required DynamicRootContextRequestModel Context { get; set; }

    /// <summary>
    /// Gets or sets the query details used to determine the dynamic root.
    /// </summary>
    public required DynamicRootQueryRequestModel Query { get; set; }
}
