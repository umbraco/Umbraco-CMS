namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the data required to copy an element via the Management API, including the target location.
/// </summary>
public class CopyElementRequestModel
{
    /// <summary>
    /// Gets or sets the target location, specified by ID, where the element will be copied.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
