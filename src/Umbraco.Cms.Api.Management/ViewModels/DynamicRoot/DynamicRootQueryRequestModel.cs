namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

/// <summary>
/// Represents the model for a request to query dynamic root content items in the API.
/// </summary>
public class DynamicRootQueryRequestModel
{
    /// <summary>
    /// Gets or sets the origin parameters used to determine the starting point for the dynamic root query.
    /// </summary>
    public required DynamicRootQueryOriginRequestModel Origin { get; set; }

    /// <summary>
    /// Gets or sets the collection of steps that define the dynamic root query.
    /// Each step represents a part of the query logic.
    /// </summary>
    public required IEnumerable<DynamicRootQueryStepRequestModel> Steps { get; set; }
}
