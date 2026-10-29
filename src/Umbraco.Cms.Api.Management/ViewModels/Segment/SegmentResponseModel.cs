namespace Umbraco.Cms.Api.Management.ViewModels.Segment;

/// <summary>
/// Represents a response model containing information about a segment returned by the management API.
/// </summary>
public class SegmentResponseModel
{
    /// <summary>
    /// Gets or sets the name of the segment.
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias of the segment.
    /// </summary>
    public required string Alias { get; set; } = string.Empty;
}
