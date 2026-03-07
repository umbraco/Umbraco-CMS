namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

/// <summary>
/// Represents the request model used to query the origin node of a dynamic root in the Umbraco CMS management API.
/// </summary>
public class DynamicRootQueryOriginRequestModel
{
    /// <summary>
    /// Gets or sets the alias that identifies the origin of the dynamic root query.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the dynamic root query origin.
    /// </summary>
    public Guid? Id { get; set; }
}
