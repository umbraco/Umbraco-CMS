using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Segment;

public class SegmentResponseModel
{
    public required string Name { get; set; } = string.Empty;

    public required string Alias { get; set; } = string.Empty;
}
