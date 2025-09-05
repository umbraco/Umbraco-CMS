namespace Umbraco.Cms.Api.Management.ViewModels.Segment;

public class SegmentResponseModel
{
    public required string Name { get; set; } = string.Empty;

    public required string Alias { get; set; } = string.Empty;

    [Obsolete("This property is temporary and will be removed in a future release (planned for v20). A more permanent solution will follow.")]
    public IEnumerable<string>? Cultures { get; set; } = null;
}
