namespace Umbraco.Cms.Api.Management.ViewModels.Segment;

public class SegmentResponseModel
{
    public required string Name { get; set; } = string.Empty;

    public required string Alias { get; set; } = string.Empty;

    [Obsolete("This property is temporary. A more permanent solution will follow. Scheduled for removal in Umbraco 20.")]
    public IEnumerable<string>? Cultures { get; set; } = null;
}
